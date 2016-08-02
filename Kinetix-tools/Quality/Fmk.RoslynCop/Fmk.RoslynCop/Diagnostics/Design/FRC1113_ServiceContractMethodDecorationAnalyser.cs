using System.Collections.Immutable;
using System.Linq;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie que les méthodes de contrats de service sont décorées avec OperationContract.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1113_ServiceContractMethodDecorationAnalyser : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1113";
        private const string Category = "Design";
        private static readonly string Description = "Les méthodes de contrats de service doivent être décorés OperationContract.";
        private static readonly string MessageFormat = "La méthode {0} contrat de service {1} doit être décorée avec l'attribut WCF OperationContract.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        private static readonly string Title = "Les méthodes de contrats de service doivent être décorés OperationContract.";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            /* Analyse pour les déclaration de classes. */
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InterfaceDeclaration);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {

            /* Vérifie que la classe est un contrat de service. */
            var interfaceNode = context.Node as InterfaceDeclarationSyntax;
            if (interfaceNode == null) {
                return;
            }

            ////if (!interfaceNode.SyntaxTree.IsServiceContractFile()) {
            ////    return;
            ////}
            var interfaceSymbol = context.SemanticModel.GetDeclaredSymbol(interfaceNode, context.CancellationToken);
            if (interfaceSymbol == null) {
                return;
            }

            if (!interfaceSymbol.IsServiceContract()) {
                return;
            }

            /* Parcourt les méthodes */
            foreach (var methDecl in interfaceNode.Members.OfType<MethodDeclarationSyntax>()) {

                /* Vérifie que la méthode est décorée avec OperationContract. */
                var methSymbol = context.SemanticModel.GetDeclaredSymbol(methDecl, context.CancellationToken);
                if (!methSymbol.IsOperationContract()) {

                    /* Créé le diagnostic. */
                    var location = methDecl.GetNameDeclarationLocation();
                    var diagnostic = Diagnostic.Create(Rule, location, methSymbol.Name, interfaceSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
