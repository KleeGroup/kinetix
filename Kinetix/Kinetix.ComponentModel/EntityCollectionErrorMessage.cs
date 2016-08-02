using System;
using System.Collections.Generic;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Class which represents the errors for an entity collection: a composite object.
    /// </summary>
    public class EntityCollectionErrorMessage {

        /// <summary>
        /// Container for a collection.
        /// </summary>
        private readonly IDictionary<string, EntityErrorMessage> _entitiesErrorMessage;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntityCollectionErrorMessage() {
            _entitiesErrorMessage = new Dictionary<string, EntityErrorMessage>();
        }

        /// <summary>
        /// Error messages for a collection.
        /// </summary>
        /// <param name="key">Client side id.</param>
        /// <param name="entityErrorMessage">Message.</param>
        public EntityCollectionErrorMessage(string key, EntityErrorMessage entityErrorMessage) {
            _entitiesErrorMessage = new Dictionary<string, EntityErrorMessage>();
            AddEntityError(key, entityErrorMessage);
        }

        /// <summary>
        /// Serialization of the errors into Dictionnary.
        /// </summary>
        public object Errors {
            get {
                Dictionary<string, Dictionary<string, string>> errors = new Dictionary<string, Dictionary<string, string>>();
                foreach (var errElt in _entitiesErrorMessage) {
                    errors.Add(errElt.Key, (Dictionary<string, string>)errElt.Value.FieldErrors);
                }

                return errors;
            }
        }

        /// <summary>
        /// Add an error for an entity.
        /// </summary>
        /// <param name="key">Identifier of the entity inside the object "collection".</param>
        /// <param name="entityErrorMessage">The entityErrorMessage object.</param>
        public void AddEntityError(string key, EntityErrorMessage entityErrorMessage) {
            if (string.IsNullOrWhiteSpace(key)) {
                throw new ArgumentNullException("key");
            }

            if (entityErrorMessage == null) {
                throw new ArgumentNullException("entityErrorMessage");
            }

            _entitiesErrorMessage.Add(key, entityErrorMessage);
        }

        /// <summary>
        /// Throw a collection constraint exception if the dictionnary of errors has entries.
        /// </summary>
        public void ThrowIfError() {
            if (HasError()) {
                throw new CollectionConstraintException(this);
            }
        }

        /// <summary>
        /// Returns tru if this error contains an error.
        /// </summary>
        /// <returns>Boolean.</returns>
        public bool HasError() {
            return this._entitiesErrorMessage.Count > 0;
        }
    }
}
