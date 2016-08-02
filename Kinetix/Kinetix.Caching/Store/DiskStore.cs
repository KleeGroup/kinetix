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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using log4net;

namespace Kinetix.Caching.Store {
    /// <summary>
    /// A disk store implementation.
    ///
    /// As of ehcache-1.2 (v1.41 of this file) DiskStore has been changed to a mix of finer grained locking using synchronized collections
    /// and synchronizing on the whole instance, as was the case with earlier versions.
    ///
    /// The DiskStore, as of ehcache-1.2.4, supports eviction using an LFU policy, if a maximum disk
    /// store size is set. LFU uses statistics held at the Element level which survive moving between
    /// maps in the MemoryStore and DiskStores.
    /// </summary>
    internal sealed class DiskStore : IStore, IDisposable {

        private const int TicksPerSecond = 10000000;
        private const int SpoolThreadInterval = 200;
        private const int EstimatedMinimumPayloadSize = 512;
        private const int OneMegabyte = 1048576;

        private static readonly ILog _log = LogManager.GetLogger(typeof(DiskStore).Name);

        /// <summary>
        /// If persistent, the disk file will be kept
        /// and reused on next startup. In addition the
        /// memory store will flush all contents to spool,
        /// and spool will flush all to disk.
        /// </summary>
        private readonly bool _persistent;
        private readonly string _diskPath;
        private readonly string _name;

        /// <summary>
        /// The maximum elements to allow in the disk file.
        /// </summary>
        private readonly long _maxElementsOnDisk;

        private long _expiryThreadInterval;
        private bool _active;
        private FileStream _randomAccessFile;
        private Dictionary<object, DiskElement> _diskElements = new Dictionary<object, DiskElement>();
        private List<DiskElement> _freeSpace = new List<DiskElement>();
        private Dictionary<object, Element> _spool = new Dictionary<object, Element>();
        private object _spoolLock = new object();
        private ManualResetEvent _spoolAndExpiryResetEvent;
        private Thread _spoolAndExpiryThread;
        private IEhcache _cache;
        private FileInfo _dataFile;

        /// <summary>
        /// Used to persist elements.
        /// </summary>
        private FileInfo _indexFile;

        /// <summary>
        /// The size in bytes of the disk elements.
        /// </summary>
        private long _totalSize;

        /// <summary>
        /// Whether the cache is eternal.
        /// </summary>
        private bool _eternal;
        private int _lastElementSize;
        private int _diskSpoolBufferSizeBytes;

        /// <summary>
        /// Creates a disk store.
        /// </summary>
        /// <param name="cache">The Cache that the store is part of.</param>
        /// <param name="diskPath">The directory in which to create data and index files.</param>
        public DiskStore(IEhcache cache, string diskPath) {
            this._cache = cache;
            this._diskPath = diskPath;
            this._name = cache.Name;

            _expiryThreadInterval = _cache.Configuration.DiskExpiryThreadIntervalSeconds;
            _persistent = _cache.Configuration.DiskPersistent;
            _maxElementsOnDisk = _cache.Configuration.MaxElementsOnDisk;
            _eternal = _cache.Configuration.IsEternal;
            _diskSpoolBufferSizeBytes = _cache.Configuration.DiskSpoolBufferSizeMB * OneMegabyte;

            try {
                this.InitialiseFiles();
                _active = true;

                // Always start up the spool thread
                _spoolAndExpiryResetEvent = new ManualResetEvent(false);
                ThreadStart threadStart = new ThreadStart(this.SpoolAndExpiryThreadMain);
                _spoolAndExpiryThread = new Thread(threadStart);
                _spoolAndExpiryThread.Name = "Store " + _name + " Spool Thread";
                _spoolAndExpiryThread.Priority = ThreadPriority.Normal;
                _spoolAndExpiryThread.IsBackground = true;
                _spoolAndExpiryThread.Start();
            } catch (IOException e) {
                this.Dispose();
                _log.Error(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SR.ErrorCouldNotCreateStore,
                        _name,
                        e.Message),
                    e);
            }
        }

