using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Fmk.RoslynCop.Common.Ordering {

    /// <summary>
    /// Comparateur d'accessibilité.
    /// </summary>
    public class AccessibilityComparer : IComparer<ISymbol> {

        /// <inheritdoc cref="IComparer{T}.Compare" />
        public int Compare(ISymbol x, ISymbol y) {
            var valeurs = new Dictionary<Accessibility, int>
            {
                { Accessibility.NotApplicable, 0 },
                { Accessibility.Private, 1 },
                { Accessibility.Protected, 2 },
                { Accessibility.ProtectedAndInternal, 3 },
                { Accessibility.ProtectedOrInternal, 3 },
                { Accessibility.Internal, 4 },
                { Accessibility.Public, 5 }
            };

            var xEstExplicite = (x as IMethodSymbol)?.MethodKind == MethodKind.ExplicitInterfaceImplementation;
            var yEstExplicite = (y as IMethodSymbol)?.MethodKind == MethodKind.ExplicitInterfaceImplementation;

            if (xEstExplicite || yEstExplicite) {
                return xEstExplicite && y.DeclaredAccessibility == Accessibility.Public
                    || yEstExplicite && x.DeclaredAccessibility == Accessibility.Public
                    || xEstExplicite && yEstExplicite
                    ? 0
                    : xEstExplicite ? -1 : 1;
            }

            return valeurs[x.DeclaredAccessibility] > valeurs[y.DeclaredAccessibility] ? -1
                 : valeurs[x.DeclaredAccessibility] < valeurs[y.DeclaredAccessibility] ? 1
                 : 0;
        }
    }
}