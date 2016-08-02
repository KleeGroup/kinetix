using System.ComponentModel;
using System.Configuration;

namespace Kinetix.Caching.Config {
    /// <summary>
    /// Section de configuration du cache.
    /// </summary>
    public class CacheManagerConfigElement : ConfigurationSection {
        /// <summary>
        /// Nom de la section de configuration du cache.
        /// </summary>
        public const string CacheElementName = "cacheManager";
        private const string CollectionCaches = "caches";
        private const string PropertyDiskStorePath = "diskStorePath";

        /// <summary>
        /// Obtient ou définit le répertoire de stockage du
        /// cache disque.
        /// </summary>
        [ConfigurationProperty(PropertyDiskStorePath, DefaultValue = ".")]
        [Description("Répertoire de stockage du cache disque")]
        public string DiskStorePath {
            get {
                return (string)this[PropertyDiskStorePath];
            }

            set {
                this[PropertyDiskStorePath] = value;
            }
        }

        /// <summary>
        /// Collection de configuration des caches.
        /// </summary>
        [ConfigurationProperty(CollectionCaches, IsDefaultCollection = false)]
        public CacheConfigCollection Caches {
            get {
                CacheConfigCollection cachesCollection =
                        (CacheConfigCollection)this[CollectionCaches];
                return cachesCollection;
            }
        }
    }
}
