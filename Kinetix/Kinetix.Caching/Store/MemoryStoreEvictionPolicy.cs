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

namespace Kinetix.Caching.Store {

    /// <summary>
    /// The policy used to evict elements from the MemoryStore.
    /// The default value is LRU.
    /// </summary>
    public enum MemoryStoreEvictionPolicy {

        /// <summary>
        /// LRU - least recently used.
        /// </summary>
        Lru,

        /// <summary>
        /// LFU - least frequently used.
        /// </summary>
        Lfu,

        /// <summary>
        /// FIFO - first in first out, the oldest element by creation time.
        /// </summary>
        Fifo
    }
}
