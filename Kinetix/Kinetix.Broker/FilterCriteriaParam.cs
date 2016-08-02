namespace Kinetix.Broker {

    /// <summary>
    /// Paramétarge d'un critère de recherche.
    /// </summary>
    public sealed class FilterCriteriaParam {

        /// <summary>
        /// Constructeur d'un paramétrage de critère.
        /// </summary>
        /// <param name="columnName">Nom de la colonne en base de données.</param>
        /// <param name="exprType">Expression de comparaison utilisée.</param>
        /// <param name="value">Valeur du critère.</param>
        internal FilterCriteriaParam(string columnName, Expression exprType, object value) {
            this.ColumnName = columnName;
            this.Value = value;
            this.Expression = exprType;
        }

        /// <summary>
        /// Valeur du champ en base.
        /// </summary>
        public object Value {
            get;
            private set;
        }

        /// <summary>
        /// Nom de la colonne testée.
        /// </summary>
        public string ColumnName {
            get;
            private set;
        }

        /// <summary>
        /// Type d'expression invoquée.
        /// </summary>
        public Expression Expression {
            get;
            private set;
        }
    }
}
