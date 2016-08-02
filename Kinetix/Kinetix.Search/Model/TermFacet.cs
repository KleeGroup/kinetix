using Kinetix.ServiceModel;

namespace Kinetix.Search.Model {

    /// <summary>
    /// Facette de terme.
    /// </summary>
    public class TermFacet : IFacetDefinition {

        /// <inheritdoc />
        public string FieldName {
            get;
            set;
        }

        /// <inheritdoc />
        public string Name {
            get;
            set;
        }

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public string ResolveLabel(object primaryKey) {
            return (string)primaryKey;
        }
    }
}
