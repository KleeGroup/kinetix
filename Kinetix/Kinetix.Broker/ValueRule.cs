namespace Kinetix.Broker {

    /// <summary>
    /// Règle.
    /// </summary>
    public sealed class ValueRule {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="value">Valeur retournée.</param>
        /// <param name="action">Action retournée.</param>
        public ValueRule(object value, ActionRule action) {
            this.Value = value;
            this.Action = action;
        }

        /// <summary>
        /// Obtient l'action.
        /// </summary>
        public ActionRule Action {
            get;
            private set;
        }

        /// <summary>
        /// Obtient la valeur.
        /// </summary>
        public object Value {
            get;
            private set;
        }
    }
}
