using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fmk.RoslynCop.Common;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.CodeFixes {

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddHttpVerbAttributeFix))]
    [Shared]
    public class AddHttpVerbAttributeFix : CodeFixProvider {

        private const string Title = "Décorer avec {0}";
        private static readonly string[] HttpVerbAttributes = {
            "HttpGet",
            "HttpPost",
            "HttpPut",
            "HttpDelete"
        };

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(FRC1111_ApiActionShouldBeDecoratedWithHttpVerbAnalyser.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var methDecl = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (methDecl == null) {
                return;
            }

            // Register a code action that will invoke the fix.
            foreach (var httpVerb in HttpVerbAttributes) {
                var titleFormat = string.Format(Title, httpVerb);
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: titleFormat,
                        createChangedDocument: c => RemoveAttributeAsync(context.Document, methDecl, c, httpVerb),
                        equivalenceKey: titleFormat),
                    diagnostic);
            }
        }

        private static async Task<Document> RemoveAttributeAsync(Document document, MethodDeclarationSyntax methDecl, CancellationToken cancellationToken, string attributeName) {

            /* Créé l'attribut. */
            var newAttrList = SyntaxFactory.AttributeList(
                SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Attribute(
                        SyntaxFactory.IdentifierName(
                            SyntaxFactory.Identifier(attributeName)))
                }));

            /* Récupère le trivia du premier token. */
            var initFirstToken = methDecl.GetFirstToken();
            var initLeadingTrivia = initFirstToken.LeadingTrivia;

            /* Enlève le trivia du premier token. */
            var newMethodSyntax = methDecl.ReplaceToken(
                initFirstToken,
                initFirstToken.WithLeadingTrivia(SyntaxFactory.Whitespace("\n")));

            /* Injecte le trivia sur le nouvel attribut. */
            newAttrList = newAttrList.WithLeadingTrivia(initLeadingTrivia);

            /* Ajoute l'attribut à la méthode. */
            var newAttrLists = newMethodSyntax.AttributeLists.Insert(0, newAttrList);
            newMethodSyntax = newMethodSyntax.WithAttributeLists(newAttrLists);

            /* Remplace la méthode. */
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(methDecl, newMethodSyntax);

            /* Ajoute le using. */
            newRoot = newRoot.AddUsing(FrameworkNames.SystemWebHttp);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}