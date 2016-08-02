namespace Kinetix.Broker {
    /// <summary>
    /// Régle permettant la gestion des dates de modification.
    /// </summary>
    public sealed class ModificationDateRule : CreationDateRule {

        /// <summary>
        /// Crée une nouvelle de règle.
        /// </summary>
        /// <param name="fieldName">Nom du champ portant la règle.</param>
        public ModificationDateRule(string fieldName)
            : base(fieldName) {
        }

        /// <summary>
        /// Retourne la valeur à mettre à jour.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>Retourne la valeur et l'action à effectuer.</returns>
        public override ValueRule GetUpdateValue(object fieldValue) {
            return GetInsertValue(fieldValue);
        }
    }
}
