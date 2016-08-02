using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Kinetix.ComponentModel.Search {

    /// <summary>
    /// Liste de groupe de résultats lors d'une recherche par groupe.
    /// Association valeur du champ de groupe => liste de résultats.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    [JsonConverter(typeof(OrderedDictionarySerializer))]
    public class GroupResultList<TDocument> : OrderedDictionary<string, ICollection<TDocument>> {

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        public GroupResultList() {
        }

        /// <summary>
        /// Construit un GroupResultList à partir d'une liste.
        /// </summary>
        /// <param name="list">La liste.</param>
        public GroupResultList(IList<KeyValuePair<string, ICollection<TDocument>>> list) {
            InnerList = list?.Select(ToDict).ToList();
        }
    }
}
