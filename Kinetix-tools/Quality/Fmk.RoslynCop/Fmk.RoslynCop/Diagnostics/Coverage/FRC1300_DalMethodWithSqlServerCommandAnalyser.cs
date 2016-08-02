using System.Collections.Immutable;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Coverage {

    /// <summary>
    /// Détecte les méthodes de DAL utilisant SqlServerCommand.
    /// Ce diagnostic est Hidden car il ne correspond pas à un warning.
    /// Le CodeFix AddDalUnitTestRefactoringFix utilise ce diagnostic pour proposer l'ajout du test unitaire s'il manque.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1300_DalMethodWithSqlServerCommandAnalyser : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1300";
        private const string Category = "Coverage";
        private static readonly string Description = "Méthode de DAL avec SqlServerCommand.";
        private static readonly string MessageFormat = "La méthode utilise SqlServerCommand ou GetBroker.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description, DiagnosticSeverity.Hidden);

        private static readonly string Title = "Méthode de DAL avec SqlServerCommand ou GetBroker.";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            /* Analyse les identifiants. */
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, ImmutableArray.Create(SyntaxKind.ClassDeclaration));
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {

            /* Obtient le node de déclaration de classe. */
            var classDecl = context.Node as ClassDeclarationSyntax;
            if (classDecl == null) {
                return;
            }

            /* Récupère le symbole du type. */
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl, context.CancellationToken);
            if (classSymbol == null) {
                return;
            }

            /* Vérifie que la classe est une DAL. */
            if (!classSymbol.IsDalImplementation()) {
                return;
            }

            /* Visite de la classe. */
            var walker = new MethodWithSqlServerCommandWalker(context);
            walker.Visit(classDecl);
        }

        private class MethodWithSqlServerCommandWalker : CSharpSyntaxWalker {

            private readonly SyntaxNodeAnalysisContext _context;
            private MethodDeclarationSyntax _currentMethDecl;

            public MethodWithSqlServerCommandWalker(SyntaxNodeAnalysisContext context) {
                _context = context;
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node) {
                var symbol = _context.SemanticModel.GetSymbolInfo(node, _context.CancellationToken).Symbol;
                if (symbol != null) {
                    switch (symbol.Name) {
                        case "GetSqlServerCommand":
                        case "GetBroker":
                            /* La méthode est candidate au test unitaire de DAL : on créé le diagnostic. */
                            var diagnostic = Diagnostic.Create(Rule, _currentMethDecl.GetMethodLocation());
                            _context.ReportDiagnostic(diagnostic);
                            /* On arrête l'analyse et on sort. */
                            return;
                    }
                }

                /* Symbole de DAL non trouvé : on continue à analyser la méthode. */
                base.VisitInvocationExpression(node);
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node) {
                _currentMethDecl = node;
                base.VisitMethodDeclaration(node);
            }
        }
    }
}