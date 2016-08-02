using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fmk.RoslynCop.CodeFixes.TestGenerator;
using Fmk.RoslynCop.CodeFixes.TestGenerator.Dto;
using Fmk.RoslynCop.CodeFixes.TestGenerator.Templates;
using Fmk.RoslynCop.Common;
using Fmk.RoslynCop.Diagnostics.Coverage;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.CodeFixes {

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReplaceImplementationWithContractFix))]
    [Shared]
    public class AddDalUnitTestFix : CodeFixProvider {

        private const string TitleSemantic = "Ajouter un test unitaire sémantique";
        private const string TitleStandard = "Ajouter un test unitaire standard";

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(FRC1300_DalMethodWithSqlServerCommandAnalyser.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(context.Span);

            /* Filtre sur les méthodes publiques. */
            var methDecl = node as MethodDeclarationSyntax;
            if (methDecl == null || !methDecl.IsPublic()) {
                return;
            }

            /* Vérifie qu'on est dans une implémentation de DAL. */
            var classDecl = methDecl.Parent as ClassDeclarationSyntax;
            if (classDecl == null) {
                return;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDecl, context.CancellationToken);
            if (!classSymbol.IsDalImplementation()) {
                return;
            }

            var document = context.Document;
            var solution = document.Project.Solution;

            /* Dossier du document. */
            var classTestDir = classDecl.GetClassName() + "Test";

            /* Nom du document. */
            var methTestFile = methDecl.GetMethodName() + "Test.cs";

            /* Trouve le projet de test. */
            var testProjetName = document.Project.Name + ".Test";
            Project testProject = solution.Projects.FirstOrDefault(x => x.Name == testProjetName);
            if (testProject == null) {
                return;
            }

            /* Vérifie si le test n'existe pas déjà. */
            var folders = new List<string> { classTestDir };
            var hasTest = testProject.Documents.Any(x =>
                x.Name == methTestFile &&
                x.Folders.SequenceEqual(folders));
            if (hasTest) {
                return;
            }

            /* Obtient le symbole de la méthode. */
            var methSymbol = semanticModel.GetDeclaredSymbol(methDecl, context.CancellationToken);
            if (methSymbol == null) {
                return;
            }

            /* Ajoute une action pour l'ajout du test unitaire sémantique. */
            context.RegisterCodeFix(
                    CodeAction.Create(
                        title: TitleSemantic,
                        createChangedSolution: c => Task.FromResult(AddUnitTestAsync(context.Document, methDecl, methSymbol, classDecl, DalTestStrategy.Semantic, c)),
                        equivalenceKey: TitleSemantic),
                    context.Diagnostics.First());

            /* Ajoute une action pour l'ajout du test unitaire standard. */
            context.RegisterCodeFix(
                    CodeAction.Create(
                        title: TitleStandard,
                        createChangedSolution: c => Task.FromResult(AddUnitTestAsync(context.Document, methDecl, methSymbol, classDecl, DalTestStrategy.Standard, c)),
                        equivalenceKey: TitleStandard),
                    context.Diagnostics.First());
        }

        /// <summary>
        /// Renvoie une expression de valeur factice pour un type donné.
        /// </summary>
        /// <param name="typeSymbol">Type.</param>
        /// <returns>Expression de la valeur factice.</returns>
        private static string GetDummyValue(ITypeSymbol typeSymbol) {
            string fullName = typeSymbol?.ToString();
            if (string.IsNullOrEmpty(fullName)) {
                return "null";
            }

            switch (fullName) {
                case "System.Int32":
                case "int":
                case "int?":
                    return "Dum.Id";
                case "System.Guid":
                case "Guid":
                case "Guid?":
                    return "Dum.Guid";
                case "System.String":
                case "string":
                    return "Dum.Code";
                case "System.DateTime":
                case "DateTime":
                case "DateTime?":
                    return "Dum.Date";
                case "System.Decimal":
                case "decimal":
                case "decimal?":
                    return "Dum.Montant";
                case "System.Boolean":
                case "bool":
                case "bool?":
                    return "Dum.Booleen";
                case "int[]":
                    return "Dum.IdArray";
                case "string[]":
                    return "Dum.StringArray";
                default:
                    /* Cas d'un nullable. */
                    if (fullName.StartsWith("System.Nullable", System.StringComparison.Ordinal)) {
                        var nullableType = (INamedTypeSymbol)typeSymbol;
                        var innerType = nullableType.TypeArguments.First();
                        return GetDummyValue(innerType);
                    }

                    /* Cas d'une collection. */
                    if (fullName.StartsWith("System.Collections.Generic.ICollection", System.StringComparison.Ordinal)) {
                        var collectionType = (INamedTypeSymbol)typeSymbol;
                        var innerType = collectionType.TypeArguments.First();
                        switch (innerType.ToString()) {
                            case "System.String":
                                return "Dum.StringList";
                            case "System.Int32":
                                return "Dum.IdList";
                        }

                        if (innerType.IsReferenceType) {
                            return $"Dum.DumColl<{innerType.Name}>()";
                        }
                    }

                    /* Cas d'un bean. */
                    if (typeSymbol.IsReferenceType) {
                        return $"Dum.Dum<{typeSymbol.Name}>()";
                    }

                    return "null";
            }
        }

        /// <summary>
        /// Renvoie les usings nécessaire pour l'appel du paramètre.
        /// </summary>
        /// <param name="typeSymbol">Type.</param>
        /// <returns>Expression de la valeur factice.</returns>
        private static ICollection<string> GetNameSpaces(ITypeSymbol typeSymbol) {
            string fullName = typeSymbol.ToString();
            switch (fullName) {
                case "System.Int32":
                case "System.String":
                case "System.DateTime":
                case "System.Boolean":
                    return new List<string>();
                default:
                    /* Cas d'une collection de beans. */
                    /* Cas d'une collection. */
                    if (fullName.StartsWith("System.Collections.Generic.ICollection", System.StringComparison.Ordinal)) {
                        var collectionType = (INamedTypeSymbol)typeSymbol;
                        var innerType = collectionType.TypeArguments.First();
                        switch (innerType.ToString()) {
                            case "System.String":
                            case "System.Int32":
                                return new List<string>();
                        }

                        if (innerType.IsReferenceType) {
                            return new List<string> { innerType.ContainingNamespace.ToString() };
                        }
                    }

                    /* Cas d'un bean. */
                    if (typeSymbol.IsReferenceType && typeSymbol.ContainingNamespace != null) {
                        return new List<string> { typeSymbol.ContainingNamespace.ToString() };
                    }

                    return new List<string>();
            }
        }

        /// <summary>
        /// Construit un paramètre de méthode de DAL.
        /// </summary>
        /// <param name="paramSymbol">Définition du paramètre.</param>
        /// <returns>Parémètre.</returns>
        private static DalMethodParam GetParameter(IParameterSymbol paramSymbol) {
            return new DalMethodParam {
                Name = paramSymbol.Name,
                Value = GetDummyValue(paramSymbol.Type),
                SpecificUsings = GetNameSpaces(paramSymbol.Type)
            };
        }

        private Solution AddUnitTestAsync(Document document, MethodDeclarationSyntax methDecl, IMethodSymbol methSymbol, ClassDeclarationSyntax classDecl, DalTestStrategy strategy, CancellationToken cancellationToken) {

            var solution = document.Project.Solution;

            /* Trouver le projet de test. */
            var testProjetName = document.Project.Name + ".Test";
            var testProject = solution.Projects.FirstOrDefault(x => x.Name == testProjetName);
            if (testProject == null) {
                return solution;
            }

            /* Dossier du document. */
            var classTestDir = classDecl.GetClassName() + "Test";

            /* Nom du document. */
            var methTestFile = methSymbol.Name + "Test";

            /* Contenu du document. */
            var classSymbol = methSymbol.ContainingType;
            var applicationName = classSymbol.ContainingAssembly.GetApplicationName();
            DalTestTemplate template = new DalTestTemplate(new DalMethodItem {
                DalClassName = classSymbol.Name,
                DalMethodName = methSymbol.Name,
                DalAssemblyName = classSymbol.ContainingAssembly.Name,
                DalNamespace = classDecl.GetNameSpaceFullName(),
                Params = methSymbol.Parameters.Select(x => GetParameter(x)).ToList(),
                SpecificUsings = new List<string> { $"{applicationName}.Business.Common.Test" }
            });
            string content = template.Render(strategy);

            /* Création du document. */
            var newDoc = testProject.AddDocument(methTestFile, content, new List<string> { classTestDir });

            /* Retourne la solution modifiée. */
            return newDoc.Project.Solution;
        }
    }
}