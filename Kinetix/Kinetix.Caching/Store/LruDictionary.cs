/*
 *  Copyright 2001-2004 The Apache Software Foundation
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

namespace Kinetix.Caching.Store {
    /// <summary>
    /// An implementation of a Map which has a maximum size and uses a Least Recently Used
    /// algorithm to remove items from the Map when the maximum size is reached and new items are added.
    /// </summary>
    /// <typeparam name="TKey">Type clef.</typeparam>
    /// <typeparam name="TValue">Type valeur.</typeparam>
    internal class LruDictionary<TKey, TValue> : SequencedDictionary<TKey, TValue> {
        /// <summary>
        /// Default constructor, primarily for the purpose of
        /// de-externalization.  This constructors sets a default
        /// LRU limit of 100 keys, but this value may be overridden
        /// internally as a result of de-externalization.
        /// </summary>
        public LruDictionary()
            : this(100) {
        }

        /// <summary>
        /// Create a new LruDictionary with a maximum capacity of maximumSize.
        /// </summary>
        /// <param name="maximumSize">Maximum capacity.</param>
        public LruDictionary(int maximumSize)
            : base(maximumSize) {
            this.MaximumSize = maximumSize;
        }

        /// <summary>
        /// Obtient ou définit la taille maximum du dictionnaire.
        /// </summary>
        public int MaximumSize {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la valeur pour une clef.
        /// </summary>
        /// <param name="key">Clef.</param>
        /// <returns>Valeur.</returns>
        public override TValue this[TKey key] {
            get {
                if (!this.ContainsKey(key)) {
                    return default(TValue);
                }

                TValue value = base[key];
                Remove(key);
                Add(key, value);
                return value;
            }

            set {
                int mapSize = this.Count;
                if (mapSize >= this.MaximumSize) {
                    if (!this.ContainsKey(key)) {
                        this.RemoveLru();
                    }
                }

                base[key] = value;
            }
        }

        /// <summary>
        /// This method is used internally by the class for
        /// finding and removing the LRU Object.
        /// </summary>
        protected virtual void RemoveLru() {
            TKey key = this.FirstKey;
            TValue value = base[key];
            this.Remove(key);

            this.ProcessRemovedLru(key, value);
        }

        /// <summary>
        /// Subclasses of LRUMap may hook into this method to
        /// provide specialized actions whenever an Object is
        /// automatically removed from the cache.  By default,
        /// this method does nothing.
        /// </summary>
        /// <param name="key">Key that was removed.</param>
        /// <param name="value">Value of that key (can be null).</param>
        protected virtual void ProcessRemovedLru(TKey key, TValue value) {
        }
    }
}
