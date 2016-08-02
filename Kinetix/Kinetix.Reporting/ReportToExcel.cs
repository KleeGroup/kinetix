using System;
using System.IO;
using Kinetix.ServiceModel;

namespace Kinetix.Reporting {

    /// <summary>
    /// Classe de génération de document Excel à partir de données fournies.
    /// </summary>
    public static class ReportToExcel {
        /// <summary>
        /// Maximum number of row for xls document.
        /// </summary>
        public const int MaxRows = 65535;

        /// <summary>
        /// Crée un fichier Excel 2003 à partir des valeurs et des critères données.
        /// </summary>
        /// <param name="exportData">Données de l'export.</param>
        /// <returns>Le fichier Excel généré.</returns>
        public static byte[] CreateXlsDocument(ExportDataSource exportData) {
            byte[] xlsxDocument = CreateXslsxDocument(exportData);
            using (ServiceChannel<IExcelConverter> channel = new ServiceChannel<IExcelConverter>()) {
                return channel.Service.XlsxToXls(xlsxDocument);
            }
        }

        /// <summary>
        /// Créer un fichier Excel 2007 à partir des valeurs et des critères donnés.
        /// </summary>
        /// <param name="exportData">Données de l'export.</param>
        /// <returns>Le fichier Excel généré.</returns>
        public static byte[] CreateXslsxDocument(ExportDataSource exportData) {
            if (exportData == null) {
                throw new ArgumentNullException("exportData");
            }

            string fileName = GetTemporaryFileName("xlsx");
            using (ExcelDocument excelDocument = new ExcelDocument(fileName)) {
                excelDocument.FillDocument(exportData.Sheets);
            }

            byte[] xlsxDocument = File.ReadAllBytes(fileName);
            File.Delete(fileName);
            return xlsxDocument;
        }

        /// <summary>
        /// Retourne le nom de fichier temporaire utilisé pour générer le fichier Excel.
        /// </summary>
        /// <param name="extension">Extension du fichier.</param>
        /// <returns>Le nom du fichier temporaire.</returns>
        private static string GetTemporaryFileName(string extension) {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "." + extension);
        }
    }
}
