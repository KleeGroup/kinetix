using System;
using System.Web.Mvc;

namespace Kinetix.Reporting {
    /// <summary>
    /// Publish a method in order to export a file from its export id.
    /// </summary>
    public interface IExportFile {

        /// <summary>
        /// Perform the generation of the export file.
        /// </summary>
        /// <param name="id">Identifier of the export.</param>
        /// <returns>The file which have been generated.</returns>
        FileContentResult ExportFile(Guid id);
    }
}
