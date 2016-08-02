using System;
using System.Globalization;
using System.Text;
using System.Web;

namespace Kinetix.Reporting {
    /// <summary>
    /// Envoie du téléchargement d'un fichier dans la réponse du context HTTP courant.
    /// </summary>
    public static class ReportHttpContext {
        /// <summary>
        /// Crée une réponse HTTP et l'envoie dans le contexte HTTP courant.
        /// </summary>
        /// <param name="fileName">Nom du fichier.</param>
        /// <param name="contentType">Type du fichier.</param>
        /// <param name="binaryFile">Fichier binaire.</param>
        public static void SendResponse(string fileName, string contentType, byte[] binaryFile) {
            if (binaryFile == null) {
                throw new ArgumentNullException("binaryFile");
            }

            if (HttpContext.Current.Response.IsClientConnected) {
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=\"" + RemoveDiacritics(fileName) + "\"");
                HttpContext.Current.Response.ContentEncoding = Encoding.Default;
                HttpContext.Current.Response.ContentType = contentType;
                if (binaryFile.Length > 0) {
                    HttpContext.Current.Response.BinaryWrite(binaryFile);
                }

                HttpContext.Current.Response.End();
            }
        }

        /// <summary>
        /// Supprime les accents d'une chaine de caractère.
        /// </summary>
        /// <param name="item">La chaine à traiter.</param>
        /// <returns>Retourne la chaine sans accents.</returns>
        public static string RemoveDiacritics(string item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            string normalizedString = item.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < normalizedString.Length; i++) {
                char c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
