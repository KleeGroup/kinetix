using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Fmk.RoslynCop.Common {

    /// <summary>
    /// Extensions des contextes d'analyse.
    /// </summary>
    public static class ContextExtensions {

        /// <summary>
        /// Obtient un symbole de type.
        /// </summary>
        /// <param name="context">Contexte.</param>
        /// <param name="typeNode">Node du type.</param>
        /// <returns>Symbole du type.</returns>
        public static ITypeSymbol GetTypeSymbol(this SyntaxNodeAnalysisContext context, TypeSyntax typeNode) {
            return context.SemanticModel.GetTypeInfo(typeNode, context.CancellationToken).Type;
        }

        /// <summary>
        /// Obtient un symbole nommé de type.
        /// </summary>
        /// <param name="context">Contexte.</param>
        /// <param name="typeNode">Node du type.</param>
        /// <returns>Symbole nommé.</returns>
        public static INamedTypeSymbol GetNamedSymbol(this SyntaxNodeAnalysisContext context, TypeSyntax typeNode) {
            var typeSymbol = context.GetTypeSymbol(typeNode);
            return typeSymbol as INamedTypeSymbol;
        }
    }
}
