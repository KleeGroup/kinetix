using Kinetix.Broker;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Régle de test pour le store.
    /// </summary>
    public class TestStoreRule : IStoreRule {

        private readonly string _fieldName;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="fieldName">Nom du champ.</param>
        public TestStoreRule(string fieldName) {
            _fieldName = fieldName;
        }

        /// <summary>
        /// Valeur de la régle pour l'insertion.
        /// </summary>
        public ValueRule InsertValue {
            get;
            set;
        }

        /// <summary>
        /// Valeur de la régle pour la mise à jour.
        /// </summary>
        public ValueRule UpdateValue {
            get;
            set;
        }

        /// <summary>
        /// Valeur de la régle pour la clause Where.
        /// </summary>
        public ValueRule WhereValue {
            get;
            set;
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
        public ValueRule GetInsertValue(object fieldValue) {
            return InsertValue;
        }

        /// <summary>
        /// Retourne la valeur à mettre à jour.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>La valeur du champ et le type d'action attendu.</returns>
        public ValueRule GetUpdateValue(object fieldValue) {
            return UpdateValue;
        }

        /// <summary>
        /// Retourne la clause Where à insérer.
        /// </summary>
        /// <param name="fieldValue">Valeur du champ.</param>
        /// <returns>La valeur du champ et le type d'action attendu.</returns>
        public ValueRule GetWhereClause(object fieldValue) {
            return WhereValue;
        }
    }
}
