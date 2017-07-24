using System.Collections.Generic;

namespace Kinetix.ComponentModel.Search {

    /// <summary>
    /// Sortie d'une recherche avancée.
    /// </summary>
    public class QueryOutput {

        /// <summary>
        /// Groupe de liste de résultats (cas d'une recherche avec groupe).
        /// </summary>
        public ICollection<GroupResult> Groups {
            get;
            set;
        }

        /// <summary>
        /// Facettes sélectionnées.
        /// </summary>
        public ICollection<FacetOutput> Facets {
            get;
            set;
        }

        /// <summary>
        /// Nombre total d'éléments.
        /// </summary>
        public long? TotalCount {
            get;
            set;
        }

        /// <summary>
        /// Rappel de l'entrée de la recherche.
        /// </summary>
        public QueryInput Query {
            get;
            set;
        }
    }
}
