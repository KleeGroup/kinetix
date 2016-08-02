using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Kinetix.ComponentModel.DataAnnotations {

    /// <summary>
    /// Attribut de validation lié aux emails.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class EmailAttribute : StringLengthAttribute {

        /// <summary>
        /// Chaine d'expression régulière de validation des emails.
        /// </summary>
        private const string _strRegex = @"^[a-z_.-]+@[a-z_.-]+\.[a-z]+$";

        /// <summary>
        /// Expression régulière de validation des emails.
        /// </summary>
        private readonly Regex _mailRegex = new Regex(_strRegex);

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="maximumLength">Longueur maximum autorisée pour l'email.</param>
        public EmailAttribute(int maximumLength)
            : base(maximumLength) {
        }

        /// <summary>
        /// Retourne si l'objet valide la contrainte.
        /// </summary>
        /// <param name="value">Valeur testée.</param>
        /// <returns><code>True</code> si l'objet est valide, <code>False</code> sinon.</returns>
        public override bool IsValid(object value) {
            string strValue = value as string;
            return base.IsValid(value) && (strValue == null || _mailRegex.IsMatch(strValue));
        }

        /// <summary>
        /// Retourne le message d'erreur.
        /// </summary>
        /// <param name="name">Nom du champ.</param>
        /// <returns>Message d'erreur.</returns>
        public override string FormatErrorMessage(string name) {
            return string.Format(CultureInfo.CurrentCulture, SR.ErrorConstraintEmail, this.MaximumLength);
        }
    }
}
