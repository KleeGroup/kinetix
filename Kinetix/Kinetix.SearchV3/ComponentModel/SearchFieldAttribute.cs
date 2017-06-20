using System;

namespace Kinetix.Search.ComponentModel {

    /// <summary>
    /// Attribut de décoration de propriété de document de moteur de recherche.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SearchFieldAttribute : Attribute {

        /// <summary>
        /// Créé une nouvelle instance de SearchFieldAttribute.
        /// </summary>
        /// <param name="category">Catégorie.</param>
        public SearchFieldAttribute(SearchFieldCategory category) {
            this.Category = category;
        }

        /// <summary>
        /// Catégorie du champ.
        /// </summary>
        public SearchFieldCategory Category {
            get;
            private set;
        }
    }
}
