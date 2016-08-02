using System;

namespace Kinetix.Broker {

    /// <summary>
    /// Règle de comportement sur le champ IsActif d'un objet T.
    /// </summary>
    public class ActivableRule : IStoreRule {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="fieldName">Nom du fichier qui porte la règle.</param>
        public ActivableRule(string fieldName) {
            if (string.IsNullOrEmpty(fieldName)) {
                throw new ArgumentNullException("fieldName");
            }

            this.FieldName = fieldName;
        }

        /// <summary>
        /// Le nom du champ.
        /// </summary>
        public string FieldName {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la valeur à insérer.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>La valeur du champ et le type d'action attendu.</returns>
        public ValueRule GetInsertValue(object fieldValue) {
            return new ValueRule(fieldValue ?? true, ActionRule.Update);
        }

        /// <summary>
        /// Retourne la valeur à mettre à jour.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>Retourne la valeur et l'action à effectuer.</returns>
        public ValueRule GetUpdateValue(object fieldValue) {
            return GetInsertValue(fieldValue);
        }

        /// <summary>
        /// Retourne la clause where à insérer.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>Retourne la valeur et l'action à effectuer.</returns>
        public ValueRule GetWhereClause(object fieldValue) {
            return new ValueRule(null, ActionRule.DoNothing);
        }
    }
}
