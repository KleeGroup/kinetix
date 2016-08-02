using System;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;

namespace Kinetix.ComponentModel.DataAnnotations {

    /// <summary>
    /// Contrainte sur les dates.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DateAttribute : ValidationAttribute {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="precision">Précision.</param>
        public DateAttribute(int precision) {
            Precision = precision;

            this.ErrorMessageResourceType = typeof(SR);
            this.ErrorMessageResourceName = "DateContraintError";
        }

        /// <summary>
        /// Retourne la précision.
        /// </summary>
        public int Precision {
            get;
            private set;
        }

        /// <summary>
        /// Retourne si l'objet testé est une date valide pour SQL Server.
        /// </summary>
        /// <param name="value">Objet testé.</param>
        /// <returns><code>True</code> si l'objet est une date et qu'elle est bien dans l'intervalle de date SQL Server.</returns>
        public override bool IsValid(object value) {
            string strValue = value as string;
            if (strValue != null) {
                if (string.IsNullOrEmpty(strValue)) {
                    return true;
                }

                DateTime testedValue;
                return DateTime.TryParse(strValue, out testedValue) && CheckRange(testedValue);
            }

            DateTime? dateValue = (DateTime?)value;
            return !dateValue.HasValue || CheckRange(dateValue.Value);
        }

        /// <summary>
        /// Vérifie que la date est bien une date SQL Server valide.
        /// </summary>
        /// <param name="value">Valeur de la date testée.</param>
        /// <returns><code>True</code> si la date est correcte, <code>False</code> sinon.</returns>
        private static bool CheckRange(DateTime value) {
            return SqlDateTime.MaxValue.Value >= value && SqlDateTime.MinValue.Value <= value;
        }
    }
}
