using System;
using System.Collections;
using System.Collections.Generic;

namespace Kinetix.Caching.Store {
    /// <summary>
    /// A map of objects whose mapping entries are sequenced based on the order in
    /// which they were added.  This data structure has fast O(1) search
    /// time, deletion time, and insertion time.
    ///
    /// Although this map is sequenced, it cannot implement
    /// List because of incompatible interface definitions.
    /// The remove methods in List and Map have different return values
    ///
    /// This class is not thread safe.
    /// </summary>
    /// <typeparam name="TKey">Key Type.</typeparam>
    /// <typeparam name="TValue">Value Type.</typeparam>
    public class SequencedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary {

        /// <summary>
        /// Sentinel used to hold the head and tail of the list of entries.
        /// </summary>
        private Entry _sentinel;

        /// <summary>
        /// Map of key to entries.
        /// </summary>
        private Dictionary<TKey, Entry> _entries;

        /// <summary>
        /// Holds the number of modifications that have occurred to the map,
        /// excluding modifications made through a collection view's iterator.
        /// This is used to create a fail-fast behavior with the iterators.
        /// </summary>
        private long _modCount = 0;

        /// <summary>
        /// Construct a new sequenced hash map with default initial size and load
        /// factor.
        /// </summary>
        public SequencedDictionary() {
            _sentinel = CreateSentinel();
            _entries = new Dictionary<TKey, Entry>();
        }

        /// <summary>
        /// Construct a new sequenced hash map with the specified initial size and
        /// default load factor.
        /// </summary>
        /// <param name="initialSize">The initial size for the dictionary.</param>
        public SequencedDictionary(int initialSize) {
            _sentinel = CreateSentinel();
            _entries = new Dictionary<TKey, Entry>(initialSize);
        }

        /// <summary>
        /// Retourne le nombre d'entrée du dictionnaire.
        /// </summary>
        public int Count {
            get {
                return _entries.Count;
            }
        }

        /// <summary>
        /// Retourne la première clef.
        /// </summary>
        public TKey FirstKey {
            get {
                return _sentinel.Next.Key;
            }
        }

        /// <summary>
        /// Retourne la première valeur.
        /// </summary>
        public TValue FirstValue {
            get {
                return _sentinel.Next.Value;
            }
        }

        /// <summary>
        /// Retourne une collection de clef.
        /// </summary>
        public ICollection<TKey> Keys {
            get {
                return _entries.Keys;
            }
        }

        /// <summary>
        /// Retourne la dernière clef.
        /// </summary>
        public TKey LastKey {
            get {
                return _sentinel.Prev.Key;
            }
        }

        /// <summary>
        /// Retourne la dernière valeur.
        /// </summary>
        public TValue LastValue {
            get {
                return _sentinel.Prev.Value;
            }
        }

        /// <summary>
        /// Indique si le dictionnaire est de taille fixe.
        /// </summary>
        public bool IsFixedSize {
            get {
                return false;
            }
        }

        /// <summary>
        /// Indique si la liste est en lecture seule.
        /// </summary>
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
            get {
                return false;
            }
        }

        /// <summary>
        /// Indique si la liste est en lecture seule.
        /// </summary>
        public bool IsReadOnly {
            get {
                return false;
            }
        }

        /// <summary>
        /// Indique si la collection est synchronisée.
        /// </summary>
        public bool IsSynchronized {
            get {
                return false;
            }
        }

        /// <summary>
        /// Retourne la liste des clefs de la collection.
        /// </summary>
        ICollection IDictionary.Keys {
            get {
                return _entries.Keys;
            }
        }

        /// <summary>
        /// Donne la racine de synchronisation pour la collection.
        /// </summary>
        public object SyncRoot {
            get {
                return this;
            }
        }

        /// <summary>
        /// Retourne les valeurs de la collection.
        /// </summary>
        ICollection<TValue> IDictionary<TKey, TValue>.Values {
            get {
                List<TValue> values = new List<TValue>(_entries.Count);
                foreach (Entry e in _entries.Values) {
                    values.Add(e.Value);
                }

                return values;
            }
        }

        /// <summary>
        /// Retourne les valeurs de la collection.
        /// </summary>
        public ICollection Values {
            get {
                List<TValue> values = new List<TValue>(_entries.Count);
                foreach (Entry e in _entries.Values) {
                    values.Add(e.Value);
                }

                return values;
            }
        }

        /// <summary>
        /// Obtient ou définit la valeur associé à une clef.
        /// </summary>
        /// <param name="key">Clef.</param>
        /// <returns>Valeur.</returns>
        public virtual TValue this[TKey key] {
            get {
                Entry entry;
                if (_entries.TryGetValue(key, out entry)) {
                    return entry.Value;
                } else {
                    return default(TValue);
                }
            }

            set {
                this.Add(key, value);
            }
        }

