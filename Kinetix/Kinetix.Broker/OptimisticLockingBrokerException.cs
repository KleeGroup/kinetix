using System;
using System.Runtime.Serialization;

namespace Kinetix.Broker {
    /// <summary>
    /// Exception générée par le broker spécifique pour l'optimistic locking.
    /// </summary>
    [Serializable]
    public class OptimisticLockingBrokerException : BrokerException {
        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        public OptimisticLockingBrokerException()
            : base() {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public OptimisticLockingBrokerException(string message)
            : base(message) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public OptimisticLockingBrokerException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        protected OptimisticLockingBrokerException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}
