using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Fmk.RoslynCop.Common.Ordering {
    class StaticReadonlyComparer : IComparer<ISymbol> {

        /// <inheritdoc cref="IComparer{T}.Compare" />
        public int Compare(ISymbol x, ISymbol y) {
            return ValeurSymbole(x) > ValeurSymbole(y) ? -1
                 : ValeurSymbole(x) < ValeurSymbole(y) ? 1
                 : 0;
        }

        private int ValeurSymbole(ISymbol x) {
            var valeur = 0;
            if (x.IsStatic)
                valeur += 2;
            if ((x as IFieldSymbol)?.IsReadOnly ?? false)
                valeur++;

            return valeur;
        }
    }
}
