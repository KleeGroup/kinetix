using System;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Kinetix.ComponentModel.Formatters {
    /// <summary>
    /// Définition d'un formateur de booléen.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class FormatterBooleen : AbstractFormatter<bool?> {

        /// <summary>
        /// Valeur True.
        /// </summary>
        public static string True {
            get {
                return SR.TextBooleanYes;
            }
        }

        /// <summary>
        /// Valeur False.
        /// </summary>
        public static string False {
            get {
                return SR.TextBooleanNo;
            }
        }

        /// <summary>
        /// Valeur non définie.
        /// </summary>
        public static string Undefined {
            get {
                return SR.TextBooleanUndefined;
            }
        }

        /// <summary>
        /// Convertit une chaine de caractère Oui ou Non en booléen.
        /// </summary>
        /// <param name="text">Texte du booléen.</param>
        /// <returns>La valeur booléenne correspondant au texte.</returns>
        /// <todo type="IGNORE" who="SEY">Internationalisation.</todo>
        protected override bool? InternalConvertFromString(string text) {
            if (string.IsNullOrEmpty(text)) {
                return null;
            }

            // conversion en booléen
            bool value;
            if (!bool.TryParse(text, out value)) {

                // vérification du format de saisie
                string numeroFormat = @"^(true|True|oui|Oui|1)$";
                if (Regex.IsMatch(text, numeroFormat)) {
                    return true;
                }

                numeroFormat = @"^(false|False|non|Non|0)$";
                if (Regex.IsMatch(text, numeroFormat)) {
                    return false;
                }

                throw new FormatException(SR.ErrorFormatBooleen);
            }

            return value;
        }

        /// <summary>
        /// Convertit un booléen en chaîne de caractères.
        /// </summary>
        /// <param name="value">Représentation de la valeur booléenne.</param>
        /// <returns>La valeur du booléen en string.</returns>
        protected override string InternalConvertToString(bool? value) {
            if (!value.HasValue) {
                return Undefined;
            }

            return value.Value ? True : False;
        }
    }
}
