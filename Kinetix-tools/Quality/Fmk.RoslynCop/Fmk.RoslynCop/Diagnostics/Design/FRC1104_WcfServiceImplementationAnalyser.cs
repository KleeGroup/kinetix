using System.Collections.Immutable;
using System.Linq;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie que les implémentations de service WCF ne sont pas décorés.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1104_WcfServiceImplementationAnalyser : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1104";
        private const string Category = "Design";
        private static readonly string Description = "Les décorations doivent être portées par le contrat de service.";
        private static readonly string MessageFormat = "La méthode {1} du service {0} ne doit pas être décorée avec l'attribut {2}.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        private static readonly string Title = "Les implémentations de service ne doivent pas être décorés.";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            /* Analyse pour les déclaration de fields. */
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {
            var classNode = context.Node as ClassDeclarationSyntax;
            if (classNode == null) {
                return;
            }

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classNode, context.CancellationToken);
            if (classSymbol == null) {
                return;
            }

            if (!classSymbol.IsServiceImplementation()) {
                return;
            }

            var serviceName = classNode.GetClassName();
            var methods = classNode.Members.OfType<MethodDeclarationSyntax>();
            foreach (var methodNode in methods) {
                var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodNode, context.CancellationToken);
                var attrs = methodNode.AttributeLists.SelectMany(x => x.Attributes);
                foreach (var attr in attrs) {
                    var attrSymbol = context.SemanticModel.GetSymbolInfo(attr, context.CancellationToken);
                    var attrType = attrSymbol.Symbol.ContainingType;
                    if (attrType.IsServiceImplementationAttribute()) {
                        continue;
                    }

                    var attrName = attrSymbol.Symbol.ContainingType.Name;
                    var diagnostic = Diagnostic.Create(Rule, attr.GetLocation(), serviceName, methodSymbol.Name, attrName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
