using System.Collections;

namespace Kinetix.ComponentModel.Search {

    /// <summary>
    /// Liste de groupe de résultats lors d'une recherche par groupe.
    /// Association valeur du champ de groupe => liste de résultats.
    /// </summary>
    public class GroupResult {

        /// <summary>
        /// Code du groupe (= celui de la facette).
        /// </summary>
        public string Code {
            get;
            set;
        }

        /// <summary>
        /// Label du groupe (= celui de la facette)
        /// </summary>
        public string Label {
            get;
            set;
        }

        /// <summary>
        /// Liste d'éléments du groupe.
        /// </summary>
        public ICollection List {
            get;
            set;
        }

        /// <summary>
        /// Nombre d'éléments du groupe.
        /// </summary>
        public int? TotalCount {
            get;
            set;
        }
    }
}
