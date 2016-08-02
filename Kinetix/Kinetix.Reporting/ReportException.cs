using System;
using System.Runtime.Serialization;

namespace Kinetix.Reporting {
    /// <summary>
    /// Exception levée par le générateur d'éditions.
    /// </summary>
    [Serializable]
    public class ReportException : Exception {
        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        public ReportException()
            : base() {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public ReportException(string message)
            : base(message) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public ReportException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        protected ReportException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}
