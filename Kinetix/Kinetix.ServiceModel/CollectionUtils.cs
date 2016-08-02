using System;
using System.Collections;
using System.Collections.Generic;
using Kinetix.ComponentModel;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Classe utilitaire permettant d'effectuer des opérations génériques sur les collections.
    /// </summary>
    public static class CollectionUtils {

        /// <summary>
        /// Ajoute les élements à une collection.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="source">Collection source.</param>
        /// <param name="collection">Eléments à ajouter.</param>
        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> collection) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            List<T> list = source as List<T>;
            if (list != null) {
                list.AddRange(collection);
            }
            else {
                foreach (T item in collection) {
                    source.Add(item);
                }
            }
        }

        /// <summary>
        /// Effectue un foreach sur un IEnumerable.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="source">Collection source.</param>
        /// <param name="predicate">Prédicat.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> predicate) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            if (predicate == null) {
                throw new ArgumentNullException("predicate");
            }

            foreach (var item in source) {
                predicate(item);
            }
        }

        /// <summary>
        /// Retourne la collection filtrée.
        /// </summary>
        /// <param name="collection">La collection à filtrer.</param>
        /// <returns>La collection filtrée.</returns>
        /// <remarks>Utilisé dans Kinetix.WebControls.</remarks>
        public static ICollection Filter(ICollection collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            Type collectionType = collection.GetType();
            Type[] innerTypes = collectionType.GetGenericArguments();
            if (innerTypes.Length != 1) {
                throw new NotSupportedException();
            }

            Type genericType = typeof(List<>).MakeGenericType(innerTypes[0]);
            if (!(genericType is IActivable)) {
                return collection;
            }

            IList newCollection = (IList)Activator.CreateInstance(genericType);
            foreach (object obj in collection) {
                IActivable iActivable = obj as IActivable;
                if (iActivable != null && iActivable.IsActif != null && iActivable.IsActif == true) {
                    newCollection.Add(obj);
                }
            }

            return newCollection;
        }

        /// <summary>
        /// Retourne si la collection contient l'élément.
        /// </summary>
        /// <param name="collection">Collection a traiter.</param>
        /// <param name="pkValue">Valeur de la clef primaire.</param>
        /// <returns>True si l'objet est inclus, False sinon.</returns>
        /// <remarks>Utilisé dans Kinetix.WebControls.</remarks>
        public static bool Contains(ICollection collection, object pkValue) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            if (pkValue == null) {
                throw new ArgumentNullException("pkValue");
            }

            BeanDefinition definition = BeanDescriptor.GetCollectionDefinition(collection);
            if (definition.PrimaryKey == null) {
                throw new NotSupportedException();
            }

            IEnumerator enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext()) {
                object tmp = enumerator.Current;
                if (pkValue.Equals(definition.PrimaryKey.GetValue(tmp))) {
                    return true;
                }
            }

            return false;
        }
    }
}
