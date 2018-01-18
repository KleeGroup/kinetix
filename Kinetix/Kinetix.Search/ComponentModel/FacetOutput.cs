using System.Collections.Generic;

namespace Kinetix.Search.ComponentModel {

    /// <summary>
    /// Facette en sortie de recherche avancée : liste de facettes.
    /// </summary>
    public class FacetOutput {

        /// <summary>
        /// Code de la facette.
        /// </summary>
        public string Code {
            get;
            set;
        }

        /// <summary>
        /// Libellé de la facette.
        /// </summary>
        public string Label {
            get;
            set;
        }

        /// <summary>
        /// Facettes multi sélectionnable.
        /// </summary>
        public bool? IsMultiSelectable {
            get;
            set;
        }

        /// <summary>
        /// Valeurs possibles des facettes.
        /// </summary>
        public ICollection<FacetItem> Values {
            get;
            set;
        }
    }
}