        /// <summary>
        /// Obtient ou définit la valeur associé à une clef.
        /// </summary>
        /// <param name="key">Clef.</param>
        /// <returns>Valeur.</returns>
        object IDictionary.this[object key] {
            get {
                return this[(TKey)key];
            }

            set {
                this[(TKey)key] = (TValue)value;
            }
        }

        /// <summary>
        /// Ajoute une nouvelle entrée au dictionnaire.
        /// </summary>
        /// <param name="key">Clef.</param>
        /// <param name="value">Valeur.</param>
        public void Add(TKey key, TValue value) {
            _modCount++;

            Entry e;
            if (_entries.TryGetValue(key, out e)) {
                RemoveEntry(e);
                e.Value = value;
            } else {
                e = new Entry(key, value);
                _entries.Add(key, e);
            }

            InsertEntry(e);
        }

        /// <summary>
        /// Supprime tous les éléments.
        /// </summary>
        public void Clear() {
            _modCount++;
            _entries.Clear();
            _sentinel.Next = _sentinel;
            _sentinel.Prev = _sentinel;
        }

        /// <summary>
        /// Indique si le dictionnaire contient une entrée pour une clef.
        /// </summary>
        /// <param name="key">Clef.</param>
        /// <returns>True si la clef est présente.</returns>
        public bool ContainsKey(TKey key) {
            return _entries.ContainsKey(key);
        }

        /// <summary>
        /// Copie la collection dans un tableau.
        /// </summary>
        /// <param name="array">Tableau.</param>
        /// <param name="arrayIndex">Index de début de copie.</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }

