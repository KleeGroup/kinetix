namespace Kinetix.Search.Model {

    /// <summary>
    /// Facette de booléen.
    /// </summary>
    public class BooleanFacet : IFacetDefinition {

        /// <inheritdoc />
        public string Code { get; set; }

        /// <inheritdoc />
        public string Label { get; set; }

        /// <inheritdoc />
        public string FieldName { get; set; }

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public string ResolveLabel(object primaryKey) {
            return (string)primaryKey == "1" || (string)primaryKey == "true" ? "Oui" : "Non";
        }
    }
}
