using System;
using System.Runtime.Serialization;

namespace Kinetix.Data.SqlClient {

    /// <summary>
    /// Exception générée par le CollectionBuilder.
    /// </summary>
    [Serializable]
    public class CollectionBuilderException : Exception {

        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        public CollectionBuilderException()
            : base() {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public CollectionBuilderException(string message)
            : base(message) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public CollectionBuilderException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        protected CollectionBuilderException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}