        /// <summary>
        /// The file name of the data file where the disk store stores data, without any path information.
        /// </summary>
        public string DataFileName {
            get {
                return _name + ".data";
            }
        }

        /// <summary>
        /// The file name of the index file, which maintains a record of elements and their addresses
        /// on the data file, without any path information.
        /// </summary>
        public string IndexFileName {
            get {
                return _name + ".index";
            }
        }

        /// <summary>
        /// Estimation de la charge d'un élément.
        /// </summary>
        private int EstimatedPayloadSize {
            get {
                int size = 0;
                if (_diskElements.Count > 0) {
                    size = (int)(_totalSize / _diskElements.Count);
                }

                if (size <= 0) {
                    size = EstimatedMinimumPayloadSize;
                }

                return size;
            }
        }

        /// <summary>
        /// An unsynchronized and very low cost check to see if a key is in the Store. No check is made to see if the Element is expired.
        /// </summary>
        /// <param name="key">The Element key.</param>
        /// <returns>
        /// True if found. If this method return false, it means that an Element with the given key is definitely not in the MemoryStore.
        /// If it returns true, there is an Element there. An attempt to get it may return null if the Element has expired.
        /// </returns>
        public bool ContainsKey(object key) {
            return _diskElements.ContainsKey(key) || _spool.ContainsKey(key);
        }

        /// <summary>
        /// Shuts down the disk store in preparation for cache shutdown.
        ///
        /// If a VM crash happens, the shutdown hook will not run. The data file and the index file
        /// will be out of synchronisation. At initialisation we always delete the index file
        /// after we have read the elements, so that it has a zero length. On a dirty restart, it still will have
        /// and the data file will automatically be deleted, thus preserving safety.
        /// </summary>
        public void Dispose() {
            // Close the cache
            _active = false;
            try {
                this.Flush();

                _spoolAndExpiryResetEvent.Set();
                _spoolAndExpiryThread.Join();

                ((IDisposable)_spoolAndExpiryResetEvent).Dispose();

                _spool.Clear();
                _diskElements.Clear();
                _freeSpace.Clear();
                if (_randomAccessFile != null) {
                    _randomAccessFile.Close();
                    _randomAccessFile.Dispose();
                }

                if (!_persistent) {
                    _log.Debug("Deleting file " + _dataFile.Name);
                    _dataFile.Delete();
                }
            } catch (IOException e) {
                _log.Error(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SR.ErrorCouldNotShutdownStore,
                        _name,
                        e.Message),
                    e);
            }

            _randomAccessFile = null;
            _cache = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Flush the spool if persistent, so we don't lose any data.
        /// </summary>
        public void Flush() {
            if (_persistent) {
                this.FlushSpool();
                this.WriteIndex();
            }
        }

        /// <summary>
        /// Gets an Element from the Disk Store.
        /// </summary>
        /// <param name="key">Element's Key.</param>
        /// <returns>The element.</returns>
        public Element Get(object key) {
            lock (this) {
                try {
                    this.CheckActive();

                    Element element = null;

                    // Check in the spool.  Remove if present
                    lock (_spoolLock) {
                        if (_spool.TryGetValue(key, out element)) {
                            _spool.Remove(key);
                        }
                    }

                    if (element != null) {
                        element.UpdateAccessStatistics();
                        return element;
                    }

                    // Check if the element is on disk
                    DiskElement diskElement;
                    if (!_diskElements.TryGetValue(key, out diskElement)) {
                        return null;
                    }

                    element = this.LoadElementFromDiskElement(diskElement);
                    element.UpdateAccessStatistics();
                    return element;
                } catch (IOException e) {
                    _log.Error(
                        _name + "Cache: Could not read disk store element for key " + key
                            + ". Error was " + e.Message, e);
                }

                return null;
            }
        }

