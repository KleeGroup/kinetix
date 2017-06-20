namespace Kinetix.Search.Model {

    /// <summary>
    /// Facette de booléen.
    /// </summary>
    public class BooleanFacet : IFacetDefinition {

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

        /// <inheritdoc />
        public string Label {
            get;
            set;
        }

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public string ResolveLabel(object primaryKey) {

            // TODO : gestion des langues.
            return (string)primaryKey == "T" ? "Oui" : "Non";
        }
    }
}
