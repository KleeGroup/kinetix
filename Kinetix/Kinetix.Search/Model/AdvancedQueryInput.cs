using System.Collections.Generic;
using Kinetix.ComponentModel.Search;

namespace Kinetix.Search.Model {

    /// <summary>
    /// Entrée complète d'une recherche avancée.
    /// </summary>
    public class AdvancedQueryInput {

        /// <summary>
        /// Entrée de l'API.
        /// </summary>
        public QueryInput ApiInput {
            get;
            set;
        }

        /// <summary>
        /// Définition de la recherhe à facette.
        /// </summary>
        public FacetQueryDefinition FacetQueryDefinition {
            get;
            set;
        }

        /// <summary>
        /// Filtrage de sécurité.
        /// </summary>
        public string Security {
            get;
            set;
        }

        /// <summary>
        /// Portefeuille de l'utilisateur.
        /// </summary>
        public string Portfolio {
            get;
            set;
        }

        /// <summary>
        /// La liste des filtres clé/valeur.
        /// </summary>
        public IDictionary<string, string> FilterList {
            get;
            set;
        }
    }
}
