using System;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using Kinetix.Caching.Store;

namespace Kinetix.Caching.Test {
    /// <summary>
    /// Classe de test du gestionnaire de cache.
    /// </summary>
    [TestFixture]
    public class CacheManagerTest {
        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void CreateInstanceTest() {
            using (CacheManager manager = CacheManager.Instance) {
                Assert.IsNotNull(manager.Configuration);
                Assert.IsNotNull(manager.CacheDefaultConfiguration);
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void GetCacheTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void GetCacheLruTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MemoryStoreEvictionPolicy = "Lru";
                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(MemoryStoreEvictionPolicy.Lru, cache.Configuration.EvictionPolicy);
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void GetCacheLfuTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MemoryStoreEvictionPolicy = "Lfu";
                manager.GetCache("Test");
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void GetCacheFifoTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MemoryStoreEvictionPolicy = "Fifo";
                manager.GetCache("Test");
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void DefaultMemoryStoreEvictionPolicyTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MemoryStoreEvictionPolicy = null;
                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(MemoryStoreEvictionPolicy.Lru, cache.Configuration.EvictionPolicy);
                }
            }
        }
    }
}
