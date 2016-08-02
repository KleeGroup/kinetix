using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.CodeFixes {

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveAttributeFix))]
    [Shared]
    public class RemoveAttributeFix : CodeFixProvider {

        private const string Title = "Supprimer l'attribut";

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(FRC1104_WcfServiceImplementationAnalyser.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<AttributeSyntax>().FirstOrDefault();
            if (declaration == null) {
                return;
            }

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => RemoveAttributeAsync(context.Document, declaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> RemoveAttributeAsync(Document document, AttributeSyntax attrDecl, CancellationToken cancellationToken) {

            MethodDeclarationSyntax newMethodSyntax;

            /* Récupère l'AttributeList. */
            var attrList = attrDecl.Parent as AttributeListSyntax;

            /* Déclaration de la méthode. */
            var methodSyntax = attrList.Parent as MethodDeclarationSyntax;

            if (attrList.Attributes.Count > 1) {

                /* Supprime l'attribut dans l'AttributeList */
                var newAttrs = attrList.Attributes.Remove(attrDecl);
                var newAttrList = attrList.WithAttributes(newAttrs);
                var newAttrLists = methodSyntax.AttributeLists.Replace(attrList, newAttrList);
                newMethodSyntax = methodSyntax.WithAttributeLists(newAttrLists);
            }
            else {

                /* Supprime l'AttributeList */

                /* Vérifie si l'AttributeList à supprimer est le premier node de la méthode. */
                var isFirstNode = methodSyntax.ChildNodes().First() == attrList;

                /* Supprime l'attribute list */
                var newAttrLists = methodSyntax.AttributeLists.Remove(attrList);
                newMethodSyntax = methodSyntax.WithAttributeLists(newAttrLists);

                /* Cas du premier node : on reprend le trivia avec les commentaires. */
                if (isFirstNode) {
                    /* Récupère le leading trivia */
                    var leadingTrivia = attrList.GetFirstToken().LeadingTrivia;
                    /* Reprend le trivia sur le premier token. */
                    var firstToken = newMethodSyntax.GetFirstToken();
                    var newFirstToken = firstToken.WithLeadingTrivia(leadingTrivia);
                    newMethodSyntax = newMethodSyntax.ReplaceToken(firstToken, newFirstToken);
                }
            }

            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(methodSyntax, newMethodSyntax);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}