        /// <summary>
        /// Gets an Element from the Disk Store, without updating statistics.
        /// </summary>
        /// <param name="key">Element's Key.</param>
        /// <returns>The element.</returns>
        public Element GetQuiet(object key) {
            lock (this) {
                try {
                    this.CheckActive();

                    Element element = null;

                    // Check in the spool.  Remove if present
                    lock (_spoolLock) {
                        if (_spool.TryGetValue(key, out element)) {
                            _spool.Remove(key);
                        }
                    }

                    if (element != null) {
                        return element;
                    }

                    // Check if the element is on disk
                    DiskElement diskElement;
                    if (!_diskElements.TryGetValue(key, out diskElement)) {
                        return null;
                    }

                    element = this.LoadElementFromDiskElement(diskElement);
                    return element;
                } catch (IOException e) {
                    _log.Error(
                        _name + "Cache: Could not read disk store element for key " + key
                            + ". Error was " + e.Message, e);
                }

                return null;
            }
        }

        /// <summary>
        /// Puts an element into the disk store.
        ///
        /// This method is not synchronized. It is however threadsafe. It uses fine-grained
        /// synchronization on the spool.
        /// </summary>
        /// <param name="element">Element to put.</param>
        public void Put(Element element) {
            try {
                this.CheckActive();
                if (_spoolAndExpiryThread.IsAlive) {
                    lock (_spoolLock) {
                        _spool.Add(element.Key, element);
                    }
                } else {
                    _log.Error(_name + "Cache: Elements cannot be written to disk store because the" +
                            " spool thread has died.");
                    lock (_spoolLock) {
                        _spool.Clear();
                    }
                }
            } catch (IOException e) {
                _log.Error(
                    _name + "Cache: Could not write disk store element for " + element.Key
                        + ". Initial cause was " + e.Message, e);
            }
        }

        /// <summary>
        /// Removes an item from the disk store.
        /// </summary>
        /// <param name="key">Element's Key.</param>
        /// <returns>Element.</returns>
        public Element Remove(object key) {
            lock (this) {
                Element element = null;
                try {
                    this.CheckActive();
                    lock (_spoolLock) {
                        if (_spool.TryGetValue(key, out element)) {
                            _spool.Remove(key);
                        }
                    }

                    lock (_diskElements) {
                        DiskElement diskElement;
                        if (_diskElements.TryGetValue(key, out diskElement)) {
                            _diskElements.Remove(key);
                            element = this.LoadElementFromDiskElement(diskElement);
                            this.FreeBlock(diskElement);
                        }
                    }
                } catch (Exception e) {
                    string message = _name + "Cache: Could not remove disk store entry for " + key
                            + ". Error was " + e.Message;
                    _log.Error(message, e);
                    throw new CacheException(message, e);
                }

                return element;
            }
        }

        /// <summary>
        /// Remove all of the elements from the store.
        ///
        /// If there are registered CacheEventListeners they are notified of the expiry or removal
        /// of the Element as each is removed.
        /// </summary>
        public void RemoveAll() {
            lock (this) {
                try {
                    this.CheckActive();

                    // Ditch all the elements, and truncate the file
                    _spool = new Dictionary<object, Element>();
                    _diskElements = new Dictionary<object, DiskElement>();
                    _freeSpace = new List<DiskElement>();
                    _totalSize = 0;
                    _randomAccessFile.Close();
                    _randomAccessFile = new FileStream(_dataFile.FullName, FileMode.Truncate, FileAccess.ReadWrite);
                    if (_persistent) {
                        _indexFile.Delete();
                        FileStream fs = _indexFile.Create();
                        fs.Close();
                    }
                } catch (IOException e) {
                    _log.Error(_name + " Cache: Could not rebuild disk store. Initial cause was " + e.Message, e);
                    this.Dispose();
                }
            }
        }

