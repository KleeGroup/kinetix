using System.Collections.Immutable;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie que les classes d'implémentation d'un module métier ne sont pas utilisées dans un autre module.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1101_DoNotUseBusinessImplementationAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1101";
        private const string Category = "Design";
        private static readonly string Description = "L'implémentation d'un module métier ne peut être consommé que via un contrat de service.";
        private static readonly string MessageFormat = "L'objet {0} ne doit pas être utilisé en dehors de son assemblée.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        private static readonly string Title = "Ne pas utiliser les implémentations d'une autre assemblée";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            /* Analyse les identifiants. */
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, ImmutableArray.Create(SyntaxKind.IdentifierName));
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {

            /* Obtient le node d'identifiant. */
            var identifier = context.Node as IdentifierNameSyntax;
            if (identifier == null) {
                return;
            }

            /* Récupère le symbole du type. */
            var identifierSymbol = context.SemanticModel.GetSymbolInfo(identifier, context.CancellationToken).Symbol;
            if (identifierSymbol == null) {
                return;
            }

            /* Récupère le type contenant l'identifiant. */
            var containingNode = identifier.FirstAncestorOrSelf<TypeDeclarationSyntax>();
            if (containingNode == null) {
                return;
            }

            /* Récupère le symbole du type contenant. */
            var currentNodeSymbol = context.SemanticModel.GetDeclaredSymbol(containingNode, context.CancellationToken);
            /* Récupère les assemblées. */
            var referenceAssembly = identifierSymbol.ContainingAssembly;
            var currentAssembly = currentNodeSymbol.ContainingAssembly;
            /* Compare les assemblées. */
            if (referenceAssembly != currentAssembly &&
                referenceAssembly.IsBusinessImplementationAssembly() &&
                currentAssembly.IsBusinessImplementationAssembly()) {
                /* Assemblée d'implémentation différentes. */
                var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), identifierSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
