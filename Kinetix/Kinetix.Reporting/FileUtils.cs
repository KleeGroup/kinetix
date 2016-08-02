using System;
using Kinetix.ComponentModel;

namespace Kinetix.Reporting {

    /// <summary>
    /// Classe de gestion des fichiers : nom de fichier, type MIME, format.
    /// </summary>
    public sealed class FileUtils {

        /// <summary>
        /// Content-Type PNG.
        /// </summary>
        public const string ContentTypePng = "image/png";

        /// <summary>
        /// Content-Type PNG spécifique IE.
        /// </summary>
        public const string ContentTypeXpng = "image/x-png";

        /// <summary>
        /// Content-Type GIF.
        /// </summary>
        public const string ContentTypeGif = "image/gif";

        /// <summary>
        /// Content-Type JPEG.
        /// </summary>
        public const string ContentTypeJpeg = "image/jpeg";

        /// <summary>
        /// Content-Type JPEG spécifique IE.
        /// </summary>
        public const string ContentTypePjpeg = "image/pjpeg";

        /// <summary>
        /// Content-Type JPG.
        /// </summary>
        public const string ContentTypeJpg = "image/jpg";

        /// <summary>
        /// Content-Type BMP.
        /// </summary>
        public const string ContentTypeBmp = "image/bmp";

        /// <summary>
        /// Content-Type CSV.
        /// </summary>
        public const string ContentTypeCsv = "text/csv";

        /// <summary>
        /// Content-Type Word 2000.
        /// </summary>
        public const string ContentTypeDoc2000 = "application/msword";

        /// <summary>
        /// Content-Type Word 2007.
        /// </summary>
        public const string ContentTypeDoc2007 = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        /// <summary>
        /// Content-Type Excel 2007.
        /// </summary>
        public const string ContentTypeExcel2007 = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// Content-Type Excel.
        /// </summary>
        public const string ContentTypeExcel = "application/vnd.ms-excel";

        /// <summary>
        /// Content-Type Pdf.
        /// </summary>
        public const string ContentTypePdf = "application/pdf";

        /// <summary>
        /// Content-Type Xml.
        /// </summary>
        public const string ContentTypeXml = "application/xml";

        /// <summary>
        /// Content-Type Zip.
        /// </summary>
        public const string ContentTypeZip = "application/zip";

        /// <summary>
        /// Content-Type Text.
        /// </summary>
        public const string ContentTypeText = "text/plain";

        /// <summary>
        /// Content-Type HTML.
        /// </summary>
        public const string ContentTypeHtml = "text/html";

        /// <summary>
        /// Constructeur privé.
        /// </summary>
        private FileUtils() {
        }

        /// <summary>
        /// Retourne le Content-Type HTTP associé au format du fichier.
        /// </summary>
        /// <param name="format">Le format du fichier.</param>
        /// <returns>Le content-type HTTP.</returns>
        public static string GetContentType(FileFormat format) {
            switch (format) {
                case FileFormat.Csv:
                    return ContentTypeCsv;
                case FileFormat.Doc2000:
                    return ContentTypeDoc2000;
                case FileFormat.Doc2007:
                    return ContentTypeDoc2007;
                case FileFormat.Excel:
                    return ContentTypeExcel;
                case FileFormat.Excel2007:
                    return ContentTypeExcel2007;
                case FileFormat.Pdf:
                    return ContentTypePdf;
                case FileFormat.Xml:
                    return ContentTypeXml;
                case FileFormat.Zip:
                    return ContentTypeZip;
                case FileFormat.Text:
                    return ContentTypeText;
                default:
                    throw new NotSupportedException("Le type de fichier '" + format + "' n'est pas supporté.");
            }
        }

        /// <summary>
        /// Retourne l'extension d'un fichier à partir de son type de fichier.
        /// </summary>
        /// <param name="format">Format du fichier.</param>
        /// <returns>L'extension du fichier.</returns>
        public static string GetExtension(FileFormat format) {
            switch (format) {
                case FileFormat.Csv:
                    return "csv";
                case FileFormat.Doc2000:
                    return "doc";
                case FileFormat.Doc2007:
                    return "docx";
                case FileFormat.Excel:
                    return "xls";
                case FileFormat.Excel2007:
                    return "xlsx";
                case FileFormat.Pdf:
                    return "pdf";
                case FileFormat.Xml:
                    return "xml";
                case FileFormat.Zip:
                    return "zip";
                case FileFormat.Text:
                    return "txt";
                default:
                    throw new NotSupportedException("Le type de fichier '" + format + "' n'est pas supporté.");
            }
        }

        /// <summary>
        /// Retourne l'extension d'un fichier à partir de son content type.
        /// </summary>
        /// <param name="contentType">Format du fichier.</param>
        /// <returns>Content type.</returns>
        public static string GetExtension(string contentType) {
            switch (contentType) {
                case ContentTypeCsv:
                    return "csv";
                case ContentTypeDoc2000:
                    return "doc";
                case ContentTypeDoc2007:
                    return "docx";
                case ContentTypeExcel:
                    return "xls";
                case ContentTypeExcel2007:
                    return "xlsx";
                case ContentTypePdf:
                    return "pdf";
                case ContentTypeXml:
                    return "xml";
                case ContentTypeZip:
                    return "zip";
                case ContentTypeText:
                    return "txt";
                default:
                    throw new NotSupportedException("Le content type '" + contentType + "' n'est pas supporté.");
            }
        }

        /// <summary>
        /// Retourne le format de fichier demandé dans le générateur
        /// d'édition.
        /// </summary>
        /// <param name="format">Type MIME du fichier à générer.</param>
        /// <returns>FileFormat.</returns>
        public static FileFormat GetReportingFileFormat(FileFormat format) {
            switch (format) {
                case FileFormat.Doc2000:
                    return FileFormat.Doc2000;
                case FileFormat.Pdf:
                    return FileFormat.Pdf;
                default:
                    throw new NotSupportedException("Le type de fichier '" + format + "' n'est pas supporté par le service de génération des rapports.");
            }
        }
    }
}
