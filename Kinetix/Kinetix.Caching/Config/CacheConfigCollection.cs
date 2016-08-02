using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Kinetix.Caching.Config {
    /// <summary>
    /// Collection de CacheConfigElement.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface", Justification = "API Microsoft")]
    public class CacheConfigCollection : ConfigurationElementCollection {

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        public CacheConfigCollection() {
        }

        /// <summary>
        /// Retourne la configuration.
        /// </summary>
        /// <param name="name">Nom du cache.</param>
        /// <returns>Element de configuration.</returns>
        public new CacheConfigElement this[string name] {
            get {
                return (CacheConfigElement)BaseGet(name);
            }
        }

        /// <summary>
        /// Supprime un élément de configuration.
        /// </summary>
        /// <param name="cacheConfig">Element de configuration.</param>
        public void Remove(CacheConfigElement cacheConfig) {
            if (cacheConfig == null) {
                throw new ArgumentNullException("cacheConfig");
            }

            if (BaseIndexOf(cacheConfig) >= 0) {
                BaseRemove(cacheConfig.Name);
            }
        }

        /// <summary>
        /// Supprime la configuration.
        /// </summary>
        /// <param name="name">Nom de l'élément.</param>
        public void Remove(string name) {
            BaseRemove(name);
        }

        /// <summary>
        /// Crée une nouvelle élément.
        /// </summary>
        /// <returns>CacheConfigElement.</returns>
        public CacheConfigElement NewElement() {
            return (CacheConfigElement)this.CreateNewElement();
        }

        /// <summary>
        /// Crée un nouvel élément correspondant à la configuration d'un cache.
        /// </summary>
        /// <param name="elementName">Nom du cache.</param>
        /// <returns>CacheConfigElement.</returns>
        protected override ConfigurationElement CreateNewElement(string elementName) {
            return new CacheConfigElement(elementName);
        }

        /// <summary>
        /// Retourne le nom du cache.
        /// </summary>
        /// <param name="element">Element de configuration.</param>
        /// <returns>Nom du cache.</returns>
        protected override object GetElementKey(ConfigurationElement element) {
            return ((CacheConfigElement)element).Name;
        }

        /// <summary>
        /// Crée un nouvelle élément de configuration.
        /// </summary>
        /// <returns>Retourne un nouvel élément pour la collection.</returns>
        protected override ConfigurationElement CreateNewElement() {
            return new CacheConfigElement();
        }
    }
}
