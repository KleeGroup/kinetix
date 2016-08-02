using System;
using System.Globalization;
using System.Windows.Data;

namespace Kinetix.ComponentModel.Formatters {
    /// <summary>
    /// Définition d'un formateur de date.
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class FormatterDate : AbstractFormatter<DateTime?> {

        /// <summary>
        /// Tableau des formats de String acceptés.
        /// </summary>
        private static readonly string[] _stringFormats = { "dd/MM/yyyy", "ddMMyyyy", "dd/MM/yy", "ddMMyy", "d/MM/yyyy", "dd/M/yyyy", "d/M/yyyy", "d/MM/yy", "dd/M/yy", "d/M/yy" };

        /// <summary>
        /// Convertit un string en date.
        /// </summary>
        /// <param name="text">Données sous forme string.</param>
        /// <returns>Date.</returns>
        /// <exception cref="System.FormatException">En cas d'erreur de convertion.</exception>
        protected override DateTime? InternalConvertFromString(string text) {
            if (string.IsNullOrEmpty(text)) {
                return null;
            }

            try {
                return DateTime.ParseExact(text, _stringFormats, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None);
            } catch (FormatException) {
                char[] array = text.ToCharArray();
                for (int i = 0; i < array.Length; i++) {
                    if (array[i] >= '2' && array[i] <= '9') {
                        array[i] = '1';
                    }
                }

                DateTime date;
                if (DateTime.TryParseExact(new string(array), _stringFormats, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out date)) {
                    throw new FormatException(SR.ErrorFormatDateValue);
                }

                throw new FormatException(SR.ErrorFormatDate);
            }
        }

        /// <summary>
        /// Convertit une date en string.
        /// </summary>
        /// <param name="value">Date.</param>
        /// <returns>Représentation textuelle de la date.</returns>
        protected override string InternalConvertToString(DateTime? value) {
            return value.HasValue ? value.GetValueOrDefault().ToString(this.FormatString, DateTimeFormatInfo.CurrentInfo) : null;
        }
    }
}
