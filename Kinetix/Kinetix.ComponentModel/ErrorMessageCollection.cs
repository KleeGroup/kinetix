using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Pile d'erreur.
    /// </summary>
    public sealed class ErrorMessageCollection : ICollection<ErrorMessage> {

        private readonly List<ErrorMessage> _entryList = new List<ErrorMessage>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        public ErrorMessageCollection() {
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="erreurs">Liste d'erreurs.</param>
        public ErrorMessageCollection(IEnumerable<string> erreurs) {
            foreach (var err in erreurs) {
                AddConstraintException(err);
            }
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="erreurs">Liste d'erreurs.</param>
        public ErrorMessageCollection(IEnumerable<ErrorMessage> erreurs) {
            _entryList.AddRange(erreurs);
        }

        /// <summary>
        /// Indique si la pile contient des erreurs.
        /// </summary>
        public bool HasError {
            get { return _entryList.Count > 0; }
        }

        /// <summary>
        /// Indique le nombre d'éléments dans la collection.
        /// </summary>
        int ICollection<ErrorMessage>.Count {
            get {
                return _entryList.Count;
            }
        }

        /// <summary>
        /// Indique que la collection est en lecture seule.
        /// </summary>
        bool ICollection<ErrorMessage>.IsReadOnly {
            get {
                return true;
            }
        }

        /// <summary>
        /// Ajoute une exception à la liste des erreurs.
        /// </summary>
        /// <param name="ce">Exception.</param>
        public void AddConstraintException(ConstraintException ce) {
            if (ce == null) {
                throw new ArgumentNullException("ce");
            }

            if (ce.Errors != null) {
                this.AddErrorStack(string.Empty, ce.Errors);
            } else if (ce.Property != null) {
                this.AddEntry(ce.Property.PropertyName, ce.Message);
            } else {
                this.AddEntry(string.Empty, ce.Message);
            }
        }

        /// <summary>
        /// Ajoute une exception à la liste des erreurs.
        /// </summary>
        /// <param name="message">Le message de l'exception.</param>
        public void AddConstraintException(string message) {
            this.AddConstraintException(new ConstraintException(message));
        }

        /// <summary>
        /// Ajoute une entrée à la pile d'erreur.
        /// </summary>
        /// <param name="errorMessage">Message d'erreur.</param>
        public void AddEntry(ErrorMessage errorMessage) {
            _entryList.Add(errorMessage);
        }

        /// <summary>
        /// Ajoute une entrée à la pile d'erreur.
        /// </summary>
        /// <param name="fieldName">Nom du champ.</param>
        /// <param name="errorMessage">Message d'erreur.</param>
        public void AddEntry(string fieldName, string errorMessage) {
            _entryList.Add(new ErrorMessage(fieldName, errorMessage, null));
        }

        /// <summary>
        /// Ajoute une entrée à la pile d'erreur.
        /// </summary>
        /// <param name="rownum">Numéro de la ligne en erreur.</param>
        /// <param name="fieldName">Nom du champ.</param>
        /// <param name="errorMessage">Message d'erreur.</param>
        public void AddEntry(int rownum, string fieldName, string errorMessage) {
            _entryList.Add(new ErrorMessage("[" + rownum.ToString(CultureInfo.InvariantCulture) + "]." + fieldName, errorMessage, null));
        }

        /// <summary>
        /// Ajoute une pile d'erreur à la pile courante.
        /// </summary>
        /// <param name="fieldPrefix">Préfixe à utiliser.</param>
        /// <param name="errorCollection">Liste des erreurs.</param>
        public void AddErrorStack(string fieldPrefix, ErrorMessageCollection errorCollection) {
            if (errorCollection == null) {
                throw new ArgumentNullException("errorCollection");
            }

            foreach (ErrorMessage entry in errorCollection) {
                this.AddEntry(fieldPrefix + entry.FieldName, entry.Message);
            }
        }

        /// <summary>
        /// Léve une erreur si des erreurs ont été détectées.
        /// </summary>
        public void Throw() {
            if (this.HasError) {
                throw new ConstraintException(this);
            }
        }

        /// <summary>
        /// Ajoute un élément. Non supporté.
        /// </summary>
        /// <param name="item">Message.</param>
        void ICollection<ErrorMessage>.Add(ErrorMessage item) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Supprime tous les éléments de la liste. Non supporté.
        /// </summary>
        void ICollection<ErrorMessage>.Clear() {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Teste si la collection contient un item.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <returns>Non supporté.</returns>
        bool ICollection<ErrorMessage>.Contains(ErrorMessage item) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copie la collection. Non supporté.
        /// </summary>
        /// <param name="array">Tableau de sortie.</param>
        /// <param name="arrayIndex">Position d'écriture.</param>
        void ICollection<ErrorMessage>.CopyTo(ErrorMessage[] array, int arrayIndex) {
            _entryList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Retourne un énumérateur sur la collection.
        /// </summary>
        /// <returns>Enumérateur.</returns>
        IEnumerator<ErrorMessage> IEnumerable<ErrorMessage>.GetEnumerator() {
            return _entryList.GetEnumerator();
        }

        /// <summary>
        /// Retourne un énumérateur sur la collection.
        /// </summary>
        /// <returns>Enumérateur.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return _entryList.GetEnumerator();
        }

        /// <summary>
        /// Supprime un élément de la collection.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <returns>Non supporté.</returns>
        bool ICollection<ErrorMessage>.Remove(ErrorMessage item) {
            throw new NotSupportedException();
        }
    }
}