        /// <summary>
        /// In some circumstances data can be written so quickly to the spool that the VM runs out of memory
        /// while waiting for the spooling to disk.
        ///
        /// This is a very simple and quick test which estimates the spool size based on the last element's written size.
        /// </summary>
        /// <returns>True if backup need.</returns>
        internal bool BackedUp() {
            long estimatedSpoolSize = _spool.Count * _lastElementSize;
            bool backedUp = estimatedSpoolSize > _diskSpoolBufferSizeBytes;
            if (backedUp && _log.IsDebugEnabled) {
                _log.Debug("A back up on cache puts occurred. Consider increasing diskSpoolBufferSizeMB for cache " + _name);
            }

            return backedUp;
        }

        /// <summary>
        /// Uses random numbers to sample the entire map.
        /// </summary>
        /// <param name="diskElements">Elements.</param>
        /// <returns>An array of sampled elements.</returns>
        private static IMetaData[] SampleElements(Dictionary<object, DiskElement> diskElements) {
            int[] offsets = LfuPolicy.GenerateRandomSample(diskElements.Count);
            DiskElement[] elements = new DiskElement[offsets.Length];
            Dictionary<object, DiskElement>.Enumerator enumerator = diskElements.GetEnumerator();
            for (int i = 0; i < offsets.Length; i++) {
                for (int j = 0; j <= offsets[i]; j++) {
                    enumerator.MoveNext();
                }

                elements[i] = enumerator.Current.Value;
            }

            return elements;
        }

        /// <summary>
        /// Asserts that the store is active.
        /// </summary>
        private void CheckActive() {
            if (!_active) {
                throw new CacheException(_name + " Cache: The Disk store is not active.");
            }
        }

        /// <summary>
        /// Recherche ou crée un block de données.
        /// </summary>
        /// <param name="bufferLength">Taille du block.</param>
        /// <returns>Block.</returns>
        private DiskElement CheckForFreeBlock(int bufferLength) {
            DiskElement diskElement = this.FindFreeBlock(bufferLength);
            if (diskElement == null) {
                diskElement = new DiskElement();
                diskElement.Position = _randomAccessFile.Length;
                diskElement.BlockSize = bufferLength;
            }

            return diskElement;
        }

        /// <summary>
        /// Crée un nouveau fichier index.
        /// </summary>
        private void CreateNewIndexFile() {
            if (_indexFile.Exists) {
                _indexFile.Delete();
                if (_log.IsDebugEnabled) {
                    _log.Debug("Index file " + _indexFile.Name + " deleted.");
                }
            } else {
                FileStream fs = null;
                try {
                    fs = _indexFile.Create();
                    if (_log.IsDebugEnabled) {
                        _log.Debug("Index file " + _indexFile.Name + " created successfully");
                    }
                } catch (IOException e) {
                    throw new IOException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            SR.ExceptionCouldNotCreateIndex,
                            _indexFile.Name),
                        e);
                } finally {
                    if (fs != null) {
                        fs.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Supprime le fichier index si le fichier Data n'existe pas.
        /// </summary>
        private void DeleteIndexIfNoData() {
            if (!_dataFile.Exists && _indexFile.Exists) {
                if (_log.IsDebugEnabled) {
                    _log.Debug("Matching data file missing for index file. Deleting index file " + this.IndexFileName);
                }

                _indexFile.Delete();
            }
        }

        /// <summary>
        /// Evict an element.
        /// </summary>
        private void EvictLfuDiskElement() {
            lock (_diskElements) {
                DiskElement diskElement = this.FindRelativelyUnused();
                if (diskElement != null) {
                    _diskElements.Remove(diskElement.Key);
                    this.NotifyEvictionListeners(diskElement);
                    this.FreeBlock(diskElement);
                }
            }
        }

        /// <summary>
        /// Expire elements.
        /// </summary>
        /// <param name="nextExpiryTime">ExpiryTime.</param>
        /// <returns>The expiry time.</returns>
        private long ExceptionSafeExpireElementsIfRequired(long nextExpiryTime) {
            long updatedNextExpiryTime = nextExpiryTime;
            if (!_eternal && DateTime.Now.Ticks > nextExpiryTime) {
                updatedNextExpiryTime += _expiryThreadInterval * TicksPerSecond;
                try {
                    this.ExpireElements();
                } catch (CacheException e) {
                    _log.Error(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            SR.ErrorCouldNotExpireElements,
                            _name,
                            e.Message),
                        e);
                }
            }

            return updatedNextExpiryTime;
        }

        /// <summary>
        /// Write elements to disk.
        /// </summary>
        private void ExceptionSafeFlushIfRequired() {
            if (_spool != null && _spool.Count != 0) {
                try {
                    this.FlushSpool();
                } catch (CacheException e) {
                    _log.Error(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            SR.ErrorCouldNotFlushElements,
                            _name,
                            e.Message),
                        e);
                }
            }
        }

