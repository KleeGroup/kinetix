using System.Collections.Immutable;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Interdit l'usage de Thread.Sleep.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1102_NoThreadSleepAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1102";
        private const string Category = "Design";
        private static readonly string Description = "Supprimer ou remplacer par un WaitHandle.";
        private static readonly string MessageFormat = "Ne pas utiliser Thread.Sleep.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        private static readonly string Title = "Ne pas utiliser Thread.Sleep";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, ImmutableArray.Create(SyntaxKind.SimpleMemberAccessExpression));
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {
            var expression = context.Node as MemberAccessExpressionSyntax;
            if (expression == null) {
                return;
            }

            var methodSymbol = context.SemanticModel.GetSymbolInfo(expression.Name, context.CancellationToken).Symbol;
            if (methodSymbol == null) {
                return;
            }

            if (methodSymbol.ContainingType == null) {
                return;
            }

            if (methodSymbol.ContainingType.ToString() == "System.Threading.Thread" &&
                methodSymbol.Name == "Sleep") {
                var diagnostic = Diagnostic.Create(Rule, expression.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
