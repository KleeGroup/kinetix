using System.ServiceModel;

namespace Kinetix.Reporting {
    /// <summary>
    /// Interface de convertion d'un document XLSX vers d'autres formats.
    /// </summary>
    [ServiceContract]
    public interface IExcelConverter {

        /// <summary>
        /// Convertit un document au format XLSX vers un document au format XLS.
        /// </summary>
        /// <param name="xlsxDocument">Document XLSX.</param>
        /// <returns>Document XLS.</returns>
        [OperationContract]
        byte[] XlsxToXls(byte[] xlsxDocument);

        /// <summary>
        /// Convertit un document au format XLS vers un document au format XLSX.
        /// </summary>
        /// <param name="xlsDocument">Document XLS.</param>
        /// <returns>Document XLSX.</returns>
        [OperationContract]
        byte[] XlsToXlsx(byte[] xlsDocument);
    }
}
