using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Fmk.RoslynCop.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Diagnostics.Design {

    /// <summary>
    /// Vérifie que le nommage des méthodes de chargement des listes.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FRC1502_LoadListNamingAnalyser : DiagnosticAnalyzer {

        public const string DiagnosticId = "FRC1502";

        private static readonly string Title = "Nommage des méthodes de chargement de liste";
        private static readonly string MessageFormat = "La méthode {1} du service {0} ne suit pas la règle de nommage Load[Nom métier]List[Complément].";
        private static readonly string Description = "Nommage des méthodes de chargement de liste.";
        private const string Category = "Naming";

        private static readonly Regex LoadListPattern = new Regex(@"^Load(.*)List(.*)$");

        private static readonly DiagnosticDescriptor Rule = DiagnosticRuleUtils.CreateRule(DiagnosticId, Title, MessageFormat, Category, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context) {
            /* Analyse pour les déclaration de fields. */
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, ImmutableArray.Create(SyntaxKind.InterfaceDeclaration));
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context) {

            var node = context.Node as InterfaceDeclarationSyntax;
            if (node == null) {
                return;
            }
            var symbol = context.SemanticModel.GetDeclaredSymbol(node, context.CancellationToken);

            /* Vérifie que l'interface est un contrat de service de lecture. */
            if (!symbol.IsServiceContract()) {
                return;
            }

            new MethodsWalker(context, node.GetInterfaceName()).Visit(node);
        }

        private class MethodsWalker : CSharpSyntaxWalker {

            private readonly SyntaxNodeAnalysisContext _context;
            private readonly string _interfaceName;

            public MethodsWalker(SyntaxNodeAnalysisContext context, string interfaceName) {
                _context = context;
                _interfaceName = interfaceName;
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node) {
                /* Vérifie si la méthode est une méthode de chargement. */
                if (!node.GetMethodName().StartsWith("Load", System.StringComparison.OrdinalIgnoreCase)) {
                    return;
                }
                /* Vérifie que la méthode renvoie une ICollection<T>. */
                var returnType = node.ReturnType;
                var typeSymbol = _context.SemanticModel.GetTypeInfo(returnType, _context.CancellationToken).Type;
                if (typeSymbol == null) {
                    return;
                }
                if (typeSymbol.Kind != SymbolKind.NamedType) {
                    return;
                }
                var namedType = typeSymbol as INamedTypeSymbol;
                if (!namedType.IsGenericType || namedType.ConstructedFrom?.ToString() != @"System.Collections.Generic.ICollection<T>") {
                    return;
                }
                var methodName = node.GetMethodName();
                if (LoadListPattern.IsMatch(methodName)) {
                    return;
                }

                var diagnostic = Diagnostic.Create(Rule, node.GetMethodLocation(), _interfaceName, methodName);
                _context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
