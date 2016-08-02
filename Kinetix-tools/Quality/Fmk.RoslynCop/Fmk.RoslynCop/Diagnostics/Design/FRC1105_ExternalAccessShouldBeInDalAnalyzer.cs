using System.Collections.Immutable;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie que les accès aux données (SQL / Elastic / etc.) ne se font pas à l'extérieur d'une DAL.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1105_ExternalAccessShouldBeInDalAnalyzer : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1105";
        private const string Category = "Design";
        private static readonly string Description = "Les accès extérieurs doivent être fait dans une DAL.";
        private static readonly string MessageFormat = "L'objet {0} est utilisé dans la classe {1} qui n'est pas une DAL.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        private static readonly string Title = "Les accès extérieurs doivent être fait dans une DAL";

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

            /* Vérifie que la classe n'est pas une DAL. */
            if (classSymbol.IsDalImplementation()) {
                return;
            }

            /* Vérifie que la DAL est une implémentation de service. */
            if (!classSymbol.IsServiceImplementation()) {
                return;
            }

            /* Vérifie que la classe n'est pas dans le Framework. */
            if (classSymbol.IsFramework()) {
                return;
            }

            /* Visite de la classe. */
            var walker = new ExternalAccessWalker(context, classSymbol);
            walker.Visit(classDecl);
        }

        private class ExternalAccessWalker : CSharpSyntaxWalker {
            private readonly INamedTypeSymbol _classSymbol;

            private readonly SyntaxNodeAnalysisContext _context;

            public ExternalAccessWalker(SyntaxNodeAnalysisContext context, INamedTypeSymbol classSymbol) {
                _classSymbol = classSymbol;
                _context = context;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node) {
                var symbol = _context.SemanticModel.GetSymbolInfo(node, _context.CancellationToken).Symbol;
                if (symbol == null) {
                    return;
                }

                if (symbol.Name == "SqlServerCommand") {
                    var diagnostic = Diagnostic.Create(Rule, node.GetLocation(), symbol.Name, _classSymbol.Name);
                    _context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
