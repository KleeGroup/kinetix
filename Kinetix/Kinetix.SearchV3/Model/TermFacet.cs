using System.Globalization;

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

        /// <inheritdoc />
        public string Label {
            get;
            set;
        }

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public string ResolveLabel(object primaryKey) {

            // Les espaces doivent être au préalable remplacés par des _ dans l'index.
            string labelNotFormatted = (string)primaryKey;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(labelNotFormatted.Replace('_', ' '));
        }
    }
}
