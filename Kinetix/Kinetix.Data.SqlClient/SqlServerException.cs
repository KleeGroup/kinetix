using System;
using System.Runtime.Serialization;

namespace Kinetix.Data.SqlClient {
    /// <summary>
    /// Exception générée par les appels base de données.
    /// </summary>
    [Serializable]
    public class SqlServerException : Exception {
        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        public SqlServerException() {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public SqlServerException(string message)
            : base(message) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public SqlServerException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        protected SqlServerException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}
