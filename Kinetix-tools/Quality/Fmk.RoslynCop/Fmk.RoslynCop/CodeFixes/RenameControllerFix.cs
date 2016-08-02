using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fmk.RoslynCop.Common;
using Fmk.RoslynCop.Diagnostics.Design;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.CodeFixes {

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RenameControllerFix)), Shared]
    public class RenameControllerFix : CodeFixProvider {

        private const string title = "Renommer en {0}";

        private static readonly Regex ServiceContractNamePattern = new Regex(@"^IService(.*)$");

        public sealed override ImmutableArray<string> FixableDiagnosticIds {
            get { return ImmutableArray.Create(FRC1503_ControllerNamingAnalyser.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {

            /* Récupère le node de field. */
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var node = root.FindNode(diagnosticSpan).AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (node == null) {
                return;
            }

            var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
            var namedTypeSymbol = semanticModel.GetDeclaredSymbol(node, context.CancellationToken) as INamedTypeSymbol;
            if (namedTypeSymbol == null) {
                return;
            }

            if (!namedTypeSymbol.IsApiController()) {
                return;
            }

            /* Vérifie qu'on a un unique constructeur. */
            var ctrList = namedTypeSymbol.Constructors;
            if (ctrList.Length != 1) {
                return;
            }

            /* Vérifie que le constructeur n'a qu'un seul paramètre. */
            var ctr = ctrList.First();
            var paramList = ctr.Parameters;
            if (paramList.Length != 1) {
                return;
            }

            /* Vérifie que le paramètre est un contrat de service. */
            var namedParamType = paramList.First().Type as INamedTypeSymbol;
            if (namedParamType == null) {
                return;
            }
            if (!namedParamType.IsServiceContract()) {
                return;
            }

            /* Calcul du nom */
            var contractName = namedParamType.Name;
            if (!ServiceContractNamePattern.IsMatch(contractName)) {
                return;
            }
            var newName = ServiceContractNamePattern.Replace(contractName, "$1Controller");

            /* Ajoute le fix. */
            var titleFormat = string.Format(title, newName);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: titleFormat,
                    createChangedSolution: c => context.RenameSymbol(namedTypeSymbol, newName, c),
                    equivalenceKey: titleFormat),
                diagnostic);
        }
    }
}