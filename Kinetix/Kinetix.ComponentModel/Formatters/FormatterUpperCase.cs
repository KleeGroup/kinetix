using System;
using System.Globalization;
using System.Windows.Data;

namespace Kinetix.ComponentModel.Formatters {
    /// <summary>
    /// Définition d'un formateur de chaînes en majuscules.
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class FormatterUpperCase : AbstractFormatter<string> {

        /// <summary>
        /// Convertit une chaîne entrée en majuscule.
        /// </summary>
        /// <param name="text">Chaîne saisie.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override string InternalConvertFromString(string text) {
            return string.IsNullOrEmpty(text) ? null : text.ToUpper(CultureInfo.CurrentUICulture);
        }

        /// <summary>
        /// Convertit une chaîne en majuscule pour affichage.
        /// </summary>
        /// <param name="value">Chaîne d'origine.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override string InternalConvertToString(string value) {
            return InternalConvertFromString(value);
        }
    }
}
