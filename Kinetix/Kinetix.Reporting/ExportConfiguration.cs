using System;
using System.Globalization;
using Kinetix.ComponentModel.Search;

namespace Kinetix.Reporting {

    /// <summary>
    /// Export configuration.
    /// </summary>
    public class ExportConfiguration {

        /// <summary>
        /// Identifier of the export.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The stringify criteria.
        /// </summary>
        public QueryInput Input { get; set; }

        /// <summary>
        /// The culture.
        /// </summary>
        public CultureInfo Culture { get; set; }
    }
}
