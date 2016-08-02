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
using System.Collections.Generic;

using log4net;

namespace Kinetix.Caching.Store {
    /// <summary>
    /// An abstract class for the Memory Stores. All Memory store implementations for different
    /// policies (e.g: FIFO, LFU, LRU, etc.) should extend this class.
    /// </summary>
    internal abstract class MemoryStore : IStore {
        private static readonly ILog _log = LogManager.GetLogger(typeof(MemoryStore).Name);

        private IStore _diskStore;

        /// <summary>
        /// Constructs things that all MemoryStores have in common.
        /// </summary>
        /// <param name="cache">Parent Cache.</param>
        /// <param name="diskStore">Associated Disk store.</param>
        protected MemoryStore(IEhcache cache, IStore diskStore) {
            this._diskStore = diskStore;
            this.Cache = cache;

            if (_log.IsDebugEnabled) {
                _log.Debug("Initialized " + this.GetType().Name + " for " + cache.Name);
            }
        }

        /// <summary>
        /// The cache this store is associated with.
        /// </summary>
        protected IEhcache Cache {
            get;
            set;
        }

        /// <summary>
        /// Map where items are stored by key.
        /// </summary>
        protected IDictionary<object, Element> Map {
            get;
            set;
        }

        /// <summary>
        /// Prepares for shutdown.
        /// </summary>
        public void Dispose() {
            lock (this) {
                this.Flush();

                this.Cache = null;
                _diskStore = null;
            }
        }

        /// <summary>
        /// Flush to disk.
        /// </summary>
        public void Flush() {
            lock (this) {
                if (this.Cache.IsOverflowToDisk) {
                    if (_log.IsDebugEnabled) {
                        _log.Debug(this.Cache.Name + " is persistent. Spooling " + this.Map.Count + " elements to the disk store.");
                    }

                    this.SpoolAllToDisk();
                    this.Clear();
                }
            }
        }

        /// <summary>
        /// Gets an item from the cache.
        ///
        /// The last access time in Element is updated.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The element, or null if there was no match for the key.</returns>
        public Element Get(object key) {
            return GetInternal(key, true);
        }

        /// <summary>
        /// Gets an item from the cache, without updating Element statistics.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>The element, or null if there was no match for the key.</returns>
        public Element GetQuiet(object key) {
            return GetInternal(key, false);
        }

        /// <summary>
        /// Puts an item in the cache.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public void Put(Element element) {
            lock (this) {
                if (element != null) {
                    this.Map[element.Key] = element;
                }
            }
        }

        /// <summary>
        /// Removes an Element from the store.
        /// </summary>
        /// <param name="key">The key of the Element, usually a String.</param>
        /// <returns>The Element if one was found, else null.</returns>
        public Element Remove(object key) {
            lock (this) {
                Element element = null;
                if (this.Map.TryGetValue(key, out element)) {
                    this.Map.Remove(key);
                } else if (_log.IsDebugEnabled) {
                    _log.Debug(this.Cache.Name + "Cache: Cannot remove entry as key " + key + " was not found");
                }

                return element;
            }
        }

        /// <summary>
        /// Remove all of the elements from the store.
        /// </summary>
        public void RemoveAll() {
            lock (this) {
                this.Clear();
            }
        }

        /// <summary>
        /// An unsynchronized check to see if a key is in the Store. No check is made to see if the Element is expired.
        /// </summary>
        /// <param name="key">The Element key.</param>
        /// <returns>
        /// True if found. If this method return false, it means that an Element with the given key is definitely not in the MemoryStore.
        /// If it returns true, there is an Element there. An attempt to get it may return null if the Element has expired.
        /// </returns>
        public bool ContainsKey(object key) {
            return this.Map.ContainsKey(key);
        }

