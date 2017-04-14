using System;

namespace Kinetix.Broker {
    /// <summary>
    /// Régle permettant la gestion des dates de création.
    /// </summary>
    public class CreationDateRule : AbstractCreationRule {
        /// <summary>
        /// Crée une nouvelle de règle.
        /// </summary>
        /// <param name="fieldName">Nom du champ portant la règle.</param>
        public CreationDateRule(string fieldName)
            : base(fieldName) {
        }

        /// <summary>
        /// Retourne la valeur à insérer.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>La valeur du champ et le type d'action attendu.</returns>
        public override ValueRule GetInsertValue(object fieldValue) {
            return new ValueRule(DateTime.UtcNow, ActionRule.Update);
        }
    }
}
