namespace Kinetix.Monitoring.Storage {
    /// <summary>
    /// Classe représentant un élément d'échantillonnage des données.
    /// </summary>
    public class CounterSampleData {

        /// <summary>
        /// Obtient ou définit la valeur d'échantillonnage.
        /// </summary>
        public double SampleValue {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le nombre d'élément correspondant à cette valeur.
        /// </summary>
        public int SampleCount {
            get;
            set;
        }
    }
}
