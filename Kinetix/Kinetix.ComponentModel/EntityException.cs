using System;
using System.Collections.Generic;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Exceptions for focus v2.
    /// </summary>
    public class EntityException : Exception {

        /// <summary>
        /// Clé de l'objet de retour JSON contenant le code de l'erreur.
        /// </summary>
        public const string CodeKey = "code";

        /// <summary>
        /// Clé de l'objet de retour JSON contenant les erreurs globales.
        /// </summary>
        public const string GlobalErrorKey = "globalErrors";

        /// <summary>
        /// List of errors organized by store and field.
        /// </summary>
        private IDictionary<string, object> _errorList = new Dictionary<string, object>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EntityException() {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="error">Error message.</param>
        public EntityException(string error) {
            this.AddError(error);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fieldPath">Name of the field (referentiel.user.nom).</param>
        /// <param name="error">Error message.</param>
        public EntityException(string fieldPath, string error) {
            this.AddError(fieldPath, error);
        }

        /// <summary>
        /// Return the list of errors.
        /// </summary>
        public IDictionary<string, object> ErrorList {
            get {
                return _errorList;
            }
        }

        /// <summary>
        /// Add an error for a field in a row in a list.
        /// </summary>
        /// <param name="fieldPath">List field path.</param>
        /// <param name="rowId">Id of the row (given by the key of the ColletionChanges object).</param>
        /// <param name="rowFieldPath">Row field path.</param>
        /// <param name="error">Error message.</param>
        public void AddCollectionError(string fieldPath, string rowId, string rowFieldPath, string error) {
            if (!_errorList.ContainsKey(fieldPath)) {
                _errorList.Add(fieldPath, new Dictionary<string, IDictionary<string, string>>());
            }

            IDictionary<string, IDictionary<string, string>> errorDetail = (IDictionary<string, IDictionary<string, string>>)_errorList[fieldPath];
            if (!errorDetail.ContainsKey(rowId)) {
                errorDetail.Add(rowId, new Dictionary<string, string>());
            }

            errorDetail[rowId].Add(rowFieldPath, error);
        }

        /// <summary>
        /// Add a general error.
        /// </summary>
        /// <param name="message">Error message.</param>
        public void AddError(string message) {
            if (!_errorList.ContainsKey(GlobalErrorKey)) {
                _errorList.Add(GlobalErrorKey, new List<string>());
            }

            ((ICollection<string>)_errorList[GlobalErrorKey]).Add(message);
        }

        /// <summary>
        /// Add a field error.
        /// </summary>
        /// <param name="fieldPath">Name of the field (referentiel.user.nom).</param>
        /// <param name="error">Error message.</param>
        public void AddError(string fieldPath, string error) {
            _errorList.Add(fieldPath, error);
        }

        /// <summary>
        /// Throw the exception if there is at least one error.
        /// </summary>
        public void ThrowIfError() {
            if (this._errorList.Count > 0) {
                throw this;
            }
        }
    }
}
