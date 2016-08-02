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

using log4net;

namespace Kinetix.Caching.Store {
    /// <summary>
    /// An implementation of a LruMemoryStore.
    ///
    /// This uses LinkedHashMap as its backing map. It uses the LinkedHashMap LRU
    /// feature. LRU for this implementation means least recently accessed.
    /// </summary>
    internal sealed class LruMemoryStore : MemoryStore {
        private static readonly ILog _log = LogManager.GetLogger(typeof(LruMemoryStore).Name);

        /// <summary>
        /// Constructor for the LruMemoryStore object.
        /// </summary>
        /// <param name="cache">Cache using this store.</param>
        /// <param name="diskStore">Associated DiskStore.</param>
        public LruMemoryStore(IEhcache cache, IStore diskStore)
            : base(cache, diskStore) {
            if (_log.IsDebugEnabled) {
                _log.Debug(cache.Name + " Cache: Using SpoolingLruDictionary implementation");
            }

            this.Map = new SpoolingLruDictionary(this, cache.MaxElementsInMemory);
        }

        /// <summary>
        /// An LRU Map implementation based on Apache Commons LRUMap.
        /// </summary>
        private class SpoolingLruDictionary : LruDictionary<object, Element> {
            private LruMemoryStore _store;

            /// <summary>
            /// Crée une nouvelle instance.
            /// </summary>
            /// <param name="store">Memory Store.</param>
            /// <param name="maximumSize">Taille maximum.</param>
            public SpoolingLruDictionary(LruMemoryStore store, int maximumSize)
                : base() {
                _store = store;
                this.MaximumSize = maximumSize;
            }

            /// <summary>
            /// Called after the element has been removed.
            /// Our choices are to do nothing or spool the element to disk.
            /// Note that value will be null when the memory size is set to 0. Thus a null guard is used.
            /// </summary>
            /// <param name="key">Element key.</param>
            /// <param name="value">Element.</param>
            protected override void ProcessRemovedLru(object key, Element value) {
                if (value == null) {
                    return;
                }

                if (value.IsExpired) {
                    _store.NotifyExpiry(value);
                } else {
                    _store.Evict(value);
                }
            }
        }
    }
}
