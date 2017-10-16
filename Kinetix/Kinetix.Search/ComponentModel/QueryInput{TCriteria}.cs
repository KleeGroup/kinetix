namespace Kinetix.Search.ComponentModel {

    /// <summary>
    /// Entrée d'une recherche avancée.
    /// </summary>
    /// <typeparam name="TCriteria">Critère.</typeparam>
    public class QueryInput<TCriteria>
        where TCriteria : Criteria {

        /// <summary>
        /// Liste des facettes.
        /// </summary>
        public FacetListInput Facets {
            get;
            set;
        }

        /// <summary>
        /// Critères de recherche.
        /// </summary>
        public TCriteria Criteria {
            get;
            set;
        }

        /// <summary>
        /// Nom du champ pour grouper (parmi les noms de facettes).
        /// </summary>
        public string Group {
            get;
            set;
        }

        /// <summary>
        /// Champ de tri.
        /// </summary>
        public string SortFieldName {
            get;
            set;
        }

        /// <summary>
        /// Indique si le tri est descendant.
        /// </summary>
        public bool SortDesc {
            get;
            set;
        }

        /// <summary>
        /// Nombre d'éléments à sauter pour la pagination.
        /// </summary>
        public int? Skip {
            get;
            set;
        }

        /// <summary>
        /// Taille d'une page d'éléments.
        /// </summary>
        public int? Top {
            get;
            set;
        }
    }
}
