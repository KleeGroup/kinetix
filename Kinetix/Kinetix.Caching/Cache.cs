/*
 *  Copyright 2003-2007 Greg Luck
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Kinetix.Caching.Config;
using Kinetix.Caching.Store;
using log4net;

namespace Kinetix.Caching {
    /// <summary>
    /// Cache un ensemble d'élément de même nature.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "API EHCache.")]
    public sealed class Cache : IEhcache, ICacheEventNotificationService, IDisposable {
        /// <summary>
        /// A reserved word for cache names. It denotes a default configuration
        /// which is applied to caches created without configuration.
        /// </summary>
        internal const string DefaultCacheName = "default";

        /// <summary>
        /// The default interval between runs of the expiry thread.
        /// </summary>
        internal const long DefaultExpiryThreadIntervalSeconds = 120;

        /// <summary>
        /// Set a buffer size for the spool of approx 30MB.
        /// </summary>
        internal const int DefaultSpoolBufferSize = 30;

        private const MemoryStoreEvictionPolicy DefaultMemoryStoreEvictionPolicy = MemoryStoreEvictionPolicy.Lru;

        /// <summary>
        /// The amount of time to wait if a store gets backed up.
        /// </summary>
        private const int BackOffTimeMillis = 50;

        /// <summary>
        /// Logger.
        /// </summary>
        private static readonly ILog _log = LogManager.GetLogger(typeof(Cache).Name);

        private DiskStore _diskStore;
        private Status _status;

        /// <summary>
        /// Configuration du cache.
        /// </summary>
        private CacheConfiguration _configuration;

        /// <summary>
        /// Cache hit count.
        /// </summary>
        private long _hitCount;

        /// <summary>
        /// Memory cache hit count.
        /// </summary>
        private long _memoryStoreHitCount;

        /// <summary>
        /// DiskStore hit count.
        /// </summary>
        private long _diskStoreHitCount;

        /// <summary>
        /// Count of misses where element was not found.
        /// </summary>
        private long _missCountNotFound;

        /// <summary>
        /// Count of misses where element was expired.
        /// </summary>
        private long _missCountExpired;

        /// <summary>
        /// The MemoryStore of this Cache. All caches have a memory store.
        /// </summary>
        private MemoryStore _memoryStore;

        private Guid _guid;

        /// <summary>
        /// Create a new instance
        /// Only the CacheManager can initialise them.
        /// </summary>
        /// <param name="name">The name of the cache. Note that "default" is a reserved name for the defaultCache.</param>
        /// <param name="maxElementsInMemory">The maximum number of elements in memory, before they are evicted.</param>
        /// <param name="memoryStoreEvictionPolicy">One of LRU, LFU and FIFO. Optionally null, in which case it will be set to LRU.</param>
        /// <param name="overflowToDisk">Whether to use the disk store.</param>
        /// <param name="diskStorePath">This parameter is ignored. CacheManager sets it using setter injection.</param>
        /// <param name="eternal">Whether the elements in the cache are eternal, i.e. never expire.</param>
        /// <param name="timeToLiveSeconds">The default amount of time to live for an element from its creation date.</param>
        /// <param name="timeToIdleSeconds">The default amount of time to live for an element from its last accessed or modified date.</param>
        /// <param name="diskPersistent">Whether to persist the cache to disk between JVM restarts.</param>
        /// <param name="diskExpiryThreadIntervalSeconds">How often to run the disk store expiry thread. A large number of 120 seconds plus is recommended.</param>
        /// <param name="maxElementsOnDisk">The maximum number of elements on disk, before they are evicted.</param>
        /// <param name="diskSpoolBufferSizeMB">The amount of memory to allocate the write buffer for puts to the DiskStore.</param>
        internal Cache(
            string name,
            int maxElementsInMemory,
            MemoryStoreEvictionPolicy memoryStoreEvictionPolicy,
            bool overflowToDisk,
            string diskStorePath,
            bool eternal,
            long timeToLiveSeconds,
            long timeToIdleSeconds,
            bool diskPersistent,
            long diskExpiryThreadIntervalSeconds,
            int maxElementsOnDisk,
            int diskSpoolBufferSizeMB) {

            this.ChangeStatus(Status.Uninitialized);

            _guid = Guid.NewGuid();

            _configuration = new CacheConfiguration();
            _configuration.Name = name;
            _configuration.MaxElementsInMemory = maxElementsInMemory;
            _configuration.EvictionPolicy = memoryStoreEvictionPolicy;
            _configuration.IsOverflowToDisk = overflowToDisk;
            _configuration.IsEternal = eternal;
            _configuration.TimeToLiveSeconds = timeToLiveSeconds;
            _configuration.TimeToIdleSeconds = timeToIdleSeconds;
            _configuration.DiskPersistent = diskPersistent;
            _configuration.MaxElementsOnDisk = maxElementsOnDisk;

            if (diskStorePath == null) {
                _configuration.DiskStorePath = ".";
            } else {
                _configuration.DiskStorePath = diskStorePath;
            }

            // Set this to a safe value.
            if (diskExpiryThreadIntervalSeconds == 0) {
                _configuration.DiskExpiryThreadIntervalSeconds = DefaultExpiryThreadIntervalSeconds;
            } else {
                _configuration.DiskExpiryThreadIntervalSeconds = diskExpiryThreadIntervalSeconds;
            }

            if (diskSpoolBufferSizeMB == 0) {
                _configuration.DiskSpoolBufferSizeMB = DefaultSpoolBufferSize;
            } else {
                _configuration.DiskSpoolBufferSizeMB = diskSpoolBufferSizeMB;
            }
        }

        /// <summary>
        /// Survient quand tous les éléments sont supprimés.
        /// </summary>
        public event EventHandler AllRemoved;

        /// <summary>
        /// Survient lors de l'arrêt du cache.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Survient suite à l'écriture dans le cache disque.
        /// </summary>
        public event EventHandler BackupComplete;

        /// <summary>
        /// Survient quand un élément est retiré du cache.
        /// </summary>
        public event EventHandler<CacheEventArgs> ElementEvicted;

        /// <summary>
        /// Survient quand un élément expiré est retiré du cache.
        /// </summary>
        public event EventHandler<CacheEventArgs> ElementExpiry;

        /// <summary>
        /// Survient quand un élément est ajouté au cache.
        /// </summary>
        public event EventHandler<CacheEventArgs> ElementPut;

        /// <summary>
        /// Survient quand un élément est supprimé du cache.
        /// </summary>
        public event EventHandler<CacheEventArgs> ElementRemoved;

        /// <summary>
        /// Survient quand un élément est mise à jour.
        /// </summary>
        public event EventHandler<CacheEventArgs> ElementUpdate;

        /// <summary>
        /// Gets the cache configuration this cache was created with.
        /// </summary>
        public CacheConfiguration Configuration {
            get {
                return _configuration;
            }
        }

        /// <summary>
        /// Guid de l'instance du cache.
        /// </summary>
        public Guid CacheGuid {
            get {
                return _guid;
            }
        }

        /// <summary>
        /// Does the overflow go to disk.
        /// </summary>
        public bool IsOverflowToDisk {
            get {
                return _configuration.IsOverflowToDisk;
            }
        }

        /// <summary>
        /// Gets the maximum number of elements to hold in memory.
        /// </summary>
        public int MaxElementsInMemory {
            get {
                return _configuration.MaxElementsInMemory;
            }
        }

        /// <summary>
        /// The policy used to evict elements from the MemoryStore.
        /// The default value is LRU.
        /// </summary>
        public MemoryStoreEvictionPolicy MemoryStoreEvictionPolicy {
            get {
                return _configuration.EvictionPolicy;
            }
        }

        /// <summary>
        /// Gets the cache name.
        /// </summary>
        public string Name {
            get {
                return _configuration.Name;
            }
        }

        /// <summary>
        /// Retourne le service de notification.
        /// </summary>
        public ICacheEventNotificationService NotificationService {
            get {
                return this;
            }
        }

        /// <summary>
        /// Flushes all cache items from memory to auxilliary caches and close the auxilliary caches.
        /// Should be invoked only by CacheManager.
        /// </summary>
        public void Dispose() {
            try {
                lock (this) {
                    if (_memoryStore != null) {
                        _memoryStore.Dispose();
                        _memoryStore = null;
                    }

                    if (_diskStore != null) {
                        _diskStore.Dispose();
                        _diskStore = null;
                    }

                    _status = Status.Shutdown;
                }

                this.NotificationService.NotifyDisposed(false);
            } finally {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Gets an element from the cache. Updates Element Statistics.
        ///
        /// Note that the Element's lastAccessTime is always the time of this get.
        /// Use GetQuiet(object) to peak into the Element to see its last access time with get.
        /// </summary>
        /// <param name="key">Key a serializable value.</param>
        /// <returns>The element, or null, if it does not exist.</returns>
        public Element Get(object key) {
            this.CheckStatus();
            Element element = null;
            lock (this) {
                element = this.SearchInMemoryStore(key, true);
                if (element == null && _configuration.IsOverflowToDisk) {
                    element = this.SearchInDiskStore(key, true);
                }

                if (element == null) {
                    _missCountNotFound++;
                    if (_log.IsInfoEnabled) {
                        _log.Info(_configuration.Name + " cache - Miss");
                    }
                } else {
                    _hitCount++;
                }
            }

            return element;
        }

        /// <summary>
        /// Gets an element from the cache, without updating Element statistics. Cache statistics are
        /// not updated.
        /// </summary>
        /// <param name="key">Key a serializable value.</param>
        /// <returns>The element, or null, if it does not exist.</returns>
        public Element GetQuiet(object key) {
            this.CheckStatus();
            Element element = null;
            lock (this) {
                element = this.SearchInMemoryStore(key, false);
                if (element == null && _configuration.IsOverflowToDisk) {
                    element = this.SearchInDiskStore(key, false);
                }
            }

            return element;
        }

        /// <summary>
        /// Whether an Element is stored in the cache on Disk, indicating a higher cost of retrieval.
        /// </summary>
        /// <param name="key">True if an element matching the key is found in the diskStore.</param>
        /// <returns>True if an element matching the key is found on disk.</returns>
        public bool IsElementOnDisk(object key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            this.CheckStatus();
            if (_configuration.IsOverflowToDisk) {
                return _diskStore != null && _diskStore.ContainsKey(key);
            } else {
                return false;
            }
        }

        /// <summary>
        /// Whether an Element is stored in the cache in Memory, indicating a very low cost of retrieval.
        /// </summary>
        /// <param name="key">Element Key.</param>
        /// <returns>True if an element matching the key is found in memory.</returns>
        public bool IsElementInMemory(object key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            this.CheckStatus();
            return _memoryStore.ContainsKey(key);
        }

        /// <summary>
        /// Checks whether this cache element has expired.
        /// </summary>
        /// <param name="element">Element to check.</param>
        /// <returns>True if it has expired.</returns>
        public bool IsExpired(Element element) {
            if (element == null) {
                throw new ArgumentNullException("element");
            }

            this.CheckStatus();
            lock (element) {
                return element.IsExpired;
            }
        }

        /// <summary>
        /// Put an element in the cache.
        ///
        /// Resets the access statistics on the element, which would be the case if it has previously been
        /// gotten from a cache, and is now being put back.
        ///
        /// Also notifies the CacheEventListener that:
        ///
        /// - the element was put, but only if the Element was actually put.
        /// - if the element exists in the cache, that an update has occurred, even if the element would be expired
        /// if it was requested.
        /// </summary>
        /// <param name="element">An object. If Serializable it can fully participate in replication and the DiskStore.</param>
        public void Put(Element element) {
            if (element == null) {
                throw new ArgumentNullException("element");
            }

            this.CheckStatus();
            element.ResetAccessStatistics();
            object key = element.Key;
            bool elementExists = IsElementInMemory(key) || IsElementOnDisk(key);
            if (elementExists) {
                element.UpdateUpdateStatistics();
            }

            this.ApplyDefaultsToElementWithoutLifespanSet(element);
            this.BackOffIsDiskSpoolFull();

            lock (this) {
                _memoryStore.Put(element);
            }

            if (elementExists) {
                this.NotificationService.NotifyElementUpdate(element, false);
            } else {
                this.NotificationService.NotifyElementPut(element, false);
            }
        }

        /// <summary>
        /// Put an element in the cache, without updating statistics, or updating listeners. This is meant to be used
        /// in conjunction with GetQuiet.
        /// </summary>
        /// <param name="element">An object. If Serializable it can fully participate in replication and the DiskStore.</param>
        public void PutQuiet(Element element) {
            if (element == null) {
                throw new ArgumentNullException("element");
            }

            this.CheckStatus();
            this.ApplyDefaultsToElementWithoutLifespanSet(element);

            lock (this) {
                _memoryStore.Put(element);
            }
        }

        /// <summary>
        /// Removes an Element from the Cache. This also removes it from any
        /// stores it may be in.
        ///
        /// Also notifies the CacheEventListener after the element was removed, but only if an Element
        /// with the key actually existed.
        /// </summary>
        /// <param name="key">The element key to operate on.</param>
        /// <returns>If the element was removed, false if it was not found in the cache.</returns>
        public bool Remove(object key) {
            return this.Remove(key, false, true);
        }

        /// <summary>
        /// Removes all cached items.
        /// </summary>
        public void RemoveAll() {
            this.CheckStatus();
            lock (this) {
                _memoryStore.RemoveAll();
                if (_configuration.IsOverflowToDisk) {
                    _diskStore.RemoveAll();
                }
            }

            this.NotificationService.NotifyRemoveAll(false);
        }

        /// <summary>
        /// Removes an Element from the Cache. This also removes it from any
        /// stores it may be in.
        /// </summary>
        /// <param name="key">The element key to operate on.</param>
        /// <returns>If the element was removed, false if it was not found in the cache.</returns>
        public bool RemoveQuiet(object key) {
            return this.Remove(key, false, false);
        }

        /// <summary>
        /// Indique si l'évènement est écouté.
        /// </summary>
        /// <returns>True.</returns>
        bool ICacheEventNotificationService.HasElementEvictedListeners() {
            return ElementEvicted != null;
        }

        /// <summary>
        /// Indique si l'évènement est écouté.
        /// </summary>
        /// <returns>True.</returns>
        bool ICacheEventNotificationService.HasElementExpiredListeners() {
            return ElementExpiry != null;
        }

        /// <summary>
        /// Notifie l'arrêt du cache.
        /// </summary>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyDisposed(bool remoteEvent) {
            if (Disposed != null) {
                Disposed(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Notifie l'éviction d'un élément.
        /// </summary>
        /// <param name="element">Evicted Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyElementEvicted(Element element, bool remoteEvent) {
            if (ElementEvicted != null) {
                ElementEvicted(this, new CacheEventArgs(element, remoteEvent));
            }
        }

        /// <summary>
        /// Notifie l'expiration d'un élément.
        /// </summary>
        /// <param name="element">Expiry Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyElementExpiry(Element element, bool remoteEvent) {
            if (ElementExpiry != null) {
                ElementExpiry(this, new CacheEventArgs(element, remoteEvent));
            }
        }

        /// <summary>
        /// Notifie l'ajout d'un élément.
        /// </summary>
        /// <param name="element">Added Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyElementPut(Element element, bool remoteEvent) {
            if (ElementPut != null) {
                ElementPut(this, new CacheEventArgs(element, remoteEvent));
            }
        }

        /// <summary>
        /// Notifie la suppression d'un élément.
        /// </summary>
        /// <param name="element">Removed Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyElementRemoved(Element element, bool remoteEvent) {
            if (ElementRemoved != null) {
                ElementRemoved(this, new CacheEventArgs(element, remoteEvent));
            }
        }

        /// <summary>
        /// Notifie la suppression de tous les éléments.
        /// </summary>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyRemoveAll(bool remoteEvent) {
            if (AllRemoved != null) {
                AllRemoved(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Notifie la mise à jour d'un élément.
        /// </summary>
        /// <param name="element">Updated Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void ICacheEventNotificationService.NotifyElementUpdate(Element element, bool remoteEvent) {
            if (ElementUpdate != null) {
                ElementUpdate(this, new CacheEventArgs(element, remoteEvent));
            }
        }

        /// <summary>
        /// Notifie la fin de l'écriture dans le cache disque.
        /// </summary>
        void ICacheEventNotificationService.NotifyBackupCompete() {
            if (BackupComplete != null) {
                BackupComplete(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Newly created caches do not have a MemoryStore or a DiskStore.
        ///
        /// This method creates those and makes the cache ready to accept elements.
        /// </summary>
        internal void Init() {
            lock (this) {
                if (_configuration.MaxElementsInMemory == 0) {
                    _configuration.MaxElementsInMemory = 1;
                    if (_log.IsWarnEnabled) {
                        _log.Warn(string.Format(
                            CultureInfo.InvariantCulture,
                            SR.WarnZeroMemoryStoreSize,
                            _configuration.Name));
                    }
                }

                _diskStore = this.CreateDiskStore();
                _memoryStore = MemoryStore.Create(this, _diskStore);
                this.ChangeStatus(Status.Alive);
            }

            if (_log.IsDebugEnabled) {
                _log.Debug("Initialised cache: " + _configuration.Name);
            }
        }

        /// <summary>
        /// Setting with Cache defaults.
        /// </summary>
        /// <param name="element">Element to configure.</param>
        private void ApplyDefaultsToElementWithoutLifespanSet(Element element) {
            if (!element.IsLifespanSet) {
                element.TimeToLive = (int)_configuration.TimeToLiveSeconds;
                element.TimeToIdle = (int)_configuration.TimeToIdleSeconds;
                element.Eternal = _configuration.IsEternal;
            }
        }

        /// <summary>
        /// Wait outside of synchronized block so as not to block readers
        /// If the disk store spool is full wait a short time to give it a chance to
        /// catch up.
        /// </summary>
        private void BackOffIsDiskSpoolFull() {
            if (_diskStore != null && _diskStore.BackedUp()) {
                // back off to avoid OutOfMemoryError
                Thread.Sleep(Cache.BackOffTimeMillis);
            }
        }

        /// <summary>
        /// Change cache status.
        /// </summary>
        /// <param name="status">New status.</param>
        private void ChangeStatus(Status status) {
            this._status = status;
        }

        /// <summary>
        /// Check cache status.
        /// </summary>
        private void CheckStatus() {
            if (!Status.Alive.Equals(_status)) {
                throw new CacheException(string.Format(
                    CultureInfo.InvariantCulture,
                    SR.ExceptionCacheNotAlive,
                    _configuration.Name));
            }
        }

        /// <summary>
        /// Creates a disk store.
        /// </summary>
        /// <returns>Disk Store.</returns>
        private DiskStore CreateDiskStore() {
            if (_configuration.IsOverflowToDisk) {
                return new DiskStore(this, _configuration.DiskStorePath);
            } else {
                return null;
            }
        }

        /// <summary>
        /// Search an object in diskStore.
        /// </summary>
        /// <param name="key">Element Key.</param>
        /// <param name="updateStatistics">True to update statistics.</param>
        /// <returns>Element or null if not found.</returns>
        private Element SearchInDiskStore(object key, bool updateStatistics) {
            Element element = null;
            if (updateStatistics) {
                element = _diskStore.Get(key);
            } else {
                element = _diskStore.GetQuiet(key);
            }

            if (element != null) {
                if (this.IsExpired(element)) {
                    if (_log.IsDebugEnabled) {
                        _log.Debug(_configuration.Name + " cache - Disk Store hit, but element expired");
                    }

                    _missCountExpired++;
                    this.Remove(key, true, true);
                    element = null;
                } else {
                    _diskStoreHitCount++;

                    // Put the item back into memory to preserve policies in the memory store and to save updated statistics
                    _memoryStore.Put(element);
                }
            }

            return element;
        }

        /// <summary>
        /// Search an object in memoryStore.
        /// </summary>
        /// <param name="key">Element key.</param>
        /// <param name="updateStatistics">True to update statistics.</param>
        /// <returns>Element or null if not found.</returns>
        private Element SearchInMemoryStore(object key, bool updateStatistics) {
            Element element = null;
            if (updateStatistics) {
                element = _memoryStore.Get(key);
            } else {
                element = _memoryStore.GetQuiet(key);
            }

            if (element != null) {
                if (this.IsExpired(element)) {
                    if (_log.IsDebugEnabled) {
                        _log.Debug(_configuration.Name + " cache - Memory cache hit, but element expired");
                    }

                    _missCountExpired++;
                    this.Remove(key, true, true);
                    element = null;
                } else {
                    _memoryStoreHitCount++;
                }
            }

            return element;
        }

        /// <summary>
        /// Removes an Element from the Cache. This also removes it from any
        /// stores it may be in.
        /// </summary>
        /// <param name="key">The element key to operate on.</param>
        /// <param name="expiry">If the reason this method is being called is to expire the element.</param>
        /// <param name="notifyListeners">Whether to notify listeners.</param>
        /// <returns>If the element was removed, false if it was not found in the cache.</returns>
        private bool Remove(object key, bool expiry, bool notifyListeners) {
            this.CheckStatus();

            bool removed = false;
            Element elementFromMemoryStore = null;
            Element elementFromDiskStore = null;
            lock (this) {
                elementFromMemoryStore = _memoryStore.Remove(key);
                if (_configuration.IsOverflowToDisk) {
                    elementFromDiskStore = _diskStore.Remove(key);
                }
            }

            if (elementFromMemoryStore != null) {
                if (notifyListeners) {
                    if (expiry) {
                        this.NotificationService.NotifyElementExpiry(elementFromMemoryStore, false);
                    } else {
                        this.NotificationService.NotifyElementRemoved(elementFromMemoryStore, false);
                    }
                }

                removed = true;
            }

            if (elementFromDiskStore != null) {
                if (notifyListeners) {
                    if (expiry) {
                        this.NotificationService.NotifyElementExpiry(elementFromDiskStore, false);
                    } else {
                        this.NotificationService.NotifyElementRemoved(elementFromDiskStore, false);
                    }
                }

                removed = true;
            }

            return removed;
        }
    }
}
