using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fmk.RoslynCop.Common;
using Fmk.RoslynCop.Diagnostics.Documentation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Fmk.RoslynCop.CodeFixes {

    /// <summary>
    /// Correcteur pour InheritDoc.
    /// Permet d'enregistrer les corrections liées aux diagnostics.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddInheritDoc))]
    [Shared]
    public class AddInheritDoc : CodeFixProvider {

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(FRC1600_InheritdocIsIncorrect.DiagnosticId); }
        }

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
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    @"Générer le ""inheritDoc""",
                    c => AjouterInheritDoc(context.Document, declaration, c),
                    @"Générer le ""inheritDoc"""),
                diagnostic);
        }

        /// <summary>
        /// Ajoute/corrige la ligne de documentation à/de la méthode.
        /// </summary>
        /// <param name="document">Le document du contexte.</param>
        /// <param name="méthode">La méthode à documenter.</param>
        /// <param name="jetonAnnulation">Le jeton d'annulation.</param>
        /// <returns>Le document mis à jour.</returns>
        private static async Task<Document> AjouterInheritDoc(Document document, MethodDeclarationSyntax méthode, CancellationToken jetonAnnulation) {
            // On récupère la recine et le modèle sémantique.
            var racine = await document
                .GetSyntaxRootAsync(jetonAnnulation)
                .ConfigureAwait(false);
            var modèleSémantique = await document.GetSemanticModelAsync(jetonAnnulation);

            // On a déjà trouvé le inheritDoc dans le diagnostic mais on ne peut pas vraiment le passer au correctif...
            var inheritDoc = Inheritdoc.InheritDocEstCorrect(racine, modèleSémantique, méthode);

            // Ajoute la ligne de commentaire à la méthode.
            var méthodeCommentée = méthode
                .WithLeadingTrivia(SyntaxFactory.LineFeed, SyntaxFactory.Comment(inheritDoc), SyntaxFactory.LineFeed)
                .WithAdditionalAnnotations(Formatter.Annotation);

            // Met à jour la racine.
            var nouvelleRacine = racine.ReplaceNode(méthode, méthodeCommentée);

            return document.WithSyntaxRoot(nouvelleRacine);
        }
    }
}