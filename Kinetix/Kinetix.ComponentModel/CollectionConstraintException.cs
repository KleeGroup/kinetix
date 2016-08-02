using System;
using System.Runtime.Serialization;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Collection constraint exception.
    /// </summary>
    [Serializable]
    public class CollectionConstraintException : Exception {

        /// <summary>
        /// Exception container.
        /// </summary>
        private readonly EntityCollectionErrorMessage _errors;

        /// <summary>
        /// Default constructor of the collection constraint exception.
        /// </summary>
        public CollectionConstraintException() {
            _errors = new EntityCollectionErrorMessage();
        }

        /// <summary>
        /// Constructor which taks an etity collection error message.
        /// </summary>
        /// <param name="collectionErrors">Collection errors object.</param>
        public CollectionConstraintException(EntityCollectionErrorMessage collectionErrors) {
            _errors = collectionErrors;
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public CollectionConstraintException(string message)
            : base(message) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exceotion interne.</param>
        public CollectionConstraintException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="serializationInfo">Information de sérialisation.</param>
        /// <param name="streamingContext">Contexte de sérialisation.</param>
        protected CollectionConstraintException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) {
        }

        /// <summary>
        /// Expose the json dictionnary error object.
        /// </summary>
        public object Errors {
            get {
                return _errors.Errors;
            }
        }

        /// <summary>
        /// Ajoute une entrée à la pile d'erreur.
        /// </summary>
        /// <param name="clientIdentifier">Identifiant du client.</param>
        /// <param name="entityError">EntityErrorMessage.</param>
        public void AddEntry(string clientIdentifier, EntityErrorMessage entityError) {
            if (string.IsNullOrWhiteSpace(clientIdentifier)) {
                throw new ArgumentNullException("clientIdentifier");
            }

            if (entityError == null) {
                throw new ArgumentNullException("entityError");
            }

            _errors.AddEntityError(clientIdentifier, entityError);
        }
    }
}
