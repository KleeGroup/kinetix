namespace Kinetix.ComponentModel.Search {

    /// <summary>
    /// Sortie d'une recherche avancée sans scope.
    /// Un groupe = un type de document.
    /// </summary>
    public class UnscopedQueryOutput {

        /// <summary>
        /// Groupe de liste de résultats (cas d'une recherche avec groupe).
        /// </summary>
        public UnscopedGroupResultList Groups {
            get;
            set;
        }

        /// <summary>
        /// Facettes sélectionnées.
        /// </summary>
        public FacetListOutput Facets {
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
