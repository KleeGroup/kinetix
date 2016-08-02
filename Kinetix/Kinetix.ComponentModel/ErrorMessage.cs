using System.Globalization;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Entrée d'une pile d'erreur.
    /// </summary>
    public sealed class ErrorMessage {

        /// <summary>
        /// Crée une nouvelle entrée.
        /// </summary>
        /// <param name="message">Message d'erreur.</param>
        /// <param name="code">Le code d'erreur.</param>
        public ErrorMessage(string message, string code = null) {
            Message = message;
            Code = code;
        }

        /// <summary>
        /// Crée une nouvelle entrée.
        /// </summary>
        /// <param name="fieldName">Nom du champ.</param>
        /// <param name="message">Message d'erreur.</param>
        /// <param name="code">Le code d'erreur.</param>
        internal ErrorMessage(string fieldName, string message, string code) {
            FieldName = fieldName;
            Message = message;
            Code = code;
        }

        /// <summary>
        /// Le code de l'erreur.
        /// </summary>
        public string Code {
            get;
            private set;
        }

        /// <summary>
        /// Nom du champ en erreur.
        /// </summary>
        public string FieldName {
            get;
            private set;
        }

        /// <summary>
        /// Nom complet.
        /// </summary>
        public string FullFieldName {
            get {
                string fullFieldName;
                if (string.IsNullOrEmpty(this.ModelName)) {
                    fullFieldName = this.FieldName;
                } else {
                    fullFieldName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", this.ModelName, this.FieldName);
                }

                return fullFieldName;
            }
        }

        /// <summary>
        /// Message d'erreur.
        /// </summary>
        public string Message {
            get;
            private set;
        }

        /// <summary>
        /// Nom du modèle concerné par l'erreur.
        /// </summary>
        public string ModelName {
            get;
            set;
        }
    }
}
