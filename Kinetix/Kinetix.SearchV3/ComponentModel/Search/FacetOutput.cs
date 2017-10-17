using System.Collections.Generic;

namespace Kinetix.ComponentModel.SearchV3 {

    /// <summary>
    /// Facette en sortie de recherche avancée : liste de facettes.
    /// </summary>
    public class FacetOutput {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public FacetOutput() {
            this.Values = new List<FacetItem>();
        }

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

        public ICollection<FacetItem> Values {
            get;
            protected set;
        }
    }
}