        /// <summary>
        /// Removes expired elements.
        ///
        /// Note that the DiskStore cannot efficiently expire based on TTI. It does it on TTL. However any gets out
        /// of the DiskStore are check for both before return.
        /// </summary>
        /// <exception cref="CacheException">La méthode garantie que c'est le seul type levé.</exception>
        private void ExpireElements() {
            long now = DateTime.Now.Ticks;
            try {
                lock (_spoolLock) {
                    List<Element> removedElement = new List<Element>();
                    foreach (Element element in _spool.Values) {
                        if (element.IsExpired) {
                            // An expired element
                            removedElement.Add(element);
                        }
                    }

                    foreach (Element element in removedElement) {
                        if (_log.IsDebugEnabled) {
                            _log.Debug(_name + "Cache: Removing expired spool element " + element.Key);
                        }

                        _spool.Remove(element);
                        this.NotifyExpiryListeners(element);
                    }
                }

                lock (_diskElements) {
                    List<DiskElement> removedDiskElement = new List<DiskElement>();
                    foreach (DiskElement diskElement in _diskElements.Values) {
                        if (now > diskElement.ExpiryTime) {
                            // An expired element
                            removedDiskElement.Add(diskElement);
                        }
                    }

                    foreach (DiskElement diskElement in removedDiskElement) {
                        if (_log.IsDebugEnabled) {
                            _log.Debug(_name + "Cache: Removing expired spool element " + diskElement.Key + " from Disk Store");
                        }

                        _diskElements.Remove(diskElement.Key);
                        if (_cache.NotificationService.HasElementExpiredListeners()) {
                            Element element = this.LoadElementFromDiskElement(diskElement);
                            this.NotifyExpiryListeners(element);
                        }

                        this.FreeBlock(diskElement);
                    }
                }
            } catch (Exception e) {
                throw new CacheException(e.Message, e);
            }
        }

