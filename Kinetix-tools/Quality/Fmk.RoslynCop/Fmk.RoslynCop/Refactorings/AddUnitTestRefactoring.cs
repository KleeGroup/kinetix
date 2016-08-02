using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.Refactorings {

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(AddUnitTestRefactoring)), Shared]
    internal class AddUnitTestRefactoring : CodeRefactoringProvider {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context) {

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
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
            var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
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

            /* Ajoute une action pour l'ajout du test unitaire. */
            var action = CodeAction.Create("Ajouter un test unitaire", c => ReverseTypeNameAsync(context.Document, methDecl, classDecl, c));
            context.RegisterRefactoring(action);
        }

        private async Task<Solution> ReverseTypeNameAsync(Document document, MethodDeclarationSyntax methDecl, ClassDeclarationSyntax classDecl, CancellationToken cancellationToken) {

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
            var methTestFile = methDecl.GetMethodName() + "Test";

            /* Contenu du document. */
            const string content = "namespace TestUnitaire {}"; // TODO

            /* Création du document. */
            var newDoc = testProject.AddDocument(methTestFile, content, new List<string> { classTestDir });

            /* Retourne la solution modifiée. */
            return newDoc.Project.Solution;
        }
    }
}