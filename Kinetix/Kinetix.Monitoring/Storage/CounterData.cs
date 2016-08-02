using System;
using System.Collections.Generic;

namespace Kinetix.Monitoring.Storage {
    /// <summary>
    /// Données d'un compteur.
    /// </summary>
    public class CounterData {

        private readonly ICollection<CounterSampleData> _sample = new List<CounterSampleData>();

        /// <summary>
        /// Clef représentative du module auquel appartient le compteur.
        /// </summary>
        public object ModuleKey {
            get;
            set;
        }

        /// <summary>
        /// Base de données.
        /// </summary>
        public string DatabaseName {
            get;
            set;
        }

        /// <summary>
        /// Code du compteur.
        /// </summary>
        public string CounterCode {
            get;
            set;
        }

        /// <summary>
        /// Libellé du compteur.
        /// </summary>
        public string CounterLabel {
            get;
            set;
        }

        /// <summary>
        /// Date de début.
        /// </summary>
        public DateTime StartDate {
            get;
            set;
        }

        /// <summary>
        /// Durée.
        /// </summary>
        public string Level {
            get;
            set;
        }

        /// <summary>
        /// Axe.
        /// </summary>
        public string Axis {
            get;
            set;
        }

        /// <summary>
        /// Nombre de hits.
        /// </summary>
        public double Hits {
            get;
            set;
        }

        /// <summary>
        /// Dernière valeur.
        /// </summary>
        public double Last {
            get;
            set;
        }

        /// <summary>
        /// Valeur maximum.
        /// </summary>
        public double Max {
            get;
            set;
        }

        /// <summary>
        /// Valeur minimum.
        /// </summary>
        public double Min {
            get;
            set;
        }

        /// <summary>
        /// Total des valeurs.
        /// </summary>
        public double Total {
            get;
            set;
        }

        /// <summary>
        /// Total du carré des valeurs.
        /// </summary>
        public double TotalOfSquares {
            get;
            set;
        }

        /// <summary>
        /// Nom associé à la valeur minimum.
        /// </summary>
        public string MinName {
            get;
            set;
        }

        /// <summary>
        /// Nom associé à la valeur maximum.
        /// </summary>
        public string MaxName {
            get;
            set;
        }

        /// <summary>
        /// Temps passé dans les sous processus.
        /// </summary>
        public double SubAvg {
            get;
            set;
        }

        /// <summary>
        /// Echantillonnage associé à la collection.
        /// </summary>
        public ICollection<CounterSampleData> Sample {
            get {
                return _sample;
            }
        }
    }
}
