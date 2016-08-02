using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.CodeFixes {

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MakeFieldStatelessFix))]
    [Shared]
    public class MakeFieldStatelessFix : CodeFixProvider {

        private const string TitleConst = "Mettre const";
        private const string TitleReadonly = "Mettre readonly";

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(FRC1103_ServiceShouldBeStatelessAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().FirstOrDefault();
            if (declaration == null) {
                return;
            }

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: TitleReadonly,
                    createChangedDocument: c => AddKeywordAsync(context.Document, declaration, c, SyntaxKind.ReadOnlyKeyword),
                    equivalenceKey: TitleReadonly),
                diagnostic);

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: TitleConst,
                    createChangedDocument: c => AddKeywordAsync(context.Document, declaration, c, SyntaxKind.ConstKeyword),
                    equivalenceKey: TitleConst),
                diagnostic);
        }

        private static async Task<Document> AddKeywordAsync(Document document, FieldDeclarationSyntax fieldDecl, CancellationToken cancellationToken, SyntaxKind keyword) {
            var readonlyToken = SyntaxFactory.Token(keyword);

            // Insert the const token into the modifiers list, creating a new modifiers list.
            var newModifiers = fieldDecl.Modifiers.Insert(fieldDecl.Modifiers.Count(), readonlyToken);

            // Produce the new local declaration.
            var newFieldDecl = fieldDecl.WithModifiers(newModifiers);

            // Replace the old local declaration with the new local declaration.
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(fieldDecl, newFieldDecl);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);
        }
    }
}