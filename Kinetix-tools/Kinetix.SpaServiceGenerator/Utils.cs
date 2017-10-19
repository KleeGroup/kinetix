using System.Text.RegularExpressions;

namespace Kinetix.SpaServiceGenerator {

    /// <summary>
    /// Regroupe quelques utilitaires.
    /// </summary>
    public static class Utils {

        /// <summary>
        /// Convertit un text en dash-case.
        /// </summary>
        /// <param name="text">Le texte en entrée.</param>
        /// <returns>Le texte en sortie.</returns>
        public static string ToDashCase(this string text) =>
            Regex.Replace(text, @"\p{Lu}", m => "-" + m.Value)
            .ToLowerInvariant()
            .Substring(1)
            .Replace("/-", "/");

        /// <summary>
        /// Met la première lettre d'un string en minuscule.
        /// </summary>
        /// <param name="text">Le texte en entrée.</param>
        /// <returns>Le texte en sortie.</returns>
        public static string ToFirstLower(string text) =>
           char.ToLower(text[0]) + text.Substring(1);
    }
}
