using System;
using System.Globalization;
using System.Windows.Data;

namespace Kinetix.ComponentModel.Formatters {
    /// <summary>
    /// Définition d'un formateur pour les décimaux avec gestion du séparateur par ',' ou '.'.
    /// </summary>
    [ValueConversion(typeof(decimal), typeof(string))]
    public class FormatterDecimal : AbstractFormatter<decimal?> {

        /// <summary>
        /// Convertit une chaîne entrée d'un décimal en décimal.
        /// </summary>
        /// <param name="text">Chaîne saisie.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override decimal? InternalConvertFromString(string text) {
            if (string.IsNullOrEmpty(text)) {
                return null;
            }

            text = text.Replace(".", ",");
            text = text.Replace(" ", string.Empty);
            decimal result;
            if (decimal.TryParse(text.Trim(), NumberStyles.Number, NumberFormatInfo.CurrentInfo, out result)) {
                return result;
            } else {
                throw new FormatException(SR.ErrorFormatDecimal);
            }
        }

        /// <summary>
        /// Convertit un nombre entier en decimal pour affichage.
        /// </summary>
        /// <param name="value">Chaîne d'origine.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override string InternalConvertToString(decimal? value) {
            if (value == null) {
                return null;
            }

            if (string.IsNullOrEmpty(this.FormatString)) {
                return value.Value.ToString(CultureInfo.CurrentCulture);
            }

            return value.Value.ToString(this.FormatString, CultureInfo.CurrentCulture);
        }
    }
}
