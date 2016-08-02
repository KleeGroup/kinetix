using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Kinetix.ComponentModel.Search {

    /// <summary>
    /// Représente un dictionnaire ordonné comme une liste.
    /// C'est à la fois une liste de KeyValuePair et un Dictionnaire.
    /// Est sérialisé comme une liste de dictionnaire, en accordance avec la spec Focus 0.13.0.
    /// </summary>
    /// <typeparam name="TKey">Clé du dictionnaire.</typeparam>
    /// <typeparam name="TValue">Valeur du dictionnaire</typeparam>
    public class OrderedDictionary<TKey, TValue> : IList<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IOrderedDictionary {

        private List<Dictionary<TKey, TValue>> _innerList = new List<Dictionary<TKey, TValue>>();

        /// <inheritdoc />
        public int Count => _innerList.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public ICollection<TKey> Keys => _innerList.SelectMany(item => item.Keys).ToList();

        /// <inheritdoc />
        public ICollection<TValue> Values => _innerList.SelectMany(item => item.Values).ToList();

        /// <summary>
        /// Accesseur à la liste interne pour le sérialiseur.
        /// </summary>
        public IList InnerList {
            get { return _innerList; }
            protected set { _innerList = value as List<Dictionary<TKey, TValue>>; }
        }

        /// <inheritdoc />
        public TValue this[TKey key] {
            get {
                var value = default(TValue);
                try {
                    _innerList.First(item => item.TryGetValue(key, out value));
                } catch {
                    throw new KeyNotFoundException();
                }

                return value;
            }

            set {
                _innerList[0][key] = value;
            }
        }

        /// <inheritdoc />
        public KeyValuePair<TKey, TValue> this[int index] {
            get {
                return _innerList[index].First();
            }

            set {
                _innerList[index] = ToDict(value);
            }
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item) => _innerList.Add(ToDict(item));

        /// <inheritdoc cref="IDictionary{TKey, TValue}.Add" />
        public void Add(TKey key, TValue value) => _innerList.Add(new Dictionary<TKey, TValue> { { key, value } });

        /// <inheritdoc />
        public void Clear() => _innerList.Clear();

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item) => _innerList.Any(dict => dict.Contains(item));

        /// <inheritdoc cref="IDictionary{TKey, TValue}.ContainsKey" />
        public bool ContainsKey(TKey key) => _innerList.Any(dict => dict.ContainsKey(key));

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => _innerList.CopyTo(array.Select(item => ToDict(item)).ToArray(), arrayIndex);

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _innerList.Select(item => item.First()).GetEnumerator();

        /// <inheritdoc cref="IList{T}.IndexOf" />
        public int IndexOf(KeyValuePair<TKey, TValue> item) => _innerList.IndexOf(_innerList.First(dict => dict.Contains(item)));

        /// <inheritdoc cref="IList{T}.Insert" />
        public void Insert(int index, KeyValuePair<TKey, TValue> item) => _innerList.Insert(index, ToDict(item));

        /// <inheritdoc cref="IDictionary{TKey, TValue}.Remove" />
        public bool Remove(TKey key) => _innerList.Remove(_innerList.FirstOrDefault(dict => dict.ContainsKey(key)));

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item) => _innerList.Remove(ToDict(item));

        /// <inheritdoc cref="IList{T}.RemoveAt" />
        public void RemoveAt(int index) => _innerList.RemoveAt(index);

        /// <inheritdoc cref="IDictionary{TKey, TValue}.TryGetValue" />
        public bool TryGetValue(TKey key, out TValue value) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// KeyValuePair to Dictionary.
        /// </summary>
        /// <param name="item">KeyValuePair.</param>
        /// <returns>Dictionary.</returns>
        protected Dictionary<TKey, TValue> ToDict(KeyValuePair<TKey, TValue> item) =>
             new Dictionary<TKey, TValue> { [item.Key] = item.Value };
    }
}
