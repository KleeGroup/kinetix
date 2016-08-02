using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kinetix.ComponentModel.DataAnnotations {

    /// <summary>
    /// Contrainte sur les numéros SIRET.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class NumeroSiretAttribute : ValidationAttribute {

        /// <summary>
        /// Longueur d'un numéro SIRET.
        /// </summary>
        public const int SiretLength = 14;

        /// <summary>
        /// Expression régulière permettant de valider le format SIRET.
        /// </summary>
        private static readonly string SiretRegex = string.Format(CultureInfo.InvariantCulture, "^[0-9]{{{0}}}$", SiretLength);

        /// <summary>
        /// Constructeur.
        /// </summary>
        public NumeroSiretAttribute() {
            this.ErrorMessageResourceType = typeof(SR);
            this.ErrorMessageResourceName = "SiretConstraintError";
        }

        /// <summary>
        /// Indique si le numéro SIRET est valide ou non.
        /// </summary>
        /// <param name="value">Numéro SIRET.</param>
        /// <returns><code>True</code> si le numéro SIRET est valide, <code>false</code> sinon.</returns>
        public override bool IsValid(object value) {
            string siret = value as string;

            if (string.IsNullOrEmpty(siret)) {
                return false;
            }

            if (!Regex.IsMatch(siret, SiretRegex)) {
                return false;
            }

            int sumOfDigits = 0;
            for (int i = 0; i < siret.Length; i++) {
                int tmp = (siret[i] - '0') * (((i + 1) % 2) + 1);
                sumOfDigits += (tmp / 10) + (tmp % 10);
            }

            return (sumOfDigits % 10) == 0;
        }
    }
}
