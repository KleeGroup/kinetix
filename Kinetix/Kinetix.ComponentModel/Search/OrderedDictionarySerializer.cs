using System;
using Newtonsoft.Json;

namespace Kinetix.ComponentModel.Search {

    /// <summary>
    /// Serializer pour OrderedDictionary.
    /// </summary>
    public class OrderedDictionarySerializer : JsonConverter {

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var facetOutputList = value as IOrderedDictionary;
            if (facetOutputList != null) {
                serializer.Serialize(writer, facetOutputList.InnerList);
            }
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType) {
            return typeof(IOrderedDictionary).IsAssignableFrom(objectType);
        }
    }
}
