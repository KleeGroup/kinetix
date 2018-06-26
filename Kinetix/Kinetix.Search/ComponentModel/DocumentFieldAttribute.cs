using System;

namespace Kinetix.Search.ComponentModel
{

    /// <summary>
    /// Attribut de décoration de propriété de document de moteur de recherche.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DocumentFieldAttribute : Attribute
    {

        /// <summary>
        /// Créé une nouvelle instance de DocumentFieldCategory.
        /// </summary>
        /// <param name="category">Catégorie.</param>
        public DocumentFieldAttribute(DocumentFieldCategory category)
        {
            this.Category = category;
        }

        /// <summary>
        /// Catégorie du champ.
        /// </summary>
        public DocumentFieldCategory Category
        {
            get;
            private set;
        }
    }
}
