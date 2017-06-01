using System;
using System.Globalization;

namespace Kinetix.Search.Model {

    /// <summary>
    /// Facette de date.
    /// </summary>
    public class DateFacet : IFacetDefinition {

        /// <inheritdoc />
        public string Code { get; set; }

        /// <inheritdoc />
        public string Label { get; set; }

        /// <inheritdoc />
        public string FieldName { get; set; }

        /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
        public string ResolveLabel(object primaryKey) {
            return DateTime.ParseExact((string)primaryKey, "yyyyMMdd", CultureInfo.CurrentCulture).ToString("dd/MM/yyyy");
        }
    }
}
