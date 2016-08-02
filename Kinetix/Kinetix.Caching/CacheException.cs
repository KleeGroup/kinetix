using System;
using System.Runtime.Serialization;

namespace Kinetix.Caching {
    /// <summary>
    /// Exception sur la gestion des caches.
    /// </summary>
    [Serializable]
    public class CacheException : Exception {
        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        public CacheException() {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public CacheException(string message)
            : base(message) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public CacheException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        protected CacheException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}
