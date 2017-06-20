using System.Collections.Generic;

namespace Kinetix.ComponentModel.SearchV3 {
    public class GroupResult<TDocument> {
        
        /// <summary>
        /// Code du groupe.
        /// </summary>
        public string Code {
            get;
            set;
        }

        /// <summary>
        /// Libellé du groupe.
        /// </summary>
        public string Label {
            get;
            set;
        }

        public ICollection<TDocument> List {
            get;
            set;
        }

        public string ListType {
            get;
            set;
        }

        /// <summary>
        /// Nombre d'éléments pour le groupe.
        /// </summary>
        public long TotalCount {
            get;
            set;
        }

        public ICollection<string> Hightlight {
            get;
            set;
        }
    }
}
