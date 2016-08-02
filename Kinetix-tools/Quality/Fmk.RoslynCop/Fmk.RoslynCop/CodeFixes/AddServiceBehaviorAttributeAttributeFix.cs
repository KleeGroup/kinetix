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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddServiceBehaviorAttributeAttributeFix))]
    [Shared]
    public class AddServiceBehaviorAttributeAttributeFix : CodeFixProvider {

        private const string Title = "Décorer avec ServiceBehavior";

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get {
                return ImmutableArray.Create(
                FRC1107_ServiceImplementationClassDecorationAnalyser.DiagnosticId,
                FRC1108_DalImplementationClassDecorationAnalyser.DiagnosticId);
            }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var classDecl = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDecl == null) {
                return;
            }

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => AddAttributeAsync(context.Document, classDecl, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> AddAttributeAsync(Document document, ClassDeclarationSyntax classDecl, CancellationToken cancellationToken) {

            /* Créé l'attribut. */
            var newAttrList = SyntaxFactory.AttributeList(
                SyntaxFactory.SeparatedList(new[] { SyntaxFactory.Attribute(
                        SyntaxFactory.IdentifierName(
                            SyntaxFactory.Identifier("ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)")))
                }));

            /* Récupère le trivia du premier token. */
            var initFirstToken = classDecl.GetFirstToken();
            var initLeadingTrivia = initFirstToken.LeadingTrivia;

            /* Enlève le trivia du premier token. */
            var newClassSyntax = classDecl.ReplaceToken(
                initFirstToken,
                initFirstToken.WithLeadingTrivia(SyntaxFactory.Whitespace("\n")));

            /* Injecte le trivia sur le nouvel attribut. */
            newAttrList = newAttrList.WithLeadingTrivia(initLeadingTrivia);

            /* Ajoute l'attribut à la classe. */
            var newAttrLists = newClassSyntax.AttributeLists.Insert(0, newAttrList);
            newClassSyntax = newClassSyntax.WithAttributeLists(newAttrLists);

            /* Remplace la classe. */
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(classDecl, newClassSyntax);

            /* Ajoute le using. */
            newRoot = newRoot.AddUsing(FrameworkNames.SystemServiceModel);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}