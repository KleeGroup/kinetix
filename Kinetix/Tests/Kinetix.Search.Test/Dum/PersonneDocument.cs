using Kinetix.Search.ComponentModel;

namespace Kinetix.Search.Test.Dum {

    /// <summary>
    /// Document Elastic d'une personne.
    /// </summary>
    [SearchDocumentType("personne")]
    public class PersonneDocument {

        /// <summary>
        /// ID de la personne.
        /// </summary>
        [SearchField(SearchFieldCategory.Id)]
        public int Id {
            get;
            set;
        }

        /// <summary>
        /// Champ de recherche.
        /// </summary>
        [SearchField(SearchFieldCategory.Search)]
        public string Text {
            get;
            set;
        }

        /// <summary>
        /// Champ de recherche secondaire.
        /// </summary>
        [SearchField(SearchFieldCategory.TextSearch)]
        public string TextSearch
        {
            get;
            set;
        }

        /// <summary>
        /// Nom de la personne.
        /// </summary>
        [SearchField(SearchFieldCategory.TextSearch)]
        public string Nom {
            get;
            set;
        }

        /// <summary>
        /// Nom de la personne (pour le tri).
        /// </summary>
        [SearchField(SearchFieldCategory.Sort)]
        public string NomSort {
            get;
            set;
        }

        /// <summary>
        /// Prénom de la personne.
        /// </summary>
        [SearchField(SearchFieldCategory.Result)]
        public string Prenom {
            get;
            set;
        }

        /// <summary>
        /// Genre de la personne (M ou F).
        /// </summary>
        [SearchField(SearchFieldCategory.Term)]
        public string Genre {
            get;
            set;
        }

        /// <summary>
        /// Liste des départements de la personne.
        /// </summary>
        [SearchField(SearchFieldCategory.Security)]
        public string DepartementList {
            get;
            set;
        }
    }
}
