using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Exception en cas de violation de contrainte.
    /// </summary>
    [Serializable]
    public class ConstraintException : Exception {

        /// <summary>
        /// Inner message parameters.
        /// </summary>
        private Dictionary<string, ErrorMessageParameter> _messageParameters = new Dictionary<string, ErrorMessageParameter>();

        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        public ConstraintException() {
        }

        /// <summary>
        /// Crée un nouvelle exception.
        /// </summary>
        /// <param name="errorCollection">Pile d'erreur.</param>
        public ConstraintException(ErrorMessageCollection errorCollection) {
            this.Errors = errorCollection;
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        public ConstraintException(string message)
            : base(message) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="messageList">Liste de messages d'erreurs.</param>
        public ConstraintException(IEnumerable<string> messageList) {
            var erreurs = new ErrorMessageCollection();
            foreach (var message in messageList) {
                erreurs.AddConstraintException(message);
            }

            Errors = erreurs;
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="messageList">Liste de messages d'erreurs.</param>
        /// <param name="code">Le code de l'erreur.</param>
        public ConstraintException(IEnumerable<ErrorMessage> messageList, string code = null) {
            Errors = new ErrorMessageCollection(messageList);
            Code = code;
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="property">Propriété associée à la violation de contrainte.</param>
        /// <param name="message">Description de l'exception.</param>
        public ConstraintException(BeanPropertyDescriptor property, string message)
            : base(message) {
            this.Property = property;
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="fieldName">Nom du champ en erreur.</param>
        /// <param name="message">Message d'erreur.</param>
        public ConstraintException(string fieldName, string message) {
            this.Errors = new ErrorMessageCollection();
            this.Errors.AddEntry(fieldName, message);
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="messageParameters">Message parameters.</param>
        public ConstraintException(string message, Dictionary<string, ErrorMessageParameter> messageParameters)
            : base(message) {
            this._messageParameters = messageParameters;
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public ConstraintException(string message, Exception innerException)
            : base(message, innerException) {
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="property">Propriété associée à la violation de contrainte.</param>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="innerException">Exception source.</param>
        public ConstraintException(BeanPropertyDescriptor property, string message, Exception innerException)
            : base(message, innerException) {
            this.Property = property;
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="fieldName">Nom du champ en erreur.</param>
        /// <param name="message">Message d'erreur.</param>
        /// <param name="code">Code d'erreur.</param>
        public ConstraintException(string fieldName, string message, string code)
            : this(fieldName, message) {
            this.Code = code;
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="message">Description de l'exception.</param>
        /// <param name="code">Code d'erreur.</param>
        /// <param name="innerException">Exception source.</param>
        public ConstraintException(string message, string code, Exception innerException)
            : base(message, innerException) {
            this.Code = code;
        }

        /// <summary>
        /// Crée une nouvelle exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        protected ConstraintException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
            if (info != null) {
                this.Property = (BeanPropertyDescriptor)info.GetValue("Property", typeof(BeanPropertyDescriptor));
            }
        }

        /// <summary>
        /// Code d'erreur.
        /// </summary>
        public string Code {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la pile des erreurs.
        /// </summary>
        public ErrorMessageCollection Errors {
            get;
            private set;
        }

        /// <summary>
        /// List of parameters to inject in the message describing the exception.
        /// </summary>
        public Dictionary<string, ErrorMessageParameter> MessageParameters {
            get {
                return _messageParameters;
            }
        }

        /// <summary>
        /// Retourne la propriété associée à la violation de contrainte.
        /// </summary>
        public BeanPropertyDescriptor Property {
            get;
            private set;
        }

        /// <summary>
        /// Sérialise l'exception.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info != null) {
                base.GetObjectData(info, context);
                info.AddValue("Property", this.Property);
            }
        }
    }
}
