using System;
using System.Configuration;

namespace Kinetix.Search.Config {

    /// <summary>
    /// Liste de DataSource.
    /// </summary>
    public class SearchDataSourceCollection : ConfigurationElementCollection {

        /// <summary>
        /// Nom de l'élement de configuration.
        /// </summary>
        internal const string PropertyName = "dataSource";

        /// <inheritdoc cref="ConfigurationElementCollection.CollectionType"/>
        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }

        /// <inheritdoc cref="ConfigurationElementCollection.ElementName"/>
        protected override string ElementName {
            get {
                return PropertyName;
            }
        }

        /// <summary>
        /// Renvoie l'élément pour un nom donné.
        /// </summary>
        /// <param name="name">Nom de l'élément.</param>
        /// <returns>DataSource.</returns>
        public new SearchDataSourceElement this[string name] {
            get {
                return (SearchDataSourceElement)BaseGet(name);
            }
        }

        /// <inheritdoc cref="ConfigurationElementCollection.IsReadOnly"/>
        public override bool IsReadOnly() {
            return false;
        }

        /// <inheritdoc cref="ConfigurationElementCollection.IsElementName"/>
        protected override bool IsElementName(string elementName) {
            return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <inheritdoc cref="ConfigurationElementCollection.CreateNewElement"/>
        protected override ConfigurationElement CreateNewElement() {
            return new SearchDataSourceElement();
        }

        /// <inheritdoc cref="ConfigurationElementCollection.GetElementKey"/>
        protected override object GetElementKey(ConfigurationElement element) {
            return ((SearchDataSourceElement)element).Name;
        }
    }
}
