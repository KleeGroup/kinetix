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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReplaceImplementationWithContractFix))]
    [Shared]
    public class ReplaceImplementationWithContractFix : CodeFixProvider {

        private const string Title = "Remplacer avec le contrat {0}";

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(FRC1100_DoNotDependOnServiceImplementationAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {

            /* Récupèrer le node du paramètre. */
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var paramNode = root.FindNode(diagnosticSpan).Parent as ParameterSyntax;
            if (paramNode == null) {
                return;
            }

            /* Retrouver le type du paramètre */
            var currentType = paramNode.Type;
            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            var currentTypeInfo = semanticModel.GetTypeInfo(currentType);

            /* Trouver les contrats de service candidats pour le fix. */
            var candidates = currentTypeInfo.Type.AllInterfaces.Where(x => x.IsServiceContract());

            /* Enregistrer les fix de remplacement */
            foreach (var candidate in candidates) {
                var titleFormat = string.Format(Title, candidate.Name);
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: titleFormat,
                        createChangedDocument: c => ReplaceWithType(context.Document, paramNode, c, candidate),
                        equivalenceKey: titleFormat),
                    diagnostic);
            }
        }

        private static async Task<Document> ReplaceWithType(Document document, ParameterSyntax paramNode, CancellationToken cancellationToken, INamedTypeSymbol replacedType) {

            /* Créé un node pour le nouveau type. */
            var newTypeSyntax = SyntaxFactory.IdentifierName(
                SyntaxFactory.Identifier(replacedType.Name));

            /* Remplace le type du paramètre. */
            var newParamNode = paramNode.WithType(newTypeSyntax);

            // TODO : gérer le trivia.

            // Replace the old local declaration with the new local declaration.
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(paramNode, newParamNode);

            /* Ajoute le using. */
            var replacedTypeNamespace = replacedType.ContainingNamespace.ToString();
            newRoot = newRoot.AddUsing(replacedTypeNamespace);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}