        /// <summary>
        /// Allocates a free block.
        /// </summary>
        /// <param name="length">Taille nécessaire.</param>
        /// <returns>DiskElement.</returns>
        private DiskElement FindFreeBlock(int length) {
            lock (_freeSpace) {
                for (int i = 0; i < _freeSpace.Count; i++) {
                    DiskElement element = _freeSpace[i];
                    if (element.BlockSize >= length) {
                        _freeSpace.RemoveAt(i);
                        return element;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Find a "relatively" unused disk element, but not the element just added.
        /// </summary>
        /// <returns>DiskElement.</returns>
        private DiskElement FindRelativelyUnused() {
            IMetaData[] elements = DiskStore.SampleElements(_diskElements);
            IMetaData element = LfuPolicy.LeastHit(elements, null);
            return (DiskElement)element;
        }

        /// <summary>
        /// Flushes all spooled elements to disk.
        /// Note that the cache is locked for the entire time that the spool is being flushed.
        /// </summary>
        /// <exception cref="CacheException">La méthode garantie que c'est le seul type levé.</exception>
        private void FlushSpool() {
            lock (this) {
                if (_spool.Count == 0) {
                    return;
                }

                Dictionary<object, Element> copyOfSpool = null;
                try {
                    copyOfSpool = this.SwapSpoolReference();
                    List<object> removedKeyList = new List<object>();
                    foreach (Element element in copyOfSpool.Values) {
                        WriteOrReplaceEntry(element);
                        removedKeyList.Add(element.Key);
                    }

                    foreach (object key in removedKeyList) {
                        copyOfSpool.Remove(key);
                    }
                } catch (Exception e) {
                    throw new CacheException(e.Message, e);
                }

                if (copyOfSpool.Count > 0) {
                    throw new CacheException("Critical Error");
                }
            }
        }

        /// <summary>
        /// Marks a block as free.
        /// </summary>
        /// <param name="diskElement">The DiskElement to move to the free space list.</param>
        private void FreeBlock(DiskElement diskElement) {
            _totalSize -= diskElement.PayloadSize;
            diskElement.PayloadSize = 0;
            diskElement.Key = null;
            diskElement.HitCount = 0;
            diskElement.ExpiryTime = 0;
            lock (_freeSpace) {
                _freeSpace.Add(diskElement);
            }
        }

        /// <summary>
        /// Initialise les fichiers de données et d'index.
        /// </summary>
        private void InitialiseFiles() {
            if (File.Exists(_diskPath)) {
                throw new CacheException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SR.ExceptionStoreDirectoryIsFile,
                        _diskPath));
            }

            if (!Directory.Exists(_diskPath)) {
                try {
                    Directory.CreateDirectory(_diskPath);
                } catch (IOException e) {
                    throw new CacheException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            SR.ExceptionCouldNotCreateDirectory,
                            _diskPath),
                        e);
                }
            }

            _dataFile = new FileInfo(_diskPath + "\\" + this.DataFileName);
            _indexFile = new FileInfo(_diskPath + "\\" + this.IndexFileName);

            this.DeleteIndexIfNoData();

            if (_persistent) {
                if (!ReadIndex()) {
                    if (_log.IsDebugEnabled) {
                        _log.Debug("Index file dirty or empty. Deleting data file " + this.DataFileName);
                    }

                    _dataFile.Delete();
                }
            } else {
                if (_log.IsDebugEnabled) {
                    _log.Debug("Deleting data file " + this.DataFileName);
                }

                _dataFile.Delete();
                _indexFile = null;
            }

            // Open the data file as random access. The dataFile is created if necessary.
            _randomAccessFile = new FileStream(_dataFile.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        /// <summary>
        /// Charge un élément depuis le disque.
        /// </summary>
        /// <param name="diskElement">Disk Element to load.</param>
        /// <returns>Element.</returns>
        private Element LoadElementFromDiskElement(DiskElement diskElement) {
            // Load the element
            _randomAccessFile.Seek(diskElement.Position, SeekOrigin.Begin);
            byte[] buffer = new byte[diskElement.PayloadSize];
            _randomAccessFile.Read(buffer, 0, buffer.Length);
            MemoryStream ms = new MemoryStream(buffer);
            Element element = null;
            try {
                BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All));
                element = (Element)formatter.Deserialize(ms);
            } finally {
                ms.Close();
            }

            return element;
        }

        /// <summary>
        /// Notify Element Eviction.
        /// </summary>
        /// <param name="diskElement">Evicted Disk element.</param>
        private void NotifyEvictionListeners(DiskElement diskElement) {
            if (_cache.NotificationService.HasElementEvictedListeners()) {
                Element element = null;
                try {
                    element = LoadElementFromDiskElement(diskElement);
                    _cache.NotificationService.NotifyElementEvicted(element, false);
                } catch (IOException e) {
                    _log.Error(
                        _name + "Cache: Could not notify disk store eviction of " + element.Key +
                        ". Error was " + e.Message, e);
                }
            }
        }

