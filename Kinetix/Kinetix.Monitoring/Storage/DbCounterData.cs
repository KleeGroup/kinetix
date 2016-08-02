namespace Kinetix.Monitoring.Storage {
    /// <summary>
    /// Données d'un compteur.
    /// </summary>
    public class DbCounterData : CounterData {

        /// <summary>
        /// Identifiant du compteur.
        /// </summary>
        public int? CntId {
            get;
            set;
        }

        /// <summary>
        /// Identifiant base de données.
        /// </summary>
        public int CdbId {
            get;
            set;
        }

        /// <summary>
        /// Identifiant du module.
        /// </summary>
        public int ModId {
            get;
            set;
        }

        /// <summary>
        /// Identifiant de l'axe.
        /// </summary>
        public int AxiId {
            get;
            set;
        }

        /// <summary>
        /// Identifiant de l'axe temps.
        /// </summary>
        public int TaxId {
            get;
            set;
        }

        /// <summary>
        /// Identifiant du compteur.
        /// </summary>
        public int? CdfId {
            get;
            set;
        }
    }
}
