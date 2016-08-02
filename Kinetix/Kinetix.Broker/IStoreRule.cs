namespace Kinetix.Broker {
    /// <summary>
    /// Interface permettant de gérer des règles de comportement
    /// sur des champs.
    /// </summary>
    public interface IStoreRule {

        /// <summary>
        /// Retourne le nom du champ.
        /// </summary>
        string FieldName {
            get;
        }

        /// <summary>
        /// Retourne la valeur à insérer.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>La valeur du champ et le type d'action attendu.</returns>
        ValueRule GetInsertValue(object fieldValue);

        /// <summary>
        /// Retourne la valeur à mettre à jour.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>La valeur du champ et le type d'action attendu.</returns>
        ValueRule GetUpdateValue(object fieldValue);

        /// <summary>
        /// Retourne la clause Where à insérer.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>La valeur du champ et le type d'action attendu.</returns>
        ValueRule GetWhereClause(object fieldValue);
    }
}
