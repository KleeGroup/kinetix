namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Interface de définition d'un compteur.
    /// </summary>
    public interface ICounterDefinition {

        /// <summary>
        /// Libellé à afficher pour cette CounterDefinition.
        /// </summary>
        string Label {
            get;
        }

        /// <summary>
        /// Code dans le cache pour cette CounterDefinition.
        /// </summary>
        string Code {
            get;
        }

        /// <summary>
        /// Priorité d'affichage du compteur.
        /// </summary>
        int Priority {
            get;
        }

        /// <summary>
        /// Warning level.
        /// </summary>
        long WarningThreshold {
            get;
        }

        /// <summary>
        /// Critical level.
        /// </summary>
        long CriticalThreshold {
            get;
        }
    }
}
