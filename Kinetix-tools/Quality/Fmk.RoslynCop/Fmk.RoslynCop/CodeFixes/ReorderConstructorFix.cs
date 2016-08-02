using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fmk.RoslynCop.Common.Ordering;
using Fmk.RoslynCop.Diagnostics.Maintainability;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.CodeFixes {

    /// <summary>
    /// Correcteur pour OrdreConstructeur.
    /// Permet d'enregistrer les corrections liées aux diagnostics.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReorderConstructorFix))]
    [Shared]
    public class ReorderConstructorFix : CodeFixProvider {

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(FRC1202_ConstructorShouldBeOrdered.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <summary>
        /// Enregistre les corrections de codes.
        /// </summary>
        /// <param name="context">Le contexte.</param>
        /// <returns>Peut être attendu.</returns>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var constructeur = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ConstructorDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Réordonner les assignations",
                    c => OrdonnerAssignations(context.Document, constructeur, c),
                    "Réordonner les assignations"),
                diagnostic);
        }

        /// <summary>
        /// Réordonne les assignations d'un constructeur.
        /// </summary>
        /// <param name="document">Le document.</param>
        /// <param name="constructeur">Le constructeur.</param>
        /// <param name="jetonAnnulation">Le jeton d'annulation.</param>
        /// <returns>Le nouveau document.</returns>
        private async Task<Document> OrdonnerAssignations(Document document, ConstructorDeclarationSyntax constructeur, CancellationToken jetonAnnulation) {
            // On récupère la racine et le modèle sémantique.
            var racine = await document
                .GetSyntaxRootAsync(jetonAnnulation)
                .ConfigureAwait(false);
            var modèleSémantique = await document.GetSemanticModelAsync(jetonAnnulation);

            // On récupère le corps du constructeur.
            var corps = constructeur.ChildNodes().First(nœud => nœud as BlockSyntax != null) as BlockSyntax;

            // On récupère toutes les conditions sur les paramètres.
            var conditions = ConstructorOrdering.TrouveConditionsParametres(corps.Statements, constructeur.ParameterList, modèleSémantique);

            // On récupère les assignations et on les ordonne.
            var assignations = ConstructorOrdering.TrouverAssignations(corps.Statements, modèleSémantique)
                .OrderBy(e => e.ToString());

            // On construit le nouveau corps du constructeur.
            var corpsOrdonné = corps.WithStatements(
                SyntaxFactory.List(
                    conditions
                        .Concat(assignations)
                        .Concat(corps.Statements
                            .Except(conditions)
                            .Except(assignations))));

            // Et on met à jour la racine.
            var nouvelleRacine = racine.ReplaceNode(corps, corpsOrdonné);

            return document.WithSyntaxRoot(nouvelleRacine);
        }
    }
}