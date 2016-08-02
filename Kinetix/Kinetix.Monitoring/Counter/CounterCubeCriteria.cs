using System;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Critère de lecture d'un hypercube.
    /// </summary>
    internal sealed class CounterCubeCriteria {
        private readonly TimeLevel _level;
        private readonly string _axis;

        /// <summary>
        /// Crée un nouveau critère.
        /// </summary>
        /// <param name="axis">Nom de l'axe de lecture.</param>
        /// <param name="level">Intervale de temps à lire.</param>
        internal CounterCubeCriteria(string axis, TimeLevel level) {
            this._axis = axis;
            this._level = level;
        }

        /// <summary>
        /// Retourne le nom de l'axe de lecture.
        /// </summary>
        internal string Axis {
            get {
                return (_axis == null) ? "global" : _axis;
            }
        }

        /// <summary>
        /// Retourne l'intervale de temps à lire.
        /// </summary>
        internal TimeLevel Level {
            get {
                return _level;
            }
        }

        /// <summary>
        /// Retourne une clef de lecture pour l'axe et l'intervale de temps.
        /// </summary>
        /// <param name="date">Date de la lecture.</param>
        /// <returns>Clef de lecture.</returns>
        internal CubeKey CreateCubeKey(DateTime date) {
            return new CubeKey(date, this.Axis, _level);
        }
    }
}
