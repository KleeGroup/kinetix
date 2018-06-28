namespace Kinetix.Search.Model {

    /// <summary>
    /// Facette de terme.
    /// </summary>
    public class TermFacet : IFacetDefinition {

        /// <inheritdoc />
        public string Code { get; set; }

        /// <inheritdoc />
        public string Label { get; set; }

        /// <inheritdoc />
        public string FieldName { get; set; }

        /// <inheritdoc />
        public bool IsMultiSelectable { get; set; } = false;

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public string ResolveLabel(object primaryKey) {
            return (string)primaryKey;
        }
    }
}
