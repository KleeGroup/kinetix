using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Kinetix.ComponentModel.Search {

    /// <summary>
    /// Liste de groupe de résultats lors d'une recherche sans scope.
    /// Association scope => liste de résultats.
    /// Un groupe = un type de document.
    /// </summary>
    [JsonConverter(typeof(OrderedDictionarySerializer))]
    public class UnscopedGroupResultList : OrderedDictionary<string, ICollection> {

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        public UnscopedGroupResultList() {
        }

        /// <summary>
        /// Construit un UnscopedGroupResultList à partir d'une liste.
        /// </summary>
        /// <param name="list">La liste.</param>
        public UnscopedGroupResultList(IList<KeyValuePair<string, ICollection>> list) {
            InnerList = list?.Select(ToDict).ToList();
        }
    }
}