        /// <summary>
        /// It is enough that an element is expiring here. Notify even though there might be another
        /// element with the same key elsewhere in the stores.
        /// </summary>
        /// <param name="element">Expiry element.</param>
        private void NotifyExpiryListeners(Element element) {
            _cache.NotificationService.NotifyElementExpiry(element, false);
        }

        /// <summary>
        /// Reads Index to disk on startup.
        ///
        /// if the index file does not exist, it creates a new one.
        ///
        /// Note that the cache is locked for the entire time that the index is being written.
        /// </summary>
        /// <returns>True if the index was read successfully, false otherwise.</returns>
        private bool ReadIndex() {
            lock (this) {
                bool success = false;
                if (_indexFile.Exists) {
                    FileStream indexStream = null;
                    try {
                        indexStream = new FileStream(_indexFile.FullName, FileMode.Open, FileAccess.Read);
                        BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All));
                        _diskElements = (Dictionary<object, DiskElement>)formatter.Deserialize(indexStream);
                        _freeSpace = (List<DiskElement>)formatter.Deserialize(indexStream);
                        success = true;
                    } catch (IOException ioe) {
                        // Normal when creating the cache for the first time
                        if (_log.IsDebugEnabled) {
                            _log.Debug("IOException reading index. Creating new index. ", ioe);
                        }
                    } catch (SerializationException se) {
                        _log.Error("Corrupt index file. Creating new index.", se);
                    } finally {
                        if (indexStream != null) {
                            indexStream.Close();
                            indexStream.Dispose();
                        }

                        // Always zero out file. That way if there is a dirty shutdown, the file will still be empty
                        // the next time we start up and readIndex will automatically fail.
                        // If there was a problem reading the index this time we also want to zero it out.
                        this.CreateNewIndexFile();
                    }
                } else {
                    this.CreateNewIndexFile();
                }

