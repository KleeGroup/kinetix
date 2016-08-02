using System;

namespace Kinetix.Search.ComponentModel {

    /// <summary>
    /// Attribut de décoration du nom de type d'un document de recherche dans l'index.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SearchDocumentTypeAttribute : Attribute {

        /// <summary>
        /// Créé une nouvelle instance de SearchDocumentTypeAttribute.
        /// </summary>
        /// <param name="documentTypeName">Nom du type de document.</param>
        public SearchDocumentTypeAttribute(string documentTypeName) {
            this.DocumentTypeName = documentTypeName;
        }

        /// <summary>
        /// Nom du type de document.
        /// </summary>
        public string DocumentTypeName {
            get;
            private set;
        }
    }
}
