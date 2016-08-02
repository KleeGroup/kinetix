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
using System.Configuration;
using Kinetix.Caching.Config;
using Kinetix.Caching.Store;

namespace Kinetix.Caching {
    /// <summary>
    /// Gestionnaire de cache.
    /// </summary>
    public sealed class CacheManager : IDisposable {

        private static CacheManager _instance;

        private readonly Dictionary<string, Cache> _cacheDictionnary;
        private readonly CacheConfigElement _defaultElement;
        private readonly CacheManagerConfigElement _configElement;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        private CacheManager() {
            _cacheDictionnary = new Dictionary<string, Cache>();
            _configElement = (CacheManagerConfigElement)ConfigurationManager.GetSection(
                    CacheManagerConfigElement.CacheElementName);

            if (_configElement == null) {
                _configElement = new CacheManagerConfigElement();
            }

            _defaultElement = _configElement.Caches[CacheConfigElement.DefaultCacheName];

            if (_defaultElement == null) {
                _defaultElement = new CacheConfigElement();
            }
        }

        /// <summary>
        /// Retourne un singleton.
        /// </summary>
        public static CacheManager Instance {
            get {
                if (_instance == null) {
                    _instance = new CacheManager();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Retourne la configuration du cache.
        /// </summary>
        public CacheManagerConfigElement Configuration {
            get {
                return _configElement;
            }
        }

        /// <summary>
        /// Retourne la configuration par défaut des caches.
        /// </summary>
        public CacheConfigElement CacheDefaultConfiguration {
            get {
                return _defaultElement;
            }
        }

        /// <summary>
        /// Retourne le cache. Le cache est créé si nécessaire.
        /// </summary>
        /// <param name="cacheName">Nom du cache.</param>
        /// <returns>Cache.</returns>
        public Cache GetCache(string cacheName) {
            lock (this) {
                Cache cache;
                if (!_cacheDictionnary.TryGetValue(cacheName, out cache)) {
                    cache = this.CreateCache(cacheName);
                    _cacheDictionnary[cacheName] = cache;
                }

                return cache;
            }
        }

        /// <summary>
        /// Libère les ressources du cache.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libère les ressources.
        /// </summary>
        /// <param name="disposing">Dispose.</param>
        private void Dispose(bool disposing) {
            if (disposing) {
                foreach (Cache cache in _cacheDictionnary.Values) {
                    cache.Dispose();
                }

                _cacheDictionnary.Clear();
                _instance = null;
            }
        }

        /// <summary>
        /// Crée un nouveau cache.
        /// </summary>
        /// <param name="cacheName">Nom du cache.</param>
        /// <returns>Cache.</returns>
        private Cache CreateCache(string cacheName) {
            CacheConfigElement element = _configElement.Caches[cacheName];
            if (element == null) {
                element = _defaultElement;
            }

            MemoryStoreEvictionPolicy policy = MemoryStoreEvictionPolicy.Lru;
            try {
                policy = (MemoryStoreEvictionPolicy)Enum.Parse(
                    typeof(MemoryStoreEvictionPolicy),
                    element.MemoryStoreEvictionPolicy);
            } catch (ArgumentException) {
                policy = MemoryStoreEvictionPolicy.Lru;
            }

            Cache cache = new Cache(
                cacheName,
                element.MaxElementsInMemory,
                policy,
                element.IsOverflowToDisk,
                _configElement.DiskStorePath,
                element.IsEternal,
                element.TimeToLiveSeconds,
                element.TimeToIdleSeconds,
                element.DiskPersistent,
                element.DiskExpiryThreadIntervalSeconds,
                element.MaxElementsOnDisk,
                element.DiskSpoolBufferSizeMB);
            cache.Init();
            return cache;
        }
    }
}
