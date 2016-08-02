using System.Collections.Immutable;
using System.Linq;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie que les actions des contrôleurs web API sont décorés avec une route explicite.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1112_ApiActionShouldBeDecoratedWithRouteAnalyser : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1112";
        private const string Category = "Design";
        private static readonly string Description = "Les routes des actions de contrôleurs doivent être explicitement déclarées.";
        private static readonly string MessageFormat = "L'action {1} du controller {0} doit être décorée avec une route explicite.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        private static readonly string Title = "L'action doit être décorée avec une route explicite.";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            /* Analyse pour les déclaration de fields. */
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {

            /* Vérifie qu'on est dans un controller Web API. */
            var classNode = context.Node as ClassDeclarationSyntax;
            if (classNode == null) {
                return;
            }

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classNode, context.CancellationToken);
            if (classSymbol == null) {
                return;
            }

            if (!classSymbol.IsApiController()) {
                return;
            }

            /* Parcourt les méthodes */
            var methods = classNode.Members.OfType<MethodDeclarationSyntax>().Where(x => x.IsPublic());
            foreach (var methodNode in methods) {

                /* Vérifie la présence de l'attribut Route. */
                var methSymbol = context.SemanticModel.GetDeclaredSymbol(methodNode, context.CancellationToken);
                if (methSymbol.HasRouteAttribute()) {
                    continue;
                }

                /* Diagnostic l'absence de la route. */
                var controllerName = classNode.GetClassName();
                var location = methodNode.GetMethodLocation();
                var diagnostic = Diagnostic.Create(Rule, location, controllerName, methodNode.GetMethodName());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
