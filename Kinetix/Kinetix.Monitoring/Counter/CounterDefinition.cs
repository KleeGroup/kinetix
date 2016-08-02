using System;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Définition d'un compteur.
    /// </summary>
    internal sealed class CounterDefinition : ICounterDefinition, IComparable<CounterDefinition> {
        private readonly string _label;
        private readonly string _code;
        private readonly long _warningThreshold;
        private readonly long _criticalThreshold;
        private readonly int _priority;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="label">Libellé du compteur.</param>
        /// <param name="code">Code du compteur.</param>
        /// <param name="warningThreshold">Warning level.</param>
        /// <param name="criticalThreshold">Critical level.</param>
        /// <param name="priority">Priorité d'affichage du compteur (minimum en premier).</param>
        internal CounterDefinition(string label, string code, long warningThreshold, long criticalThreshold, int priority) {
            this._code = code;
            this._criticalThreshold = criticalThreshold;
            this._label = label;
            this._priority = priority;
            this._warningThreshold = warningThreshold;
        }

        /// <summary>
        /// Warning level.
        /// </summary>
        long ICounterDefinition.WarningThreshold {
            get {
                return _warningThreshold;
            }
        }

        /// <summary>
        /// Critical level.
        /// </summary>
        long ICounterDefinition.CriticalThreshold {
            get {
                return _criticalThreshold;
            }
        }

        /// <summary>
        /// Libellé à afficher pour cette CounterDefinition.
        /// </summary>
        string ICounterDefinition.Label {
            get {
                return this.Label;
            }
        }

        /// <summary>
        /// Code dans le cache pour cette CounterDefinition.
        /// </summary>
        public string Code {
            get {
                return _code;
            }
        }

        /// <summary>
        /// Priorité d'affichage du compteur.
        /// </summary>
        int ICounterDefinition.Priority {
            get {
                return _priority;
            }
        }

        /// <summary>
        /// Libellé à afficher pour cette CounterDefinition.
        /// </summary>
        internal string Label {
            get {
                return _label;
            }
        }

        /// <summary>
        /// Compare deux définitions.
        /// </summary>
        /// <param name="other">Autre définition.</param>
        /// <returns>Résultat de la comparaison.</returns>
        int IComparable<CounterDefinition>.CompareTo(CounterDefinition other) {
            if (other == null) {
                throw new ArgumentNullException("other");
            }

            return string.Compare(_label, other._label, StringComparison.OrdinalIgnoreCase);
        }
    }
}
