using System;

namespace Kinetix.Broker {
    /// <summary>
    /// Règle de comportement sur un champ de version
    /// d'un objet T.
    /// </summary>
    public class VersionRule : IStoreRule {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="fieldName">Nom du champ.</param>
        public VersionRule(string fieldName) {
            if (string.IsNullOrEmpty(fieldName)) {
                throw new ArgumentNullException("fieldName");
            }

            this.FieldName = fieldName;
        }

        /// <summary>
        /// Obtient le nom du champ du bean T pour lequel on associe un rôle de type Version de Kinetix.
        /// </summary>
        public string FieldName {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la valeur à insérer.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>Retourne la valeur et l'action à effectuer.</returns>
        public ValueRule GetInsertValue(object fieldValue) {
            return new ValueRule(1, ActionRule.Update);
        }

        /// <summary>
        /// Retourne la valeur à mettre à jour.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>Retourne la valeur et l'action à effectuer.</returns>
        public ValueRule GetUpdateValue(object fieldValue) {
            return new ValueRule(1, ActionRule.IncrementalUpdate);
        }

        /// <summary>
        /// Retourne la clause where à insérer.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>Retourne la valeur et l'action à effectuer.</returns>
        public ValueRule GetWhereClause(object fieldValue) {
            return new ValueRule(fieldValue, ActionRule.Check);
        }
    }
}