                return success;
            }
        }

        /// <summary>
        /// Remove the old entry, if any.
        /// </summary>
        /// <param name="key">Key of element to remove.</param>
        private void RemoveOldEntryIfAny(object key) {
            DiskElement oldBlock = null;
            lock (_diskElements) {
                if (_diskElements.TryGetValue(key, out oldBlock)) {
                    _diskElements.Remove(key);
                }
            }

            if (oldBlock != null) {
                this.FreeBlock(oldBlock);
            }
        }

        /// <summary>
        /// Serialise un élément.
        /// </summary>
        /// <param name="element">Element à sérialiser.</param>
        /// <returns>Données sérialisées.</returns>
        private byte[] SerializeEntry(Element element) {
            MemoryStream ms = new MemoryStream(this.EstimatedPayloadSize);
            BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All));
            try {
                formatter.Serialize(ms, element);
            } catch (SerializationException e) {
                _log.Error(_name + "Cache : Failed to serialize. Reason: " + e.Message, e);
            }

            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }

        /// <summary>
        /// Both flushing and expiring contend for the same lock on diskElement, so
        /// might as well do them sequentially in the one thread.
        ///
        /// This thread is protected from Exceptions by only calling methods that guard
        /// against these.
        /// </summary>
        private void SpoolAndExpiryThreadMain() {
            long nextExpireTime = DateTime.Now.Ticks;
            while (!_spoolAndExpiryResetEvent.WaitOne(SpoolThreadInterval, false)) {
                this.ExceptionSafeFlushIfRequired();
                nextExpireTime = ExceptionSafeExpireElementsIfRequired(nextExpireTime);
                _cache.NotificationService.NotifyBackupCompete();
            }
        }

        /// <summary>
        /// Copy the reference of the old spool, not the contents. Avoid potential spike in memory usage
        ///
        /// use a new map making the reference swap above SAFE.
        /// </summary>
        /// <returns>The spool reference.</returns>
        private Dictionary<object, Element> SwapSpoolReference() {
            Dictionary<object, Element> copyOfSpool = null;
            lock (_spoolLock) {
                copyOfSpool = _spool;
                _spool = new Dictionary<object, Element>();
            }

            return copyOfSpool;
        }

        /// <summary>
        /// Ecrit un élément sur le disque.
        /// </summary>
        /// <param name="element">Element à écrire.</param>
        /// <param name="key">Clef de l'element.</param>
        private void WriteElement(Element element, object key) {
            try {
                int bufferLength;
                long expirationTime = element.ExpirationTime;

                byte[] buffer = null;
                try {
                    buffer = this.SerializeEntry(element);
                    bufferLength = buffer.Length;

                    DiskElement diskElement = this.CheckForFreeBlock(bufferLength);
                    _randomAccessFile.Seek(diskElement.Position, SeekOrigin.Begin);
                    _randomAccessFile.Write(buffer, 0, bufferLength);
                    buffer = null;

                    diskElement.PayloadSize = bufferLength;
                    diskElement.Key = key;
                    diskElement.ExpiryTime = expirationTime;
                    diskElement.HitCount = element.HitCount;
                    _totalSize += bufferLength;
                    _lastElementSize = bufferLength;

                    lock (_diskElements) {
                        _diskElements.Add(key, diskElement);
                    }
                } catch (OutOfMemoryException me) {
                    _log.Error("OutOfMemoryError on serialize: " + key, me);
                }
            } catch (IOException e) {
                _log.Error(
                    _name + "Cache: Failed to write element to disk '" + key
                        + "'. Initial cause was " + e.Message, e);
            }
        }

        /// <summary>
        /// Writes the Index to disk on shutdown.
        ///
        /// The index consists of the elements Map and the freeSpace List.
        ///
        /// Note that the cache is locked for the entire time that the index is being written.
        /// </summary>
        private void WriteIndex() {
            lock (this) {
                FileStream indexStream = null;
                try {
                    indexStream = new FileStream(_indexFile.FullName, FileMode.Create, FileAccess.Write);
                    BinaryFormatter formatter = new BinaryFormatter(null, new StreamingContext(StreamingContextStates.All));
                    formatter.Serialize(indexStream, _diskElements);
                    formatter.Serialize(indexStream, _freeSpace);
                } finally {
                    if (indexStream != null) {
                        indexStream.Close();
                        indexStream.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Ecrit ou remplace un élément sur le disque.
        /// </summary>
        /// <param name="element">Element to write.</param>
        private void WriteOrReplaceEntry(Element element) {
            if (element == null) {
                return;
            }

            this.RemoveOldEntryIfAny(element.Key);
            if (_maxElementsOnDisk > 0 && _diskElements.Count >= _maxElementsOnDisk) {
                this.EvictLfuDiskElement();
            }

            this.WriteElement(element, element.Key);
        }

        /// <summary>
        /// A reference to an on-disk elements.
        ///
        /// Copies of expiryTime and hitcount are held here as a performance optimisation, so
        /// that we do not need to load the data from Disk to get this often used information.
        /// </summary>
        [Serializable]
        private class DiskElement : IMetaData {

            /// <summary>
            /// The file pointer.
            /// </summary>
            public long Position {
                get;
                set;
            }

            /// <summary>
            /// The size used for data.
            /// </summary>
            public int PayloadSize {
                get;
                set;
            }

            /// <summary>
            /// The size of this element.
            /// </summary>
            public int BlockSize {
                get;
                set;
            }

            /// <summary>
            /// The key this element is mapped with in DiskElements. This is only a reference
            /// to the key. It is used in DiskElements and therefore the only memory cost is the
            /// reference.
            /// </summary>
            public object Key {
                get;
                internal set;
            }

            /// <summary>
            /// The expiry time in milliseconds.
            /// </summary>
            public long ExpiryTime {
                get;
                set;
            }

            /// <summary>
            /// The numbe of times the element has been requested and found in the cache.
            /// </summary>
            public long HitCount {
                get;
                set;
            }
        }
    }
}
