using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Kinetix.ComponentModel.Formatters {

    /// <summary>
    /// Définition d'un formateur pour les numéros de téléphone.
    /// </summary>
    [ValueConversion(typeof(short), typeof(string))]
    public class FormatterTelephone : AbstractFormatter<string> {

        /// <summary>
        /// Convertit une chaîne entrée en numéro de téléphone.
        /// </summary>
        /// <param name="text">Chaîne saisie.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override string InternalConvertFromString(string text) {
            if (text == null) {
                return null;
            }

            if (string.IsNullOrEmpty(text.Trim())) {
                return null;
            }

            if (!Regex.IsMatch(text, @"^([\d\s\.+()])*$")) {
                throw new FormatException(SR.ErrorFormatTelephone);
            }

            // éliminer les espaces, les points et les slash
            string telephone = text.Replace(@" ", string.Empty).Replace(@".", string.Empty).Replace(@"/", string.Empty).Replace(@"(", string.Empty).Replace(@")", string.Empty);
            if (telephone.StartsWith("+33", StringComparison.OrdinalIgnoreCase)) {
                if (telephone.Length == 13) {
                    telephone = "+33" + telephone.Substring(4);
                } else if (telephone.Length != 12) {
                    throw new FormatException(SR.ErrorFormatTelephone);
                }
            }

            return telephone;
        }

        /// <summary>
        /// Convertit un numéro de téléphone pour affichage.
        /// </summary>
        /// <param name="value">Chaîne d'origine.</param>
        /// <returns>Chaîne convertie.</returns>
        protected override string InternalConvertToString(string value) {
            if (string.IsNullOrEmpty(value)) {
                return null;
            }

            string telephone = value.Replace(@" ", string.Empty).Replace(@".", string.Empty).Replace(@"/", string.Empty).Replace(@"(", string.Empty).Replace(@")", string.Empty);

            if (telephone.StartsWith("+33", StringComparison.OrdinalIgnoreCase) && telephone.Length == 12) {
                char[] valueArray = telephone.ToCharArray();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < valueArray.Length; i++) {
                    if (i == 3 || i == 4 || i == 6 || i == 8 || i == 10) {
                        sb.Append(' ');
                    }

                    sb.Append(valueArray[i]);
                }

                return sb.ToString();
            }

            if (telephone.StartsWith("+33", StringComparison.OrdinalIgnoreCase) && telephone.Length == 13) {
                char[] valueArray = telephone.ToCharArray();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < valueArray.Length; i++) {
                    if (i == 3 || i == 5 || i == 7 || i == 9 || i == 11) {
                        sb.Append(' ');
                    }

                    if (i == 4 && valueArray[i] == '0') {
                        continue;
                    }

                    sb.Append(valueArray[i]);
                }

                return sb.ToString();
            } else {
                char[] valueArray = telephone.ToCharArray();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < valueArray.Length; i++) {
                    if ((i > 0) && (i % 2 == 0)) {
                        sb.Append(' ');
                    }

                    sb.Append(valueArray[i]);
                }

                return sb.ToString();
            }
        }
    }
}
