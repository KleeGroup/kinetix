using System;
using System.Text.RegularExpressions;

namespace Kinetix.ClassGenerator {

    /// <summary>
    /// Regroupe quelques utilitaires.
    /// </summary>
    public static class Utils {

        /// <summary>
        /// Convertit un text en dash-case.
        /// </summary>
        /// <param name="text">Le texte en entrée.</param>
        /// <param name="upperStart">Texte commençant par une majuscule.</param>
        /// <returns>Le texte en sortie.</returns>
        public static string ToDashCase(this string text, bool upperStart = true) =>
            Regex.Replace(text, @"\p{Lu}", m => "-" + m.Value)
            .ToLowerInvariant()
            .Substring(upperStart ? 1 : 0)
            .Replace("/-", "/");

        /// <summary>
        /// Met la première lettre d'un string en minuscule.
        /// </summary>
        /// <param name="text">Le texte en entrée.</param>
        /// <returns>Le texte en sortie.</returns>
        public static string ToFirstLower(string text) =>
           char.ToLower(text[0]) + text.Substring(1);

        /// <summary>
        /// Passe le texte donnée en camelCase.
        /// </summary>
        /// <param name="namespaceName">Le texte d'entrée.</param>
        /// <returns>Le texte en camelCase.</returns>
        public static string ToNamespace(string namespaceName) {
            if (namespaceName.EndsWith("DataContract", StringComparison.Ordinal)) {
                return ToFirstLower(namespaceName.Substring(0, namespaceName.Length - 12));
            }

            if (namespaceName.EndsWith("Contract", StringComparison.Ordinal)) {
                return ToFirstLower(namespaceName.Substring(0, namespaceName.Length - 8));
            }

            return ToFirstLower(namespaceName);
        }
    }
}
