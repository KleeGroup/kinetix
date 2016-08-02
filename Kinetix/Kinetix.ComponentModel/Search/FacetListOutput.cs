using Newtonsoft.Json;

namespace Kinetix.ComponentModel.Search {

    /// <summary>
    /// Liste de facettes : association nom de la facette => facette.
    /// </summary>
    [JsonConverter(typeof(OrderedDictionarySerializer))]
    public class FacetListOutput : OrderedDictionary<string, FacetOutput> {
    }
}