            int index = 0;
            foreach (TKey key in _entries.Keys) {
                array[index + arrayIndex] = new KeyValuePair<TKey, TValue>(key, _entries[key].Value);
                index++;
            }
        }

        /// <summary>
        /// Supprimme une entrée du dictionnaire.
        /// </summary>
        /// <param name="key">Key to remove.</param>
        /// <returns>True si supprimée.</returns>
        public bool Remove(TKey key) {
            return this.RemoveImpl(key);
        }

        /// <summary>
        /// Ajoute une nouvelle entrée au dictionnaire.
        /// </summary>
        /// <param name="item">Item à ajouter.</param>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Ajoute une nouvelle entrée au dictionnaire.
        /// </summary>
        /// <param name="key">Clef.</param>
        /// <param name="value">Valeur.</param>
        void IDictionary.Add(object key, object value) {
            this.Add((TKey)key, (TValue)value);
        }

        /// <summary>
        /// Indique si le dictionnaire contient une entrée pour une clef.
        /// </summary>
        /// <param name="key">Clef.</param>
        /// <returns>True si la clef est présente.</returns>
        public bool Contains(object key) {
            return this.ContainsKey((TKey)key);
        }

        /// <summary>
        /// Indique si le dictionnaire contient une entrée pour une clef.
        /// </summary>
        /// <param name="item">Valeur.</param>
        /// <returns>True si la clef est présente.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) {
            return this.ContainsKey(item.Key);
        }

        /// <summary>
        /// Copie la collection dans un tableau.
        /// </summary>
        /// <param name="array">Tableau.</param>
        /// <param name="index">Index de début de copie.</param>
        void ICollection.CopyTo(Array array, int index) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }

            int i = 0;
            foreach (TKey key in _entries.Keys) {
                array.SetValue(_entries[key].Value, index + i);
                i++;
            }
        }

        /// <summary>
        /// Supprimme une entrée du dictionnaire.
        /// </summary>
        /// <param name="item">Item à supprimer.</param>
        /// <returns>True si supprimé.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) {
            return this.Remove(item.Key);
        }

        /// <summary>
        /// Supprimme une entrée du dictionnaire.
        /// </summary>
        /// <param name="key">Clef de l'entrée à supprimer.</param>
        void IDictionary.Remove(object key) {
            this.Remove((TKey)key);
        }

        /// <summary>
        /// Met à jour une valeur.
        /// </summary>
        /// <param name="key">Clef.</param>
        /// <param name="value">Valeur.</param>
        /// <returns>True si la valeur a été mise à jour.</returns>
        public bool TryGetValue(TKey key, out TValue value) {
            Entry entry;
            if (_entries.TryGetValue(key, out entry)) {
                value = entry.Value;
                return true;
            } else {
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Retourne un énumérateur.
        /// </summary>
        /// <returns>Enumérateur.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            Entry e = _sentinel.Next;
            while (e != _sentinel) {
                yield return new KeyValuePair<TKey, TValue>(e.Key, e.Value);
                e = e.Next;
            }

            yield break;
        }

        /// <summary>
        /// Retourne un énumérateur.
        /// </summary>
        /// <returns>Enumérateur.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            Entry e = _sentinel.Next;
            while (e != _sentinel) {
                yield return e.Value;
                e = e.Next;
            }

            yield break;
        }

        /// <summary>
        /// Retourne un énumérateur.
        /// </summary>
        /// <returns>Enumérateur.</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new SequencedEnumerator(_sentinel);
        }

        /// <summary>
        /// Construct an empty sentinel used to hold the head (sentinel.next) and the
        /// tail (sentinel.prev) of the list.  The sentinel has a <code>null</code>
        /// key and value.
        /// </summary>
        /// <returns>Sentinel Entry.</returns>
        private static Entry CreateSentinel() {
            Entry s = new Entry(default(TKey), default(TValue));
            s.Prev = s;
            s.Next = s;
            return s;
        }

        /// <summary>
        /// Removes an internal entry from the linked list.  This does not remove
        /// it from the underlying map.
        /// </summary>
        /// <param name="entry">Entry to remove.</param>
        private static void RemoveEntry(Entry entry) {
            entry.Next.Prev = entry.Prev;
            entry.Prev.Next = entry.Next;
        }

        /// <summary>
        /// Inserts a new internal entry to the tail of the linked list.  This does
        /// not add the entry to the underlying map.
        /// </summary>
        /// <param name="entry">Entry to insert.</param>
        private void InsertEntry(Entry entry) {
            entry.Next = _sentinel;
            entry.Prev = _sentinel.Prev;
            _sentinel.Prev.Next = entry;
            _sentinel.Prev = entry;
        }

        /// <summary>
        /// Fully remove an entry from the map, returning the old entry or null if
        /// there was no such entry with the specified key.
        /// </summary>
        /// <param name="key">Key of the element to remove.</param>
        /// <returns>True if the remove was successfull, False otherwise.</returns>
        private bool RemoveImpl(TKey key) {
            Entry entry;
            if (_entries.TryGetValue(key, out entry)) {
                _entries.Remove(key);
                _modCount++;
                RemoveEntry(entry);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Entry that doubles as a node in the linked list
        /// of sequenced mappings.
        /// </summary>
        private class Entry {
            private TKey _key;
            private TValue _value;

            /// <summary>
            /// Crée une nouvelle instance.
            /// </summary>
            /// <param name="key">Entry key.</param>
            /// <param name="value">Entry value.</param>
            public Entry(TKey key, TValue value) {
                this._key = key;
                this._value = value;
                this.Next = null;
                this.Prev = null;
            }

            /// <summary>
            /// Retourne la clef.
            /// </summary>
            public TKey Key {
                get {
                    return this._key;
                }
            }

            /// <summary>
            /// Obtient ou définit la valeur de l'entrée.
            /// </summary>
            public TValue Value {
                get {
                    return this._value;
                }

                set {
                    this._value = value;
                }
            }

            /// <summary>
            /// Entrée suivante dans la liste.
            /// </summary>
            public Entry Next {
                get;
                set;
            }

            /// <summary>
            /// Entrée précédente dans la liste.
            /// </summary>
            public Entry Prev {
                get;
                set;
            }
        }

        /// <summary>
        /// Enumérateur sur le dictionnaire.
        /// </summary>
        private class SequencedEnumerator : IDictionaryEnumerator {

            private readonly Entry _sentinel;
            private Entry _current;

            /// <summary>
            /// Crée une nouvelle instance.
            /// </summary>
            /// <param name="sentinel">Sentinel.</param>
            public SequencedEnumerator(Entry sentinel) {
                _current = sentinel;
                _sentinel = sentinel;
            }

            /// <summary>
            /// Retourne l'entrée courante.
            /// </summary>
            public DictionaryEntry Entry {
                get {
                    if (_current == _sentinel) {
                        throw new NotSupportedException();
                    }

                    return new DictionaryEntry(_current.Key, _current.Value);
                }
            }

            /// <summary>
            /// Retourne la clef courante.
            /// </summary>
            public object Key {
                get {
                    if (_current == _sentinel) {
                        throw new NotSupportedException();
                    }

                    return _current.Key;
                }
            }

            /// <summary>
            /// Retourne la valeur courante.
            /// </summary>
            public object Value {
                get {
                    if (_current == _sentinel) {
                        throw new NotSupportedException();
                    }

                    return _current.Value;
                }
            }

            /// <summary>
            /// Retourne la valeur courante.
            /// </summary>
            public object Current {
                get {
                    if (_current == _sentinel) {
                        throw new NotSupportedException();
                    }

                    return new DictionaryEntry(_current.Key, _current.Value);
                }
            }

            /// <summary>
            /// Avance d'un élément dans la collection.
            /// </summary>
            /// <returns>True si un élément est disponible.</returns>
            public bool MoveNext() {
                _current = _current.Next;
                return _current != _sentinel;
            }

            /// <summary>
            /// Réinitialise l'itérateur.
            /// </summary>
            public void Reset() {
                _current = _sentinel;
            }
        }
    }
}
