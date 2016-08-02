using System.Collections.Immutable;
using System.Linq;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie que les implémentations de services implémentent un contrat de service.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1115_ServiceImplementationShouldImplementServiceContractMethodAnalyser : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1115";
        private const string Category = "Design";
        private static readonly string Description = "Les implémentations de services doivent implémenter un contrat de service.";
        private static readonly string MessageFormat = "L'implémentation de service {0} n'implémente aucun contrat de service.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        private static readonly string Title = "Les implémentations de services doivent implémenter un contrat de service.";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            /* Analyse pour les déclaration de classes. */
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {

            /* Vérifie que la classe est une implémentation de service. */
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

            /* Vérifie la présence d'interfaces de services. */
            var hasInterfaces = classSymbol.Interfaces.Any(x => x.IsServiceContract());
            if (hasInterfaces) {
                return;
            }

            /* Créé le diagnostic. */
            var location = classNode.GetNameDeclarationLocation();
            var diagnostic = Diagnostic.Create(Rule, location, classSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
