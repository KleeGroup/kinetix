using Kinetix.Security;

namespace Kinetix.Broker {
    /// <summary>
    /// Régle permettant la gestion de l'utilisateur de création.
    /// </summary>
    public class CreationUserRule : AbstractCreationRule {

        /// <summary>
        /// Crée une nouvelle de règle.
        /// </summary>
        /// <param name="fieldName">Nom du champ portant la règle.</param>
        public CreationUserRule(string fieldName)
            : base(fieldName) {
        }

        /// <summary>
        /// Retourne la valeur à insérer.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>La valeur du champ et le type d'action attendu.</returns>
        public override ValueRule GetInsertValue(object fieldValue) {
            int? userId = StandardUser.UserId;
            if (userId.HasValue) {
                return new ValueRule(userId.Value, ActionRule.Update);
            }

            return new ValueRule(0, ActionRule.Update);
        }
    }
}
