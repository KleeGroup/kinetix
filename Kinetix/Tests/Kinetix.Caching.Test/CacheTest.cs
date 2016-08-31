using System;
using System.Threading;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif

namespace Kinetix.Caching.Test {
    /// <summary>
    /// Classe de test d'un cache.
    /// </summary>
    [TestFixture]
    public class CacheTest {

        private readonly AutoResetEvent _allRemovedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _evictedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _putEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _removedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _updatedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _disposedEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _expiryEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _backupComplete = new AutoResetEvent(false);

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void CacheMinimumThreadIntervalTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.DiskExpiryThreadIntervalSeconds = 0;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(120, cache.Configuration.DiskExpiryThreadIntervalSeconds);
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void CacheMinimumDiskBufferTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.DiskSpoolBufferSizeMB = 0;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(30, cache.Configuration.DiskSpoolBufferSizeMB);
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void CacheWarnDiskOnlyTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 0;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CacheException))]
        public void CacheShutdownPutTest() {
            using (CacheManager manager = CacheManager.Instance) {
                Cache cache = null;
                using (cache = manager.GetCache("Test")) {
                }
                cache.Put(new Element("Entry1", "Value1"));
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CacheException))]
        public void CacheShutdownPutQuietTest() {
            using (CacheManager manager = CacheManager.Instance) {
                Cache cache = null;
                using (cache = manager.GetCache("Test")) {
                }
                cache.PutQuiet(new Element("Entry1", "Value1"));
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CacheException))]
        public void CacheShutdownGetTest() {
            using (CacheManager manager = CacheManager.Instance) {
                Cache cache = null;
                using (cache = manager.GetCache("Test")) {
                }
                cache.Get("Entry1");
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CacheException))]
        public void CacheShutdownGetQuietTest() {
            using (CacheManager manager = CacheManager.Instance) {
                Cache cache = null;
                using (cache = manager.GetCache("Test")) {
                }
                cache.GetQuiet("Entry1");
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CacheException))]
        public void CacheShutdownRemoveTest() {
            using (CacheManager manager = CacheManager.Instance) {
                Cache cache = null;
                using (cache = manager.GetCache("Test")) {
                }
                cache.Remove("Entry1");
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CacheException))]
        public void CacheShutdownRemoveQuietTest() {
            using (CacheManager manager = CacheManager.Instance) {
                Cache cache = null;
                using (cache = manager.GetCache("Test")) {
                }
                cache.RemoveQuiet("Entry1");
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CacheException))]
        public void CacheShutdownRemoveAllTest() {
            using (CacheManager manager = CacheManager.Instance) {
                Cache cache = null;
                using (cache = manager.GetCache("Test")) {
                }
                cache.RemoveAll();
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CacheException))]
        public void CacheShutdownIsElementInMemoryTest() {
            using (CacheManager manager = CacheManager.Instance) {
                Cache cache = null;
                using (cache = manager.GetCache("Test")) {
                }
                cache.IsElementInMemory("Entry1");
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CacheException))]
        public void CacheShutdownIsElementOnDiskTest() {
            using (CacheManager manager = CacheManager.Instance) {
                Cache cache = null;
                using (cache = manager.GetCache("Test")) {
                }
                cache.IsElementOnDisk("Entry1");
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CacheException))]
        public void CacheShutdownIsExpiredTest() {
            using (CacheManager manager = CacheManager.Instance) {
                Cache cache = null;
                using (cache = manager.GetCache("Test")) {
                }
                cache.IsExpired(new Element("Entry1", "Value1"));
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void DefaultDiskStorePathTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.Configuration.DiskStorePath = null;
                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(".", cache.Configuration.DiskStorePath);
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void DiskStorePathTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.Configuration.DiskStorePath = "/";
                using (Cache cache = manager.GetCache("Test2")) {
                    Assert.AreEqual("/", cache.Configuration.DiskStorePath);
                }
            }
        }

        /// <summary>
        /// Test la lecture d'un élément.
        /// </summary>
        [Test]
        public void CacheGetTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreNotEqual(Guid.Empty, cache.CacheGuid);
                    Element element = new Element("Entry1", "Value1");
                    Assert.IsTrue(DateTime.Now.Ticks - element.CreationTime < 100000);
                    cache.Put(element);
                    Assert.AreEqual(0, element.LastAccessTime);
                    element = cache.Get("Entry1");
                    Assert.AreEqual(0, element.NextToLastAccessTime);
                    Assert.AreNotEqual(0, element.LastAccessTime);
                    Assert.AreEqual("Entry1", element.Key);
                    Assert.AreEqual(1, element.HitCount);
                    Assert.AreEqual("Value1", element.Value);
                    element = cache.Get("Entry1");
                    Assert.AreNotEqual(0, element.NextToLastAccessTime);
                    Assert.AreNotEqual(0, element.LastAccessTime);
                }
            }
        }

        /// <summary>
        /// Test la lecture d'un élément en mode silencieux.
        /// </summary>
        [Test]
        public void CacheGetQuietTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    cache.Put(new Element("Entry1", "Value1"));
                    Element element = cache.GetQuiet("Entry1");
                    Assert.AreEqual("Entry1", element.Key);
                    Assert.AreEqual(0, element.HitCount);
                    Assert.AreEqual("Value1", element.Value);
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void CacheUpdateEventTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    cache.ElementUpdate += new EventHandler<CacheEventArgs>(Cache_ElementUpdate);

                    Element element = new Element("Entry1", "Value1");
                    Assert.AreEqual(0, element.LastUpdateTime);
                    cache.Put(element);
                    if (_updatedEvent.WaitOne(100, false)) {
                        Assert.Fail();
                    }
                    cache.Put(element);
                    Assert.AreNotEqual(0, element.LastUpdateTime);
                    if (!_updatedEvent.WaitOne(100, false)) {
                        Assert.Fail();
                    }
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void CachePutEventTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    cache.ElementPut += new EventHandler<CacheEventArgs>(Cache_ElementPut);

                    cache.Put(new Element("Entry1", "Value1"));
                    if (!_putEvent.WaitOne(100, false)) {
                        Assert.Fail();
                    }
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void CacheRemoveTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    cache.Put(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));
                    cache.Remove("Entry1");
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsFalse(cache.IsElementOnDisk("Entry1"));
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void CacheRemoveQuietTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    cache.Put(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));
                    cache.RemoveQuiet("Entry1");
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsFalse(cache.IsElementOnDisk("Entry1"));
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void CacheRemoveAllTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    cache.AllRemoved += new EventHandler(Cache_AllRemoved);

                    cache.Put(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));
                    cache.Put(new Element("Entry2", "Value2"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));

                    cache.RemoveAll();
                    if (!_allRemovedEvent.WaitOne(100, false)) {
                        Assert.Fail();
                    }
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry2"));
                }
            }
        }

        /// <summary>
        /// Test la création d'une instance.
        /// </summary>
        [Test]
        public void CacheRemoveEventTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    cache.ElementRemoved += new EventHandler<CacheEventArgs>(Cache_ElementRemoved);

                    cache.Put(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));

                    cache.Remove("Entry1");
                    if (!_removedEvent.WaitOne(100, false)) {
                        Assert.Fail();
                    }
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsFalse(cache.IsElementOnDisk("Entry1"));
                }
            }
        }

        /// <summary>
        /// MaxElementsInMemory = 1.
        /// </summary>
        [Test]
        public void CacheMaxElementsInMemoryTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);

                    cache.Put(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));
                    cache.Put(new Element("Entry2", "Value2"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));
                }
            }
        }

        /// <summary>
        /// IsExpiredNull.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheIsExpiredNullTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                using (Cache cache = manager.GetCache("Test")) {
                    cache.IsExpired(null);
                }
            }
        }

        /// <summary>
        /// IsElementInMemoryNull.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheIsElementInMemoryNullTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                using (Cache cache = manager.GetCache("Test")) {
                    cache.IsElementInMemory(null);
                }
            }
        }

        /// <summary>
        /// IsElementOnDiskNull.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CacheIsElementOnDiskNullTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                using (Cache cache = manager.GetCache("Test")) {
                    cache.IsElementOnDisk(null);
                }
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        [Test]
        public void CacheDisposeTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    cache.Disposed += new EventHandler(Cache_Disposed);
                }
                if (!_disposedEvent.WaitOne(100, false)) {
                    Assert.Fail();
                }
            }
        }

        /// <summary>
        /// Eternal = true.
        /// </summary>
        [Test]
        public void CacheEternalTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.IsEternal = true;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.IsTrue(cache.Configuration.IsEternal);

                    Element element = new Element("Entry1", "Value1");
                    cache.Put(element);
                    Assert.IsTrue(element.Eternal);
                    Assert.IsFalse(element.IsExpired);
                }
            }
        }

        /// <summary>
        /// TimeToIdle.
        /// </summary>
        [Test]
        public void CacheTimeToIdleTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.TimeToIdleSeconds = 1;
                manager.CacheDefaultConfiguration.TimeToLiveSeconds = 0;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(0, cache.Configuration.TimeToLiveSeconds);
                    Assert.AreEqual(1, cache.Configuration.TimeToIdleSeconds);

                    Element element = new Element("Entry1", "Value1");
                    cache.Put(element);
                    Assert.AreEqual(0, element.TimeToLive);
                    Assert.AreEqual(1, element.TimeToIdle);

                    Thread.Sleep(1100);
                    Assert.IsTrue(element.IsExpired);
                }
            }
        }

        /// <summary>
        /// TimeToIdle.
        /// </summary>
        [Test]
        public void CacheGetTimeToIdleTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.TimeToIdleSeconds = 1;
                manager.CacheDefaultConfiguration.TimeToLiveSeconds = 0;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(0, cache.Configuration.TimeToLiveSeconds);
                    Assert.AreEqual(1, cache.Configuration.TimeToIdleSeconds);

                    Element element = new Element("Entry1", "Value1");
                    cache.Put(element);
                    Assert.AreEqual(0, element.TimeToLive);
                    Assert.AreEqual(1, element.TimeToIdle);

                    Thread.Sleep(900);
                    element = cache.Get("Entry1");
                    element = cache.Get("Entry1");
                    Thread.Sleep(200);
                    Assert.IsFalse(element.IsExpired);
                    Thread.Sleep(900);
                    Assert.IsTrue(element.IsExpired);
                }
            }
        }

        /// <summary>
        /// TimeToIdle.
        /// </summary>
        [Test]
        public void CacheTimeToIdleLiveTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.TimeToIdleSeconds = 1;
                manager.CacheDefaultConfiguration.TimeToLiveSeconds = 2;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(2, cache.Configuration.TimeToLiveSeconds);
                    Assert.AreEqual(1, cache.Configuration.TimeToIdleSeconds);

                    Element element = new Element("Entry1", "Value1");
                    cache.Put(element);
                    element = cache.Get("Entry1");
                    element = cache.Get("Entry1");
                    Assert.AreEqual(2, element.TimeToLive);
                    Assert.AreEqual(1, element.TimeToIdle);
                    Assert.IsFalse(element.IsExpired);
                }
            }
        }

        /// <summary>
        /// TimeToIdle.
        /// </summary>
        [Test]
        public void CacheTimeToLiveIdleTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.TimeToIdleSeconds = 2;
                manager.CacheDefaultConfiguration.TimeToLiveSeconds = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(1, cache.Configuration.TimeToLiveSeconds);
                    Assert.AreEqual(2, cache.Configuration.TimeToIdleSeconds);

                    Element element = new Element("Entry1", "Value1");
                    cache.Put(element);
                    element = cache.Get("Entry1");
                    element = cache.Get("Entry1");
                    Assert.AreEqual(1, element.TimeToLive);
                    Assert.AreEqual(2, element.TimeToIdle);
                    Assert.IsFalse(element.IsExpired);
                }
            }
        }

        /// <summary>
        /// MaxElementsOnDisk = 1.
        /// </summary>
        [Test]
        public void CacheMaxElementsOnDiskTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = false;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    cache.ElementEvicted += new EventHandler<CacheEventArgs>(Cache_ElementEvicted);

                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsFalse(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(1, cache.Configuration.MaxElementsOnDisk);

                    cache.Put(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));

                    cache.Put(new Element("Entry2", "Value2"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsTrue(cache.IsElementOnDisk("Entry1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));

                    cache.Put(new Element("Entry3", "Value3"));
                    cache.Put(new Element("Entry4", "Value4"));

                    if (!_evictedEvent.WaitOne(1000, false)) {
                        Assert.Fail();
                    }
                }
            }
        }

        /// <summary>
        /// MaxElementsOnDisk = 1.
        /// </summary>
        [Test]
        public void CacheNotSerializableToDiskTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = false;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsFalse(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(1, cache.Configuration.MaxElementsOnDisk);

                    cache.Put(new Element("Entry1", new CacheEntry()));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));

                    cache.Put(new Element("Entry2", "Value2"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsFalse(cache.IsElementOnDisk("Entry1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));
                }
            }
        }

        /// <summary>
        /// MaxElementsOnDisk = 1.
        /// </summary>
        [Test]
        public void CacheNotSerializableFlushTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = true;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsTrue(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(1, cache.Configuration.MaxElementsOnDisk);

                    cache.Put(new Element("Entry1", new CacheEntry()));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));
                }
            }

            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = true;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsTrue(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(1, cache.Configuration.MaxElementsOnDisk);
                }
            }
        }

        /// <summary>
        /// IsExpired.
        /// </summary>
        [Test]
        public void CacheGetExpiredOnDiskTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.TimeToLiveSeconds = 1;
                manager.CacheDefaultConfiguration.TimeToIdleSeconds = 0;
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = false;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    cache.ElementExpiry += new EventHandler<CacheEventArgs>(Cache_ElementExpiry);

                    Element element = new Element("Entry1", "Value1");
                    cache.Put(element);
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));
                    cache.Put(new Element("Entry2", "Value2"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsTrue(cache.IsElementOnDisk("Entry1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));

                    Thread.Sleep(900);
                    Assert.IsFalse(cache.IsExpired(element));
                    Thread.Sleep(200);
                    Assert.IsTrue(cache.IsExpired(element));
                    Assert.IsNull(cache.Get("Entry1"));
                    if (!_expiryEvent.WaitOne(100, false)) {
                        Assert.Fail();
                    }
                }
            }
        }

        /// <summary>
        /// ApplyDefaults.
        /// </summary>
        [Test]
        public void CacheApplyDefaultsTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.TimeToLiveSeconds = 1;
                manager.CacheDefaultConfiguration.TimeToIdleSeconds = 10;
                manager.CacheDefaultConfiguration.IsEternal = true;

                using (Cache cache = manager.GetCache("Test")) {
                    Element element = new Element("Entry1", "Value1");
                    Assert.AreEqual(0, element.TimeToLive);
                    Assert.AreEqual(0, element.TimeToIdle);
                    Assert.IsFalse(element.IsExpired);
                    cache.Put(element);
                    Assert.AreEqual(1, element.TimeToLive);
                    Assert.AreEqual(10, element.TimeToIdle);
                    Assert.IsTrue(element.Eternal);
                }
            }
        }

        /// <summary>
        /// IsExpired.
        /// </summary>
        [Test]
        public void CacheIsExpiredTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.TimeToLiveSeconds = 1;
                manager.CacheDefaultConfiguration.TimeToIdleSeconds = 0;

                using (Cache cache = manager.GetCache("Test")) {
                    Element element = new Element("Entry1", "Value1");
                    cache.Put(element);
                    Thread.Sleep(900);
                    Assert.IsFalse(cache.IsExpired(element));
                    Thread.Sleep(200);
                    Assert.IsTrue(cache.IsExpired(element));
                }
            }
        }

        /// <summary>
        /// IsExpired.
        /// </summary>
        [Test]
        public void CacheGetExpiredTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.TimeToLiveSeconds = 1;
                manager.CacheDefaultConfiguration.TimeToIdleSeconds = 0;

                using (Cache cache = manager.GetCache("Test")) {
                    cache.ElementExpiry += new EventHandler<CacheEventArgs>(Cache_ElementExpiry);

                    Element element = new Element("Entry1", "Value1");
                    cache.Put(element);
                    Thread.Sleep(900);
                    Assert.IsFalse(cache.IsExpired(element));
                    Thread.Sleep(200);
                    Assert.IsTrue(cache.IsExpired(element));
                    Assert.IsNull(cache.Get("Entry1"));
                    if (!_expiryEvent.WaitOne(100, false)) {
                        Assert.Fail();
                    }
                }
            }
        }

        /// <summary>
        /// Put null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CachePutNullTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    cache.Put(null);
                }
            }
        }

        /// <summary>
        /// PutQuiet null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CachePutQuietNullTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    cache.PutQuiet(null);
                }
            }
        }

        /// <summary>
        /// PutQuiet.
        /// </summary>
        [Test]
        public void CachePutQuietTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = false;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {

                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsFalse(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(1, cache.Configuration.MaxElementsOnDisk);

                    cache.PutQuiet(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));

                    cache.PutQuiet(new Element("Entry2", "Value2"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsTrue(cache.IsElementOnDisk("Entry1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));
                }
            }
        }

        /// <summary>
        /// GetDiskElement.
        /// </summary>
        [Test]
        public void CacheGetOnDiskTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = false;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {

                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsFalse(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(1, cache.Configuration.MaxElementsOnDisk);

                    cache.Put(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));

                    cache.Put(new Element("Entry2", "Value2"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsTrue(cache.IsElementOnDisk("Entry1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));

                    Assert.AreEqual("Value1", cache.Get("Entry1").Value);
                }
            }
        }

        /// <summary>
        /// GetDiskElement.
        /// </summary>
        [Test]
        public void CacheGetOnPersistentDiskTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.DiskPersistent = true;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsTrue(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(1, cache.Configuration.MaxElementsOnDisk);

                    cache.Put(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));

                    cache.Put(new Element("Entry2", "Value2"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsTrue(cache.IsElementOnDisk("Entry1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));

                    Assert.AreEqual("Value1", cache.Get("Entry1").Value);
                }
            }

            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.DiskPersistent = true;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsTrue(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(1, cache.Configuration.MaxElementsOnDisk);

                    Assert.IsTrue(cache.IsElementOnDisk("Entry1"));
                    Assert.IsTrue(cache.IsElementOnDisk("Entry2"));

                    Assert.AreEqual("Value1", cache.Get("Entry1").Value);
                    Assert.AreEqual("Value2", cache.Get("Entry2").Value);
                }
            }
        }

        /// <summary>
        /// RemoveDiskElement.
        /// </summary>
        [Test]
        public void CacheRemoveOnDiskTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = false;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsFalse(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(1, cache.Configuration.MaxElementsOnDisk);

                    cache.Put(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));

                    cache.Put(new Element("Entry2", "Value2"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsTrue(cache.IsElementOnDisk("Entry1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));

                    cache.Remove("Entry1");
                    Assert.IsFalse(cache.IsElementOnDisk("Entry1"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                }
            }
        }

        /// <summary>
        /// RemoveDiskElement.
        /// </summary>
        [Test]
        public void CacheRemoveAllOnDiskTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = false;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsFalse(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(1, cache.Configuration.MaxElementsOnDisk);

                    cache.Put(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));

                    cache.Put(new Element("Entry2", "Value2"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsTrue(cache.IsElementOnDisk("Entry1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));

                    cache.RemoveAll();
                    Assert.IsFalse(cache.IsElementOnDisk("Entry1"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsFalse(cache.IsElementOnDisk("Entry2"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry2"));
                }
            }
        }

        /// <summary>
        /// GetQuietDiskElement.
        /// </summary>
        [Test]
        public void CacheGetQuietOnDiskTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = false;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsFalse(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(1, cache.Configuration.MaxElementsOnDisk);

                    cache.Put(new Element("Entry1", "Value1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry1"));

                    cache.Put(new Element("Entry2", "Value2"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsTrue(cache.IsElementOnDisk("Entry1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));

                    Assert.AreEqual("Value1", cache.GetQuiet("Entry1").Value);
                }
            }
        }

        /// <summary>
        /// GetMissing.
        /// </summary>
        [Test]
        public void CacheGetMissingTest() {
            using (CacheManager manager = CacheManager.Instance) {
                using (Cache cache = manager.GetCache("Test")) {
                    Assert.IsNull(cache.Get("NotFound"));
                }
            }
        }

        /// <summary>
        /// GetDiskElement.
        /// </summary>
        [Test]
        public void CacheExpiredOnDiskTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = false;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 100;
                manager.CacheDefaultConfiguration.TimeToIdleSeconds = 0;
                manager.CacheDefaultConfiguration.TimeToLiveSeconds = 1;
                manager.CacheDefaultConfiguration.DiskExpiryThreadIntervalSeconds = 2;

                using (Cache cache = manager.GetCache("Test")) {
                    cache.ElementExpiry += new EventHandler<CacheEventArgs>(Cache_ElementExpiry);

                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsFalse(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(100, cache.Configuration.MaxElementsOnDisk);
                    Assert.AreEqual(0, cache.Configuration.TimeToIdleSeconds);
                    Assert.AreEqual(1, cache.Configuration.TimeToLiveSeconds);

                    cache.Put(new Element("Entry1", "Value1"));
                    cache.Put(new Element("Entry2", "Value2"));

                    if (!_expiryEvent.WaitOne(3000, false)) {
                        Assert.Fail();
                    }

                    Assert.IsFalse(cache.IsElementOnDisk("Entry1"));
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                }
            }
        }

        /// <summary>
        /// GetDiskElement.
        /// </summary>
        [Test]
        public void CacheExpiredInMemoryTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.TimeToIdleSeconds = 0;
                manager.CacheDefaultConfiguration.TimeToLiveSeconds = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    cache.ElementExpiry += new EventHandler<CacheEventArgs>(Cache_ElementExpiry);

                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.AreEqual(0, cache.Configuration.TimeToIdleSeconds);
                    Assert.AreEqual(1, cache.Configuration.TimeToLiveSeconds);

                    cache.Put(new Element("Entry1", "Value1"));

                    Thread.Sleep(1100);
                    _expiryEvent.Reset();
                    cache.Put(new Element("Entry2", "Value2"));
                    if (!_expiryEvent.WaitOne(100, false)) {
                        Assert.Fail();
                    }
                    Assert.IsFalse(cache.IsElementInMemory("Entry1"));
                    Assert.IsTrue(cache.IsElementInMemory("Entry2"));
                }
            }
        }

        /// <summary>
        /// GetDiskElement.
        /// </summary>
        [Test]
        public void CacheBackupTest() {
            using (CacheManager manager = CacheManager.Instance) {
                manager.CacheDefaultConfiguration.MaxElementsInMemory = 1;
                manager.CacheDefaultConfiguration.IsOverflowToDisk = true;
                manager.CacheDefaultConfiguration.DiskPersistent = false;
                manager.CacheDefaultConfiguration.MaxElementsOnDisk = 100;
                manager.CacheDefaultConfiguration.DiskExpiryThreadIntervalSeconds = 1;
                manager.CacheDefaultConfiguration.DiskSpoolBufferSizeMB = 1;

                using (Cache cache = manager.GetCache("Test")) {
                    cache.BackupComplete += new EventHandler(Cache_BackupComplete);

                    Assert.AreEqual(1, cache.Configuration.MaxElementsInMemory);
                    Assert.IsTrue(cache.Configuration.IsOverflowToDisk);
                    Assert.IsFalse(cache.Configuration.DiskPersistent);
                    Assert.AreEqual(100, cache.Configuration.MaxElementsOnDisk);

                    cache.Put(new Element("Entry1", new byte[1024 * 1024]));
                    cache.Put(new Element("Entry2", new byte[1024 * 1024]));

                    _backupComplete.Reset();
                    if (!_backupComplete.WaitOne(1500, false)) {
                        Assert.Fail();
                    }

                    cache.Put(new Element("Entry3", new byte[1024 * 1024]));
                    cache.Put(new Element("Entry4", new byte[1024 * 1024]));

                    _backupComplete.Reset();
                    if (!_backupComplete.WaitOne(1500, false)) {
                        Assert.Fail();
                    }
                }
            }
        }

        /// <summary>
        /// Traite la suppression de tous les éléments du cache.
        /// </summary>
        /// <param name="sender">Source.</param>
        /// <param name="e">Argument.</param>
        private void Cache_AllRemoved(object sender, EventArgs e) {
            _allRemovedEvent.Set();
        }

        /// <summary>
        /// Traite l'éviction d'un élément du cache.
        /// </summary>
        /// <param name="sender">Source.</param>
        /// <param name="e">Argument.</param>
        private void Cache_ElementEvicted(object sender, CacheEventArgs e) {
            Assert.IsNotNull(e.SourceElement);
            Assert.IsFalse(e.RemoteEvent);
            _evictedEvent.Set();
        }

        /// <summary>
        /// Traite l'ajout d'un élément du cache.
        /// </summary>
        /// <param name="sender">Source.</param>
        /// <param name="e">Argument.</param>
        private void Cache_ElementPut(object sender, CacheEventArgs e) {
            Assert.IsNotNull(e.SourceElement);
            Assert.IsFalse(e.RemoteEvent);
            _putEvent.Set();
        }

        /// <summary>
        /// Traite la mise à jour d'un élément du cache.
        /// </summary>
        /// <param name="sender">Source.</param>
        /// <param name="e">Argument.</param>
        private void Cache_ElementUpdate(object sender, CacheEventArgs e) {
            Assert.IsNotNull(e.SourceElement);
            Assert.IsFalse(e.RemoteEvent);
            _updatedEvent.Set();
        }

        /// <summary>
        /// Traite la suppression d'un élément du cache.
        /// </summary>
        /// <param name="sender">Source.</param>
        /// <param name="e">Argument.</param>
        private void Cache_ElementRemoved(object sender, CacheEventArgs e) {
            Assert.IsNotNull(e.SourceElement);
            Assert.IsFalse(e.RemoteEvent);
            _removedEvent.Set();
        }

        /// <summary>
        /// Traite la suppression d'un élément expiré du cache.
        /// </summary>
        /// <param name="sender">Source.</param>
        /// <param name="e">Argument.</param>
        private void Cache_ElementExpiry(object sender, CacheEventArgs e) {
            Assert.IsNotNull(e.SourceElement);
            Assert.IsFalse(e.RemoteEvent);
            _expiryEvent.Set();
        }

        /// <summary>
        /// Traite la fin de l'écriture disque.
        /// </summary>
        /// <param name="sender">Source.</param>
        /// <param name="e">Argument.</param>
        private void Cache_BackupComplete(object sender, EventArgs e) {
            _backupComplete.Set();
        }

        /// <summary>
        /// Traite la suppression d'un élément du cache.
        /// </summary>
        /// <param name="sender">Source.</param>
        /// <param name="e">Argument.</param>
        private void Cache_Disposed(object sender, EventArgs e) {
            _disposedEvent.Set();
        }
    }
}
