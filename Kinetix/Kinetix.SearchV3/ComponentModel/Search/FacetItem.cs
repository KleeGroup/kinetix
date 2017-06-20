namespace Kinetix.ComponentModel.SearchV3 {

    /// <summary>
    /// Item de facette.
    /// </summary>
    public struct FacetItem {

        /// <summary>
        /// Code de la facette.
        /// </summary>
        public string Code {
            get;
            set;
        }

        /// <summary>
        /// Libellé de l'item.
        /// </summary>
        public string Label {
            get;
            set;
        }

        /// <summary>
        /// Nombre d'éléments pour l'item.
        /// </summary>
        public long Count {
            get;
            set;
        }
    }
}
