using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.Common.Ordering {

    /// <summary>
    /// Classe regroupant les fonctions partagées entre l'analyseur et le correcteur pour l'ordre dans le constructeur.
    /// </summary>
    public static class ConstructorOrdering {

        /// <summary>
        /// Retourne les assignations dans une listes d'expressions.
        /// </summary>
        /// <param name="expressions">Liste d'expressions.</param>
        /// <param name="modèleSémantique">Modèle sémantique.</param>
        /// <returns>La liste d'assignations.</returns>
        public static IEnumerable<StatementSyntax> TrouverAssignations(SyntaxList<StatementSyntax> expressions, SemanticModel modèleSémantique) =>
            expressions.Where(e => {
                var expression = (e as ExpressionStatementSyntax)?.Expression as AssignmentExpressionSyntax;
                return expression?.Kind() == SyntaxKind.SimpleAssignmentExpression
                    && modèleSémantique.GetSymbolInfo(expression.Left).Symbol?.Kind == SymbolKind.Field
                    && modèleSémantique.GetSymbolInfo(expression.Right).Symbol?.Kind == SymbolKind.Parameter;
            });

        /// <summary>
        /// Retourne les conditions sur les paramètres dans une listes d'expressions.
        /// </summary>
        /// <param name="expressions">Liste d'expressions.</param>
        /// <param name="paramètres">Les paramètres du constructeur.</param>
        /// <param name="modèleSémantique">Modèle sémantique.</param>
        /// <returns>La liste d'assignations.</returns>
        public static IEnumerable<IfStatementSyntax> TrouveConditionsParametres(SyntaxList<StatementSyntax> expressions, ParameterListSyntax paramètres, SemanticModel modèleSémantique) =>
            expressions
                .OfType<IfStatementSyntax>()
                .Where(e =>
                    (e.Condition
                        ?.DescendantNodes()?.OfType<IdentifierNameSyntax>()
                        ?.Any(identifiant => modèleSémantique.GetSymbolInfo(identifiant).Symbol?.Kind == SymbolKind.Parameter)
                    ?? false)
                && (e.Statement
                        ?.DescendantNodes()?.OfType<IdentifierNameSyntax>()
                        ?.All(identifiant => modèleSémantique.GetSymbolInfo(identifiant).Symbol?.Kind != SymbolKind.Field)
                    ?? false));
    }
}
