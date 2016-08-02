using System.Globalization;
using System.Windows.Data;

namespace Kinetix.ComponentModel.Formatters {
    /// <summary>
    /// Définition d'un formateur pour les montants stockés sous forme de décimaux avec n chiffres après la virgule.
    /// </summary>
    [ValueConversion(typeof(decimal), typeof(string))]
    public class FormatterMontant : FormatterDecimal {

        /// <summary>
        /// Nombre de chiffres après la virgule si un FormatString n'est pas précisé.
        /// </summary>
        private const int Decimales = 9;

        /// <summary>
        /// Convertit un décimal en chaîne de caractère.
        /// </summary>
        /// <param name="value">Nombre décimal.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override string InternalConvertToString(decimal? value) {

            return value == null ? null : string.Format(
                    NumberFormatInfo.CurrentInfo,
                    this.FormatString ?? "{0:N" + (Decimales - 2).ToString(NumberFormatInfo.CurrentInfo) + "}",
                    value);
        }
    }
}
