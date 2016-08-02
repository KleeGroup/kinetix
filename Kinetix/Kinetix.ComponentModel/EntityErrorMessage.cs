using System;
using System.Collections.Generic;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Class to deal with error messages on an entity.
    /// </summary>
    public class EntityErrorMessage {

        /// <summary>Dictionnary Container for the entity errors, the key is the field name, the value the error(s) message(s)</summary>
        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntityErrorMessage() {
            _errors = new Dictionary<string, string>();
        }

        /// <summary>
        /// Create En entity Error message.
        /// </summary>
        /// <param name="fieldName">Field name.</param>
        /// <param name="errorMesage">Message value.</param>
        public EntityErrorMessage(string fieldName, string errorMesage) {
            _errors = new Dictionary<string, string>();
            AddError(fieldName, errorMesage);
        }

        /// <summary>
        /// Fiels errors.
        /// </summary>
        public IDictionary<string, string> FieldErrors {
            get {
                return _errors;
            }
        }

        /// <summary>
        /// Add an error into the error object.
        /// </summary>
        /// <param name="errorMessage">ErrorMessage object.</param>
        /// <param name="overrideError">If there is an existing error, does it has to be overriden.</param>
        public void AddError(ErrorMessage errorMessage, bool overrideError = true) {
            if (errorMessage == null) {
                throw new ArgumentNullException("errorMessage");
            }

            this.AddError(errorMessage.FieldName, errorMessage.Message, overrideError);
        }

        /// <summary>
        /// Add an error into the error object.
        /// </summary>
        /// <param name="fieldName">Name of the field on which there is an error.</param>
        /// <param name="errorMesage">Error message for the field.</param>
        /// <param name="overrideError">If there is an existing error, does it has to be overriden.</param>
        public void AddError(string fieldName, string errorMesage, bool overrideError = true) {

            if (string.IsNullOrWhiteSpace(fieldName)) {
                throw new ArgumentNullException("fieldName");
            }

            if (string.IsNullOrWhiteSpace(errorMesage)) {
                throw new ArgumentNullException("errorMesage");
            }

            // Container for the error message.
            string value;
            _errors.TryGetValue(fieldName, out value);
            if (!string.IsNullOrWhiteSpace(value)) {

                // Remove the previous value from the dictionnary.
                _errors.Remove(fieldName);

                if (!overrideError) {
                    value = value + " , " + errorMesage;
                } else {
                    value = errorMesage;
                }
            } else {
                value = errorMesage;
            }

            // Add the error into the dictionnary.
            _errors.Add(fieldName, value);
        }

        /// <summary>
        /// Returns true if there is at least one error.
        /// </summary>
        /// <returns>True/false.</returns>
        public bool HasError() {
            return _errors.Count > 0;
        }

        /// <summary>
        /// Throw an exception if there is an error.
        /// </summary>
        public void ThrowIfError() {
            if (this._errors.Count > 0) {
                throw new EntityConstraintException(this);
            }
        }
    }
}
