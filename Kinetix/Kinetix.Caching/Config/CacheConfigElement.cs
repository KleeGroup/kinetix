using System.ComponentModel;
using System.Configuration;

namespace Kinetix.Caching.Config {
    /// <summary>
    /// Element de configuration par défaut du cache.
    /// </summary>
    public sealed class CacheConfigElement : ConfigurationElement {
        /// <summary>
        /// Nom du cache par défaut.
        /// </summary>
        public const string DefaultCacheName = "default";
        private const string PropertyName = "name";
        private const string PropertyDiskExpiryThreadIntervalSeconds = "diskExpiryThreadIntervalSeconds";
        private const string PropertyDiskPersistent = "diskPersistent";
        private const string PropertyDiskSpoolBufferSizeMB = "diskSpoolBufferSizeMB";
        private const string PropertyMemoryStoreEvictionPolicy = "memoryStoreEvictionPolicy";
        private const string PropertyIsEternal = "isEternal";
        private const string PropertyIsOverflowToDisk = "isOverflowToDisk";
        private const string PropertyMaxElementsInMemory = "maxElementsInMemory";
        private const string PropertyMaxElementsOnDisk = "maxElementsOnDisk";
        private const string PropertyTimeToIdleSeconds = "timeToIdleSeconds";
        private const string PropertyTimeToLiveSeconds = "timeToLiveSeconds";

        /// <summary>
        /// Crée un nouvel élément correspondant à la configuration par défaut.
        /// </summary>
        public CacheConfigElement() {
            this.Name = DefaultCacheName;
        }

        /// <summary>
        /// Crée un nouvel élément correspondant à la configuration d'un cache.
        /// </summary>
        /// <param name="cacheName">Nom du cache.</param>
        public CacheConfigElement(string cacheName) {
            this.Name = cacheName;
        }

        /// <summary>
        /// The name of the cache.
        /// </summary>
        [ConfigurationProperty(PropertyName, IsKey = true, IsRequired = true)]
        [Description("Nom du cache ou default pour les valeurs par défaut")]
        public string Name {
            get {
                return (string)this[PropertyName];
            }

            set {
                this[PropertyName] = value;
            }
        }

        /// <summary>
        /// The interval in seconds between runs of the disk expiry thread.
        /// 10 minutes is the default.
        /// </summary>
        [ConfigurationProperty(PropertyDiskExpiryThreadIntervalSeconds, IsRequired = false, DefaultValue = 600L)]
        [Description("Nombre de secondes entre deux vérifications d'éléments disque expirés")]
        public long DiskExpiryThreadIntervalSeconds {
            get {
                return (long)this[PropertyDiskExpiryThreadIntervalSeconds];
            }

            set {
                this[PropertyDiskExpiryThreadIntervalSeconds] = value;
            }
        }

        /// <summary>
        /// For caches that overflow to disk, whether the disk cache persists between CacheManager instances.
        /// </summary>
        [ConfigurationProperty(PropertyDiskPersistent, IsRequired = false, DefaultValue = false)]
        [Description("Indique si le cache disque persiste après un redémarrage")]
        public bool DiskPersistent {
            get {
                return (bool)this[PropertyDiskPersistent];
            }

            set {
                this[PropertyDiskPersistent] = value;
            }
        }

        /// <summary>
        /// The size of the disk spool used to buffer writes.
        /// </summary>
        [ConfigurationProperty(PropertyDiskSpoolBufferSizeMB, IsRequired = false, DefaultValue = 2)]
        [Description("Taille du cache d'écriture disque en Mo")]
        public int DiskSpoolBufferSizeMB {
            get {
                return (int)this[PropertyDiskSpoolBufferSizeMB];
            }

            set {
                this[PropertyDiskSpoolBufferSizeMB] = value;
            }
        }

        /// <summary>
        /// Sets whether elements are eternal. If eternal, timeouts are ignored and the element
        /// is never expired.
        /// </summary>
        [ConfigurationProperty(PropertyIsEternal, IsRequired = false, DefaultValue = false)]
        [Description("Indique si les éléments sont éternels et n'expirent jamais")]
        public bool IsEternal {
            get {
                return (bool)this[PropertyIsEternal];
            }

            set {
                this[PropertyIsEternal] = value;
            }
        }

        /// <summary>
        /// Whether elements can overflow to disk when the in-memory cache
        /// has reached the set limit.
        /// </summary>
        [ConfigurationProperty(PropertyIsOverflowToDisk, IsRequired = false, DefaultValue = false)]
        [Description("Indique si le cache déborde sur disque")]
        public bool IsOverflowToDisk {
            get {
                return (bool)this[PropertyIsOverflowToDisk];
            }

            set {
                this[PropertyIsOverflowToDisk] = value;
            }
        }

        /// <summary>
        /// The maximum objects to be held in the MemoryStore.
        /// </summary>
        [ConfigurationProperty(PropertyMaxElementsInMemory, IsRequired = false, DefaultValue = 1000)]
        [Description("Nombre d'éléments maximum en mémoire")]
        public int MaxElementsInMemory {
            get {
                return (int)this[PropertyMaxElementsInMemory];
            }

            set {
                this[PropertyMaxElementsInMemory] = value;
            }
        }

        /// <summary>
        /// The maximum objects to be held in the DiskStore.
        /// </summary>
        [ConfigurationProperty(PropertyMaxElementsOnDisk, IsRequired = false, DefaultValue = 10000)]
        [Description("Nombre d'éléments maximum sur disque")]
        public int MaxElementsOnDisk {
            get {
                return (int)this[PropertyMaxElementsOnDisk];
            }

            set {
                this[PropertyMaxElementsOnDisk] = value;
            }
        }

        /// <summary>
        /// The policy used to evict elements from the MemoryStore.
        /// </summary>
        [ConfigurationProperty(PropertyMemoryStoreEvictionPolicy, IsRequired = false, DefaultValue = "Lru")]
        [Description("Politique d'évication du cache mémoire (lru, lfu ou fifo)")]
        public string MemoryStoreEvictionPolicy {
            get {
                return (string)this[PropertyMemoryStoreEvictionPolicy];
            }

            set {
                this[PropertyMemoryStoreEvictionPolicy] = value;
            }
        }

        /// <summary>
        /// The time to idle for an element before it expires. Is only used
        /// if the element is not eternal.A value of 0 means do not check for idling.
        /// </summary>
        [ConfigurationProperty(PropertyTimeToIdleSeconds, IsRequired = false, DefaultValue = 0L)]
        [Description("Temps d'inactivité d'un élément avant expiration (0 = pas d'expiration)")]
        public long TimeToIdleSeconds {
            get {
                return (long)this[PropertyTimeToIdleSeconds];
            }

            set {
                this[PropertyTimeToIdleSeconds] = value;
            }
        }

        /// <summary>
        /// Sets the time to idle for an element before it expires. Is only used
        /// if the element is not eternal. This attribute is optional in the configuration.
        /// A value of 0 means do not check time to live.
        /// </summary>
        [ConfigurationProperty(PropertyTimeToLiveSeconds, IsRequired = false, DefaultValue = 120L)]
        [Description("Temps de vie d'un élément avant expiration (0 = pas d'expiration)")]
        public long TimeToLiveSeconds {
            get {
                return (long)this[PropertyTimeToLiveSeconds];
            }

            set {
                this[PropertyTimeToLiveSeconds] = value;
            }
        }
    }
}
