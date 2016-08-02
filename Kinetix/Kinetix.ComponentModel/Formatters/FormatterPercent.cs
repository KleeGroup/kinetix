using System;
using System.Globalization;
using System.Windows.Data;

namespace Kinetix.ComponentModel.Formatters {
    /// <summary>
    /// Définition d'un formateur pour les décimaux stockés en base sous forme d'entiers.
    /// </summary>
    [ValueConversion(typeof(decimal), typeof(string))]
    public class FormatterPercent : AbstractFormatter<decimal?> {
        /// <summary>
        /// Unité pour les pourcentages.
        /// </summary>
        public const string UnitPercentage = "%";

        /// <summary>
        /// Retourne l'unité associée au format.
        /// </summary>
        public override string Unit {
            get {
                return UnitPercentage;
            }
        }

        /// <summary>
        /// Convertit une chaîne entrée d'un pourcentage en nombre décimal.
        /// </summary>
        /// <param name="text">Chaîne saisie.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override decimal? InternalConvertFromString(string text) {
            if (string.IsNullOrEmpty(text)) {
                return null;
            }

            string testValue = text.Replace(",", ".").Trim(' ', '%');
            if (string.IsNullOrEmpty(testValue)) {
                return null;
            }

            decimal value;
            if (!decimal.TryParse(testValue, NumberStyles.Currency, CultureInfo.InvariantCulture, out value)) {
                throw new FormatException(string.Format(CultureInfo.CurrentCulture, SR.ErrorFormatPercentage, text));
            }

            return value;
        }

        /// <summary>
        /// Convertit un decimal en chaîne de caractères.
        /// </summary>
        /// <param name="value">Valeur à convertir.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override string InternalConvertToString(decimal? value) {
            return value.HasValue ? value.Value.ToString(this.FormatString, CultureInfo.CurrentCulture) : null;
        }
    }
}
