using System;
using System.Collections.Generic;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Comparateur pour une collection d'ensemble de grandeurs, sort précise la
    /// grandeur qui sert au tri.
    /// </summary>
    internal class HyperCubeComparer : IComparer<string> {
        private readonly string _counterDefinitionCode;
        private readonly CounterStatType _statType;
        private readonly TimeLevel _level;
        private readonly DateTime _date;
        private readonly IHyperCube _hyperCube;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="hyperCube">HyperCube utilisé par le comparateur.</param>
        /// <param name="date">Date à la quelle se fait la comparaison.</param>
        /// <param name="counterDefinitionCode">Compteur à comparer.</param>
        /// <param name="statType">Type de statistique à comparer (min, max, moy, ...).</param>
        /// <param name="level">Période de temps sur laquelle appliquer la comparaison.</param>
        internal HyperCubeComparer(IHyperCube hyperCube, DateTime date, string counterDefinitionCode, CounterStatType statType, TimeLevel level) {

            this._counterDefinitionCode = counterDefinitionCode;
            this._date = date;
            this._hyperCube = hyperCube;
            this._level = level;
            this._statType = statType;
        }

        /// <summary>
        /// Compare la valeur de 2 axes d'un cube.
        /// Les valeurs utilisés dépendent des paramètres du constructeur.
        /// </summary>
        /// <param name="x">Premier axe de comparaison.</param>
        /// <param name="y">Second axe de comparaison.</param>
        /// <returns>Résultat de la comparaison des valeurs.</returns>
        int IComparer<string>.Compare(string x, string y) {
            ICube cube1 = _hyperCube.GetCube(new CubeKey(_date, x, _level));
            ICube cube2 = _hyperCube.GetCube(new CubeKey(_date, y, _level));

            double v1 = double.NaN;
            double v2 = double.NaN;

            if (cube1 != null) {
                ICounter counter1 = cube1.GetCounter(_counterDefinitionCode);
                if (counter1 != null) {
                    v1 = counter1.GetValue(_statType);
                }
            }

            if (cube2 != null) {
                ICounter counter2 = cube2.GetCounter(_counterDefinitionCode);
                if (counter2 != null) {
                    v2 = counter2.GetValue(_statType);
                }
            }

            // Tri descendant
            return -v1.CompareTo(v2);
        }
    }
}
