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

using System.Diagnostics.CodeAnalysis;
using Kinetix.Caching.Config;
using Kinetix.Caching.Store;

namespace Kinetix.Caching {
    /// <summary>
    /// An interface for Ehcache.
    ///
    /// Ehcache is the central interface. Caches have Elements and are managed
    /// by the CacheManager. The Cache performs logical actions. It delegates physical
    /// implementations to its Stores.
    ///
    /// A reference to an EhCache can be obtained through the CacheManager. An Ehcache thus obtained
    /// is guaranteed to have status. This status is checked for any method which
    /// throws IllegalStateException and the same thrown if it is not alive. This would normally
    /// happen if a call is made after CacheManager.shutdown is invoked.
    ///
    /// Statistics on cache usage are collected and made available through public methods.
    /// </summary>
    public interface IEhcache {
        /// <summary>
        /// Gets the cache configuration this cache was created with.
        /// </summary>
        CacheConfiguration Configuration {
            get;
        }

        /// <summary>
        /// Does the overflow go to disk.
        /// </summary>
        bool IsOverflowToDisk {
            get;
        }

        /// <summary>
        /// Gets the maximum number of elements to hold in memory.
        /// </summary>
        int MaxElementsInMemory {
            get;
        }

        /// <summary>
        /// The policy used to evict elements from the MemoryStore.
        /// The default value is LRU.
        /// </summary>
        MemoryStoreEvictionPolicy MemoryStoreEvictionPolicy {
            get;
        }

        /// <summary>
        /// Gets the cache name.
        /// </summary>
        string Name {
            get;
        }

        /// <summary>
        /// Retourne le service de notification.
        /// </summary>
        ICacheEventNotificationService NotificationService {
            get;
        }

        /// <summary>
        /// Gets an element from the cache. Updates Element Statistics
        ///
        /// Note that the Element's lastAccessTime is always the time of this get.
        /// Use GetQuiet(object) to peak into the Element to see its last access time with get.
        /// </summary>
        /// <param name="key">Nouvel élément.</param>
        /// <returns>The element.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Nommage IE EhCache.")]
        Element Get(object key);

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
        void Put(Element element);
    }
}
