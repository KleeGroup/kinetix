using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Kinetix.ComponentModel.Formatters {
    /// <summary>
    /// Définition d'un formateur mettant la première lettre des mots
    /// en majuscule et le reste en minucule.
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class FormatterCapitalize : AbstractFormatter<string> {

        /// <summary>
        /// Convertit une chaîne entrée en majuscule.
        /// </summary>
        /// <param name="text">Chaîne saisie.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override string InternalConvertFromString(string text) {
            return Convert(text);
        }

        /// <summary>
        /// Convertit une chaîne en majuscule pour affichage.
        /// </summary>
        /// <param name="value">Chaîne d'origine.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override string InternalConvertToString(string value) {
            return Convert(value);
        }

        /// <summary>
        /// Convertit le texte.
        /// </summary>
        /// <param name="text">Texte d'entrée.</param>
        /// <returns>Texte converti.</returns>
        private static string Convert(string text) {
            if (string.IsNullOrEmpty(text)) {
                return null;
            }

            Regex regex = new Regex(@"\b\w");
            return regex.Replace(text.ToLower(CultureInfo.CurrentUICulture), UpperMatchEvaluator);
        }

        /// <summary>
        /// Convertit une expression en majuscule.
        /// </summary>
        /// <param name="match">Expression à convertir.</param>
        /// <returns>Nouveau texte.</returns>
        private static string UpperMatchEvaluator(Match match) {
            return match.Value.ToUpper(CultureInfo.CurrentUICulture);
        }
    }
}
