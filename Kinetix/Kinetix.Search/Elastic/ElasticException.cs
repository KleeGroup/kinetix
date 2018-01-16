using System;
using System.Runtime.Serialization;

namespace Kinetix.Search.Elastic {

    /// <summary>
    /// Exception générée par les appels à ElasticSearch.
    /// </summary>
    [Serializable]
    public class ElasticException : Exception {

        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        public ElasticException() {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public ElasticException(string message)
            : base(message) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public ElasticException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        protected ElasticException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}
