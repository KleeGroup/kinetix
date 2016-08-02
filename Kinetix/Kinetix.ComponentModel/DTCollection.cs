using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Collection générique gardant en mémoire les éléments ajoutés et les éléments supprimés.
    /// </summary>
    /// <typeparam name="T">Type contenu dans la liste.</typeparam>
    [Serializable]
    public class DTCollection<T> : IDTCollection<T>, IDTCollection
        where T : class {

        private readonly List<T> _addedItems = new List<T>();
        private readonly List<T> _items = new List<T>();
        private readonly List<T> _removedItems = new List<T>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        public DTCollection() {
            _items = new List<T>();
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="capacity">Capacité de la liste.</param>
        public DTCollection(int capacity) {
            _items = new List<T>(capacity);
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="collection">Elements à ajouter.</param>
        public DTCollection(IEnumerable<T> collection) {
            _items = new List<T>(collection);
        }

        /// <summary>
        /// Retourne la liste des éléments supprimés.
        /// </summary>
        public IList<T> RemovedItems {
            get {
                return new ReadOnlyCollection<T>(_removedItems);
            }
        }

        /// <summary>
        /// Retourne la liste des éléments supprimés.
        /// </summary>
        IList IDTCollection.RemovedItems {
            get {
                return _removedItems;
            }
        }

        /// <summary>
        /// Retourne le nombre d'éléments présent dans la collection.
        /// </summary>
        public int Count {
            get {
                return _items.Count;
            }
        }

        /// <summary>
        /// Retourne si la collection est en lecture seule.
        /// </summary>
        public bool IsReadOnly {
            get {
                return false;
            }
        }

        /// <summary>
        /// Retourne si la taille de la collection est fixe.
        /// </summary>
        public bool IsFixedSize {
            get {
                return false;
            }
        }

        /// <summary>
        /// Retourne si la collection est synchronisée.
        /// </summary>
        public bool IsSynchronized {
            get {
                return false;
            }
        }

        /// <summary>
        /// Retourne l'objet responsable de la synchronisation.
        /// </summary>
        public object SyncRoot {
            get {
                return null;
            }
        }

        /// <summary>
        /// Retourne l'élément à l'indice.
        /// </summary>
        /// <param name="index">Indice cible.</param>
        /// <returns>L'élément.</returns>
        public T this[int index] {
            get {
                return _items[index];
            }

            set {
                _items[index] = value;
            }
        }

        /// <summary>
        /// Retourne l'élément à l'indice.
        /// </summary>
        /// <param name="index">Indice cible.</param>
        /// <returns>L'élément.</returns>
        object IList.this[int index] {
            get {
                return _items[index];
            }

            set {
                _items[index] = (T)value;
            }
        }

        /// <summary>
        /// Retourne l'indice de l'item dans la liste.
        /// -1 si non trouvé.
        /// </summary>
        /// <param name="item">Item à ajouter.</param>
        /// <returns>Indice de l'élément.</returns>
        public int IndexOf(T item) {
            return _items.IndexOf(item);
        }

        /// <summary>
        /// Insère l'élément à l'indice spécifié.
        /// </summary>
        /// <param name="index">Indice d'insertion (commençant à 0).</param>
        /// <param name="item">Elément à ajouter.</param>
        public void Insert(int index, T item) {
            _addedItems.Add(item);
            _items.Insert(index, item);
        }

        /// <summary>
        /// Supprime l'élément à l'indice.
        /// </summary>
        /// <param name="index">Indice cible.</param>
        public void RemoveAt(int index) {
            _removedItems.Add(_items.ElementAt(index));
            _items.RemoveAt(index);
        }

        /// <summary>
        /// Ajoute l'élément dans la collection.
        /// </summary>
        /// <param name="item">Element.</param>
        public void Add(T item) {
            _addedItems.Add(item);
            _items.Add(item);
        }

        /// <summary>
        /// Supprime tous les éléments de la liste.
        /// </summary>
        public void Clear() {
            _addedItems.Clear();
            _removedItems.AddRange(_items);
            _items.Clear();
        }

        /// <summary>
        /// Retourne si la liste contient un élément.
        /// </summary>
        /// <param name="item">L'élément testé.</param>
        /// <returns><code>True</code> si l'élément est inclus dans la collection, <code>False</code> sinon.</returns>
        public bool Contains(T item) {
            return _items.Contains(item);
        }

        /// <summary>
        /// Recopie la collection dans un tableau.
        /// </summary>
        /// <param name="array">Tableau cible.</param>
        /// <param name="arrayIndex">Index de recopie.</param>
        public void CopyTo(T[] array, int arrayIndex) {
            _items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Supprime un élément.
        /// </summary>
        /// <param name="item">L'élément à supprimer.</param>
        /// <returns><code>True</code> si l'élément a été supprimé, <code>False</code> sinon.</returns>
        public bool Remove(T item) {
            if (_items.Remove(item)) {
                _removedItems.Add(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Retourne un énumerateur sur la collection.
        /// </summary>
        /// <returns>Enumerateur.</returns>
        public IEnumerator<T> GetEnumerator() {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Ajoute l'élément dans la collection.
        /// </summary>
        /// <param name="value">Element.</param>
        /// <returns>L'indice d'insertion dans la collection.</returns>
        public int Add(object value) {
            _addedItems.Add((T)value);
            _items.Add((T)value);
            return _items.Count - 1;
        }

        /// <summary>
        /// Retourne si la collection contient l'élément.
        /// </summary>
        /// <param name="value">Element.</param>
        /// <returns><code>True</code> si la collection contient l'élément, <code>False</code> sinon.</returns>
        public bool Contains(object value) {
            return Contains((T)value);
        }

        /// <summary>
        /// Retourne l'indice de l'objet dans la collection, -1 si non présent.
        /// </summary>
        /// <param name="value">Objet.</param>
        /// <returns>Indice dans la collection.</returns>
        public int IndexOf(object value) {
            return _items.IndexOf((T)value);
        }

        /// <summary>
        /// Insère l'élément à l'indice précisé.
        /// </summary>
        /// <param name="index">Indice de l'élement.</param>
        /// <param name="value">Objet à insérer.</param>
        public void Insert(int index, object value) {
            _addedItems.Add((T)value);
            _items.Insert(index, (T)value);
        }

        /// <summary>
        /// Supprime l'élément de la collection.
        /// </summary>
        /// <param name="value">Element à supprimer.</param>
        public void Remove(object value) {
            if (_items.Remove((T)value)) {
                _removedItems.Add((T)value);
            }
        }

        /// <summary>
        /// Recopie la collection dans un tableau a partir de l'indice spécifié.
        /// </summary>
        /// <param name="array">Tableau cible.</param>
        /// <param name="index">Indice de recopie.</param>
        public void CopyTo(Array array, int index) {
            _items.CopyTo((T[])array, index);
        }

        /// <summary>
        /// Retourne un énumerateur sur la collection.
        /// </summary>
        /// <returns>Enumerateur.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return _items.GetEnumerator();
        }
    }
}
