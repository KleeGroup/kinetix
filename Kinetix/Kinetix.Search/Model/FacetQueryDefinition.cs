using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Search.Model {

    /// <summary>
    /// Définition d'une recherche à facettes.
    /// </summary>
    public class FacetQueryDefinition {

        /// <summary>
        /// Créé une nouvelle instance de FacetQueryDefinition.
        /// </summary>
        /// <param name="facets">Facettes.</param>
        public FacetQueryDefinition(params IFacetDefinition[] facets) {
            Facets = facets.ToList();
        }

        /// <summary>
        /// Libellé de la valeur de facette nulle.
        /// </summary>
        public string FacetNullValueLabel {
            get;
            set;
        }

        /// <summary>
        /// Liste des facettes.
        /// </summary>
        public ICollection<IFacetDefinition> Facets {
            get;
            private set;
        }
    }
}
