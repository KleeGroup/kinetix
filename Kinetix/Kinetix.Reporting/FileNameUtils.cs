using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Kinetix.Reporting {

    /// <summary>
    /// Méthodes utilitaires pour gérer les nom de fichier.
    /// </summary>
    public static class FileNameUtils {

        private static readonly Regex _underscoreRegex = new Regex(@"[_]{2,}");

        /// <summary>
        /// Normalise un nom de fichier (sans extension):
        /// - Remplace les accents par les caractères sans accents
        /// - Remplace les caractères non alpha-numériques par des underscore.
        /// </summary>
        /// <param name="raw">Nom de fichier sans extension.</param>
        /// <returns>Nom de fichier traité.</returns>
        public static string NormalizeFileName(string raw) {
            if (string.IsNullOrEmpty(raw)) {
                return raw;
            }

            /* 1. Supprime les accents. */
            string buffer = RemoveDiacritics(raw);

            /* 2. Remplace les caractères spéciaux par des underscore. */
            buffer = RemoveSpecialChars(buffer);

            /* 3. Trim les underscores */
            buffer = TrimUnderscore(buffer);

            return buffer;
        }

        /// <summary>
        /// Remplace les caractères avec accents par les caractères correspondants sans accents.
        /// </summary>
        /// <param name="raw">Chaîne brute.</param>
        /// <returns>Chaîne traitée.</returns>
        private static string RemoveDiacritics(string raw) {
            string normalizedString = raw.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in normalizedString) {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Remplace les caractères non alpha-numérique par des underscore.
        /// </summary>
        /// <param name="raw">Chaîne brute.</param>
        /// <returns>Chaîne traitée.</returns>
        private static string RemoveSpecialChars(string raw) {
            var sb = new StringBuilder();
            foreach (char c in raw) {
                if (char.IsLetterOrDigit(c)) {
                    sb.Append(c);
                } else {
                    sb.Append('_');
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Remplace les suites d'underscore par un seul underscore et supprime les underscore en début et fin de chaîne.
        /// </summary>
        /// <param name="raw">Chaîne brute.</param>
        /// <returns>Chaîne traitée.</returns>
        private static string TrimUnderscore(string raw) {
            return _underscoreRegex.Replace(raw, "_").Trim('_');
        }
    }
}
