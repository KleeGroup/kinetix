using System;

namespace Kinetix.Reporting {

    /// <summary>
    /// Interface to extend in order to export elements.
    /// </summary>
    /// <typeparam name="T">T is a class which must have an ExportId property.</typeparam>
    public interface IExport<T> : IExportFile {

        /// <summary>
        /// Method POST to implement on the controller in order to register the export demand in the ExportCache.
        /// </summary>
        /// <param name="criteria">Criteria contains all the export criterias.</param>
        /// <returns>Th identifier of the export. Which is the ExportId.</returns>
        Guid Export(T criteria);
    }
}
