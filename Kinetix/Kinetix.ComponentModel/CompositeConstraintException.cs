using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Exception for composite view.
    /// </summary>
    [Serializable]
    public class CompositeConstraintException : Exception {

        /// <summary>
        /// Dico containings the errors by block.
        /// </summary>
        private readonly IDictionary<string, Exception> _errors = new Dictionary<string, Exception>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CompositeConstraintException() {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public CompositeConstraintException(string message)
            : base(message) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exceotion interne.</param>
        public CompositeConstraintException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="serializationInfo">Information de sérialisation.</param>
        /// <param name="streamingContext">Contexte de sérialisation.</param>
        protected CompositeConstraintException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) {
        }

        /// <summary>
        /// Map of errors.
        /// </summary>
        public IDictionary<string, Exception> Errors {
            get {
                return _errors;
            }
        }

        /// <summary>
        /// Ajoute une entrée à la pile d'erreur.
        /// </summary>
        /// <param name="subModelIdentifier">Identifier of the sub model.</param>
        /// <param name="exception">Model exception.</param>
        public void AddEntry(string subModelIdentifier, Exception exception) {
            _errors.Add(subModelIdentifier, exception);
        }

        /// <summary>
        /// Add an entry.
        /// </summary>
        /// <param name="subModelIdentifier">Model identifier.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="errorMesage">Error message.</param>
        public void AddEntry(string subModelIdentifier, string fieldName, string errorMesage) {
            _errors.Add(subModelIdentifier, new EntityConstraintException(new EntityErrorMessage(fieldName, errorMesage)));
        }

        /// <summary>
        /// Throw an exception if there is an error.
        /// </summary>
        public void ThrowIfError() {
            if (_errors.Count > 0) {
                throw this;
            }
        }
    }
}
