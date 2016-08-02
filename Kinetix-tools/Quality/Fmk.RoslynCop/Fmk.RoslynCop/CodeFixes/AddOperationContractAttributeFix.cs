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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.CodeFixes {

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddOperationContractAttributeFix))]
    [Shared]
    public class AddOperationContractAttributeFix : CodeFixProvider {

        private const string Title = "Décorer avec OperationContract";

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get {
                return ImmutableArray.Create(
                FRC1113_ServiceContractMethodDecorationAnalyser.DiagnosticId);
            }
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
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => AddAttributeAsync(context.Document, methDecl, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> AddAttributeAsync(Document document, MethodDeclarationSyntax methDecl, CancellationToken cancellationToken) {

            /* Créé la méthode avec l'attribut. */
            var newMethodSyntax = methDecl.AddAttribute(FrameworkNames.OperationContract);

            /* Remplace la méthode. */
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(methDecl, newMethodSyntax);

            /* Ajoute le using. */
            newRoot = newRoot.AddUsing(FrameworkNames.SystemServiceModel);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}