        /// <summary>
        /// A factory method to create a MemoryStore.
        /// </summary>
        /// <param name="cache">Parent Cache.</param>
        /// <param name="diskStore">Associated Disk store.</param>
        /// <returns>Nouvelle instance.</returns>
        internal static MemoryStore Create(IEhcache cache, IStore diskStore) {
            MemoryStore memoryStore = null;
            MemoryStoreEvictionPolicy policy = cache.MemoryStoreEvictionPolicy;

            switch (policy) {
                case MemoryStoreEvictionPolicy.Lru:
                    memoryStore = new LruMemoryStore(cache, diskStore);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return memoryStore;
        }

        /// <summary>
        /// Evict the Element.
        ///
        /// Evict means that the Element is:
        /// - if, the store is diskPersistent, the Element is spooled to the DiskStore
        /// - if not, the Element is removed.
        /// </summary>
        /// <param name="element">Element to evict.</param>
        internal void Evict(Element element) {
            bool spooled = false;
            if (this.Cache.IsOverflowToDisk) {
                if (!element.IsSerializable) {
                    if (_log.IsDebugEnabled) {
                        _log.Debug("Object with key " + element.Key + " is not Serializable and cannot be overflowed to disk");
                    }
                } else {
                    this.SpoolToDisk(element);
                    spooled = true;
                }
            }

            if (!spooled) {
                this.Cache.NotificationService.NotifyElementEvicted(element, false);
            }
        }

        /// <summary>
        /// Clears any data structures and places it back to its state when it was first created.
        /// </summary>
        protected void Clear() {
            this.Map.Clear();
        }

        /// <summary>
        /// Before eviction elements are checked.
        /// </summary>
        /// <param name="element">Expiry element.</param>
        protected void NotifyExpiry(Element element) {
            this.Cache.NotificationService.NotifyElementExpiry(element, false);
        }

        /// <summary>
        /// Spools all elements to disk, in preparation for shutdown.
        ///
        /// Relies on being called from a synchronized method.
        ///
        /// This revised implementation is a little slower but avoids using increased memory during the method.
        /// </summary>
        protected void SpoolAllToDisk() {
            KeyValuePair<object, Element>[] elements = new KeyValuePair<object, Element>[this.Map.Count];
            this.Map.CopyTo(elements, 0);
            for (int i = 0; i < elements.Length; i++) {
                Element element = elements[i].Value;
                if (!element.IsSerializable) {
                    if (_log.IsDebugEnabled) {
                        _log.Debug("Object with key " + element.Key
                                + " is not Serializable and is not being overflowed to disk.");
                    }
                } else {
                    this.SpoolToDisk(element);
                    this.Remove(elements[i].Key);
                }
            }
        }

        /// <summary>
        /// Puts the element in the DiskStore.
        /// Should only be called if Ehcache.IsOverflowToDisk is true
        ///
        /// Relies on being called from a synchronized method.
        /// </summary>
        /// <param name="element">The Element.</param>
        protected virtual void SpoolToDisk(Element element) {
            _diskStore.Put(element);
            if (_log.IsDebugEnabled) {
                _log.Debug(this.Cache.Name + "Cache: spool to disk done for: " + element.Key);
            }
        }

        /// <summary>
        /// Gets an item from the cache, without updating Element statistics.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="updateStatistics">With or without updating Element statistics.</param>
        /// <returns>The element, or null if there was no match for the key.</returns>
        private Element GetInternal(object key, bool updateStatistics) {
            lock (this) {
                Element element = this.Map[key];

                if (element != null) {
                    if (updateStatistics) {
                        element.UpdateAccessStatistics();
                    }

                    if (_log.IsDebugEnabled) {
                        _log.Debug(this.Cache.Name + "Cache: " + this.Cache.Name + "MemoryStore hit for " + key);
                    }
                } else if (_log.IsDebugEnabled) {
                    _log.Debug(this.Cache.Name + "Cache: " + this.Cache.Name + "MemoryStore miss for " + key);
                }

                return element;
            }
        }
    }
}
