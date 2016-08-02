using System;
using System.Text;
using Kinetix.ComponentModel;

namespace Kinetix.Reporting {

    /// <summary>
    /// Méthodes utilitaires pour l'encodage de fichiers en base 64.
    /// </summary>
    public static class FileBase64Utils {

        /// <summary>
        /// Calcule la source d'une image HTML pour afficher une image à partir d'un fichier donné.
        /// </summary>
        /// <param name="file">Fichier.</param>
        /// <returns>Source de l'image en base 64.</returns>
        public static string ComputeHtmlImageSrc(DownloadedFile file) {
            var sb = new StringBuilder();
            sb.Append("data:");
            sb.Append(file.ContentType);
            sb.Append(";base64,");
            sb.Append(Convert.ToBase64String(file.Fichier));
            return sb.ToString();
        }
    }
}
