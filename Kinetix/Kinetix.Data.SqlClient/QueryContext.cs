using Kinetix.ComponentModel;

namespace Kinetix.Data.SqlClient {

    /// <summary>
    /// Encapsule l'accès au QueryParamater dans le contexte.
    /// </summary>
    public static class QueryContext {

        /// <summary>
        /// Valeur de stockage de l'inlinecount.
        /// </summary>
        public const string InlineCountValue = "InlineCountValue";

        /// <summary>
        /// Obtient et définit le compteur du nombre de ligne de la requête.
        /// </summary>
        public static long? InlineCount {
            get {
                return (long?)ContextContainer.Current[InlineCountValue];
            }

            set {
                ContextContainer.Current[InlineCountValue] = value;
            }
        }

        /// <summary>
        /// Obtient et définit le paramètre de requête dans le contexte.
        /// </summary>
        public static QueryParameter Parameter {
            get {
                return (QueryParameter)ContextContainer.Current[typeof(QueryParameter)];
            }

            set {
                ContextContainer.Current[typeof(QueryParameter)] = value;
            }
        }
    }
}
