using System.Collections.Immutable;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie que le champ de service injecté sont nommés comme le service.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1500_ServiceFieldNamingAnalyser : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1500";

        private static readonly string Title = "Nommage du champ de service";
        private static readonly string MessageFormat = "Le champ {1} de la classe {0} doit être nommé {2}.";
        private static readonly string Description = "Renommer le champ comme le service.";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context) {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, ImmutableArray.Create(SyntaxKind.ClassDeclaration));
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {
            var node = context.Node as ClassDeclarationSyntax;
            new FieldWalker(context, node.GetClassName()).Visit(node);
        }

        private class FieldWalker : CSharpSyntaxWalker {

            private readonly SyntaxNodeAnalysisContext _context;
            private readonly string _className;

            public FieldWalker(SyntaxNodeAnalysisContext context, string className) {
                _context = context;
                _className = className;
            }

            public override void VisitFieldDeclaration(FieldDeclarationSyntax node) {
                var namedTypeSymbol = _context.GetNamedSymbol(node.Declaration.Type);
                if (namedTypeSymbol == null) {
                    return;
                }
                if (!namedTypeSymbol.IsServiceContract()) {
                    return;
                }

                var typeName = namedTypeSymbol.Name;
                var isContract = typeName.IsServiceContractName();
                if (!isContract) {
                    return;
                }
                var expectedFieldName = typeName.GetServiceContractFieldName();
                var actualFieldName = node.GetFieldName();
                if (expectedFieldName == actualFieldName) {
                    return;
                }
                var diagnostic = Diagnostic.Create(Rule, node.GetFieldNameLocation(), _className, actualFieldName, expectedFieldName);
                _context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
