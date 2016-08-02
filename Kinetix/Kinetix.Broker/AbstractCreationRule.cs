using System;

namespace Kinetix.Broker {

    /// <summary>
    /// Régle permettant la gestion de l'utilisateur de création.
    /// </summary>
    public abstract class AbstractCreationRule : IStoreRule {

        private readonly string _fieldName;

        /// <summary>
        /// Crée une nouvelle de règle.
        /// </summary>
        /// <param name="fieldName">Nom du champ portant la règle.</param>
        protected AbstractCreationRule(string fieldName) {
            if (fieldName == null) {
                throw new ArgumentNullException("fieldName");
            }

            _fieldName = fieldName;
        }

        /// <summary>
        /// Retourne le nom du champ.
        /// </summary>
        public string FieldName {
            get {
                return _fieldName;
            }
        }

        /// <summary>
        /// Retourne la valeur à insérer.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>La valeur du champ et le type d'action attendu.</returns>
        public abstract ValueRule GetInsertValue(object fieldValue);

        /// <summary>
        /// Retourne la valeur à mettre à jour.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>Retourne la valeur et l'action à effectuer.</returns>
        public virtual ValueRule GetUpdateValue(object fieldValue) {
            return new ValueRule(null, ActionRule.DoNothing);
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
