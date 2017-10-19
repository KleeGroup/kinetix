using System;
using System.IO;
using System.Linq;
using System.Text;
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
            var solution = msWorkspace.OpenSolutionAsync(args[0]).Result;
            Generate(solution, args[1], args[2]).Wait();
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
                File.WriteAllText(fileName, output, Encoding.UTF8);
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

            return new ServiceDeclaration {
                Verb = method.AttributeLists.First().Attributes.First().ToString() == "HttpGet" ? Verb.Get : Verb.Post,
                Route = ((method.AttributeLists.Last().Attributes.First().ArgumentList.ChildNodes().First() as AttributeArgumentSyntax)
                    .Expression as LiteralExpressionSyntax).Token.ValueText,
                Name = method.Identifier.ToString(),
                ReturnType = model.GetSymbolInfo(method.ReturnType).Symbol as INamedTypeSymbol,
                Parameters = method.ParameterList.ChildNodes().OfType<ParameterSyntax>().Select(parameter => new Parameter {
                    Type = model.GetSymbolInfo(parameter.Type).Symbol as INamedTypeSymbol,
                    Name = parameter.Identifier.ToString(),
                    IsOptional = parameter.Default != null
                }).ToList(),
                Documentation = new Documentation { Summary = summary, Parameters = parameters.ToList() }
            };
        }
    }
}
