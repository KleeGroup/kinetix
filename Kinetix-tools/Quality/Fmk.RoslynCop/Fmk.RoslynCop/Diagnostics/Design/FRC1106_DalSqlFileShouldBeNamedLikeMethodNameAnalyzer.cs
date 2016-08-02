using System.Collections.Immutable;
using System.Linq;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie que dans une méthode de DAL, le nom du fichier de la requête correspond au nom de la méthode.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1106_DalSqlFileShouldBeNamedLikeMethodNameAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1106";
        private const string Category = "Design";
        private static readonly string Description = "Le nom d'un fichier SQL de DAL doit être le nom de la méthode de DAL.";
        private static readonly string MessageFormat = "Le nom du fichier SQL {0} n'est pas le même que le nom de la méthode de DAL {1}.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        private static readonly string Title = "Consistence du nom du fichier SQL d'une méthode de DAL";

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
            var walker = new ExternalAccessWalker(context);
            walker.Visit(classDecl);
        }

        private class ExternalAccessWalker : CSharpSyntaxWalker {

            private readonly SyntaxNodeAnalysisContext _context;
            private MethodDeclarationSyntax _currentMethDecl;

            public ExternalAccessWalker(SyntaxNodeAnalysisContext context)
                : base(SyntaxWalkerDepth.Token) {
                _context = context;
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node) {
                var symbol = _context.SemanticModel.GetSymbolInfo(node, _context.CancellationToken).Symbol;
                if (symbol == null) {
                    return;
                }

                if (!(symbol.Name == "GetSqlServerCommand")) {
                    return;
                }

                var scriptArg = node.ArgumentList.Arguments.FirstOrDefault();
                if (scriptArg == null) {
                    return;
                }

                var literalToken = scriptArg
                    .DescendantTokens()
                    .FirstOrDefault(x => x.Kind() == SyntaxKind.StringLiteralToken);
                if (literalToken.Kind() == SyntaxKind.None) {
                    return;
                }

                var literalValue = literalToken.ValueText;
                if (!literalValue.EndsWith(".sql", System.StringComparison.Ordinal)) {
                    return;
                }

                var actualSqlFileName = literalValue.Substring(0, literalValue.Length - ".sql".Length);
                var expectedSqlFileName = _currentMethDecl.GetMethodName();
                if (actualSqlFileName == expectedSqlFileName) {
                    return;
                }

                var diagnostic = Diagnostic.Create(Rule, literalToken.GetLocation(), actualSqlFileName, expectedSqlFileName);
                _context.ReportDiagnostic(diagnostic);
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node) {
                _currentMethDecl = node;
                base.VisitMethodDeclaration(node);
            }
        }
    }
}
