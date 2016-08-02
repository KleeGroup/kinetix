namespace Kinetix.Reporting {
    /// <summary>
    /// DTO for export properties.
    /// </summary>
    public class ExportColumnLabel {
        /// <summary>
        /// Name of the property to export.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Label that will be used as the column header for this property.
        /// </summary>
        public string PropertyLabel { get; set; }
    }
}
