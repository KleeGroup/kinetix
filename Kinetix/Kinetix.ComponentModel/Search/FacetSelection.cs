namespace Kinetix.ComponentModel.Search {

    /// <summary>
    /// Sélection de facette pour une entrée de recherche.
    /// </summary>
    public class FacetSelection {

        /// <summary>
        /// Nom de la facette.
        /// </summary>
        public string Key {
            get;
            set;
        }

        /// <summary>
        /// Valeur sélectionnée de la facette.
        /// </summary>
        public string Value {
            get;
            set;
        }
    }
}
