using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Exception for an entity.
    /// </summary>
    [Serializable]
    public class EntityConstraintException : Exception {

        /// <summary>
        /// Container for the fields errors.
        /// </summary>
        private EntityErrorMessage _fieldErrors;

        /// <summary>
        /// Create an empty entity constraint exception.
        /// </summary>
        public EntityConstraintException() {
            _fieldErrors = new EntityErrorMessage();
        }

        /// <summary>
        /// Create a new EntityConstraintException from an Entity Error message.
        /// </summary>
        /// <param name="entityErrorMessage">Error Message.</param>
        public EntityConstraintException(EntityErrorMessage entityErrorMessage) {
            _fieldErrors = entityErrorMessage;
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public EntityConstraintException(string message)
            : base(message) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exceotion interne.</param>
        public EntityConstraintException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="serializationInfo">Information de sérialisation.</param>
        /// <param name="streamingContext">Contexte de sérialisation.</param>
        protected EntityConstraintException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) {
        }

        /// <summary> Get the field errors property.</summary>
        public IDictionary<string, string> FieldErrors {
            get {
                return _fieldErrors.FieldErrors;
            }
        }

        /// <summary>
        /// Add an error to the fieldConstraintException.
        /// </summary>
        /// <param name="field">Field property.</param>
        /// <param name="message">Error message or string code on the client side.</param>
        public void AddError(string field, string message) {
            if (field == null) {
                throw new ArgumentNullException("field");
            }

            if (message == null) {
                throw new ArgumentNullException("message");
            }

            _fieldErrors.AddError(field, message);
        }

        /// <summary>
        /// Add many errors.
        /// </summary>
        /// <param name="messages">A dictionnary of errors.</param>
        public void AddErrors(Dictionary<string, string> messages) {
            if (messages == null) {
                throw new ArgumentNullException("messages");
            }

            foreach (var message in messages) {
                this.AddError(message.Key, message.Value);
            }
        }
    }
}
