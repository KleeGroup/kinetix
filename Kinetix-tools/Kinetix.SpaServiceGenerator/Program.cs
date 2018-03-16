using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kinetix.SpaServiceGenerator.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace Kinetix.SpaServiceGenerator {

    /// <summary>
    /// Programme.
    /// </summary>
    internal class Program {

        /// <summary>
        /// Point d'entrée.
        /// </summary>
        /// <param name="args">
        /// Premier argument : chemin de la solution.
        /// Deuxième argument : racine de la SPA.
        /// Troisième argument : namespace du projet (exemple : "Kinetix").
        /// </param>
        public static void Main(string[] args) {
            var msWorkspace = MSBuildWorkspace.Create();
            msWorkspace.WorkspaceFailed += MsWorkspace_WorkspaceFailed;

            var solution = msWorkspace.OpenSolutionAsync(args[0]).Result;
            Generate(solution, args[1], args[2]).Wait();
        }

        private static void MsWorkspace_WorkspaceFailed(object sender, WorkspaceDiagnosticEventArgs e) {
            Console.WriteLine(e.Diagnostic.Message);
        }

        private static async Task Generate(Solution solution, string spaRoot, string projectName) {
            var definitionPath = "../../model";
            var outputPath = $"{spaRoot}/app/services";

            var frontEnd = solution.Projects.First(projet => projet.AssemblyName == $"{projectName}.FrontEnd");
            var controllers = frontEnd.Documents.Where(document =>
                document.Name.Contains("Controller")
                && document.Folders.Contains("Controllers")
                && !document.Folders.Contains("Transverse")
                && document.Folders.Count == 2
                && !document.Name.Contains("ReferenceList"));

            foreach (var controller in controllers) {

                var syntaxTree = await controller.GetSyntaxTreeAsync();
                var controllerClass = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

                var controllerName = $"{controllerClass.Identifier.ToString().Replace("Controller", string.Empty).ToDashCase()}.ts";
                Console.Out.WriteLine($"Generating {controllerName}");

                var methods = controllerClass.ChildNodes().OfType<MethodDeclarationSyntax>();
                var model = await controller.GetSemanticModelAsync();

                var serviceList = methods
                    .Where(method => method.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PublicKeyword)))
                    .Select(method => GetService(method, model));

                var fileName = $"{outputPath}/{controller.Folders.Last().ToDashCase()}/{controllerName}";
                var fileInfo = new FileInfo(fileName);

                var directoryInfo = fileInfo.Directory;
                if (!directoryInfo.Exists) {
                    Directory.CreateDirectory(directoryInfo.FullName);
                }

                var template = new ServiceSpa { ProjectName = projectName, DefinitionPath = definitionPath, Services = serviceList };
                var output = template.TransformText();
                File.WriteAllText(fileName, output, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }
        }

        private static ServiceDeclaration GetService(MethodDeclarationSyntax method, SemanticModel model) {
            var documentation = method.GetLeadingTrivia()
                .First(i => i.GetStructure() is DocumentationCommentTriviaSyntax)
                .GetStructure() as DocumentationCommentTriviaSyntax;

            var summary = (documentation.Content.First(content =>
                content.ToString().StartsWith("<summary>", StringComparison.Ordinal)) as XmlElementSyntax).Content.ToString()
                .Replace("/// ", string.Empty).Replace("\r\n       ", string.Empty).Trim();

            var parameters = documentation.Content.Where(content => content.ToString().StartsWith("<param", StringComparison.Ordinal))
                .Select(param => Tuple.Create(
                    ((param as XmlElementSyntax).StartTag.Attributes.First() as XmlNameAttributeSyntax).Identifier.ToString(),
                    (param as XmlElementSyntax).Content.ToString()));

            var verb = method.AttributeLists.First().Attributes.First().ToString();

            ICollection<Parameter> parameterList = method.ParameterList.ChildNodes().OfType<ParameterSyntax>().Select(parameter => new Parameter {
                Type = model.GetSymbolInfo(parameter.Type).Symbol as INamedTypeSymbol,
                Name = parameter.Identifier.ToString(),
                IsOptional = parameter.Default != null,
                IsFromBody = parameter.AttributeLists.FirstOrDefault() != null ? parameter.AttributeLists.FirstOrDefault().Attributes.Where(attr => attr.ToString() == "FromBody").Any() : false,
                IsFromUri = parameter.AttributeLists.FirstOrDefault() != null ? parameter.AttributeLists.FirstOrDefault().Attributes.Where(attr => attr.ToString() == "FromUri").Any() : false,
            }).ToList();

            IList<string> routeParameters = new List<string>();
            string route = ((method.AttributeLists.Last().Attributes.First().ArgumentList.ChildNodes().First() as AttributeArgumentSyntax)
                    .Expression as LiteralExpressionSyntax).Token.ValueText;
            MatchCollection matches = Regex.Matches(route, "(?s){.+?}");
            foreach (Match match in matches) {
                routeParameters.Add(match.Value.Replace("{", "").Replace("}", ""));
            }

            var uriParameters = parameterList
                .Where(param => !param.IsFromBody && routeParameters.Contains(param.Name))
                .ToList();
            var queryParameters = parameterList
                .Where(param => !param.IsFromBody && !routeParameters.Contains(param.Name))
                .ToList();

            ICollection<Parameter> bodyParameters = new List<Parameter>();
            if ((verb == "HttpPost" || verb == "HttpPut") && parameterList.Except(uriParameters).Any()) {
                var bodyParams = parameterList
                    .Except(uriParameters)
                    .Where(param => param.IsFromBody)
                    // Concat here as a fallback (use of first below)
                    .Concat(parameterList.Where(param => !param.IsFromUri));

                if (bodyParams.Any()) {
                    bodyParameters.Add(bodyParams.First());
                }

                queryParameters = queryParameters
                    .Where(param => !bodyParameters.Select(body => body.Name).Contains(param.Name))
                    .ToList();
            }

            return new ServiceDeclaration {
                Verb = verb,
                Route = route,
                Name = method.Identifier.ToString(),
                ReturnType = model.GetSymbolInfo(method.ReturnType).Symbol as INamedTypeSymbol,
                Parameters = parameterList,
                UriParameters = uriParameters,
                QueryParameters = queryParameters,
                BodyParameters = bodyParameters,
                Documentation = new Documentation { Summary = summary, Parameters = parameters.ToList() },
                IsPostPutMethod = verb == "HttpPost" || verb == "HttpPut"
            };
        }
    }
}
