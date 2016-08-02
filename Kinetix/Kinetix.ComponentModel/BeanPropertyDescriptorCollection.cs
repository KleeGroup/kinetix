using System;
using System.Collections;
using System.Collections.Generic;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Collection des descriptions de propriété d'un objet.
    /// La clef utilisé est le nom de la propriété.
    /// </summary>
    [Serializable]
    public sealed class BeanPropertyDescriptorCollection : ICollection<BeanPropertyDescriptor> {
        private readonly Type _beanType;
        private readonly Dictionary<string, BeanPropertyDescriptor> _properties;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <exception cref="System.ArgumentNullException">Si bean type est null.</exception>
        internal BeanPropertyDescriptorCollection(Type beanType) {
            if (beanType == null) {
                throw new ArgumentNullException("beanType");
            }

            _beanType = beanType;
            _properties = new Dictionary<string, BeanPropertyDescriptor>();
        }

        /// <summary>
        /// Retourne le nombre d'élément de la collection.
        /// </summary>
        public int Count {
            get {
                return _properties.Count;
            }
        }

        /// <summary>
        /// Indique sur le collection est en lecture seule.
        /// </summary>
        bool ICollection<BeanPropertyDescriptor>.IsReadOnly {
            get {
                return true;
            }
        }

        /// <summary>
        /// Le filtre (?).
        /// </summary>
        public object Filter { get; set; }

        /// <summary>
        /// Retourne la description d'une propriété à partir de son nom.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <returns>Description de la propriété.</returns>
        public BeanPropertyDescriptor this[string propertyName] {
            get {
                try {
                    return _properties[propertyName];
                } catch (KeyNotFoundException e) {
                    throw new ArgumentException("Propriété " + propertyName + " non trouvée pour le type " + _beanType.FullName + ".", e);
                }
            }
        }

        /// <summary>
        /// Ajoute une nouvelle propriété à la collection.
        /// </summary>
        /// <param name="item">Propriété.</param>
        /// <exception cref="NotSupportedException">La collection est en lecture seule.</exception>
        void ICollection<BeanPropertyDescriptor>.Add(BeanPropertyDescriptor item) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Efface le contenu de la collection.
        /// </summary>
        /// <exception cref="NotSupportedException">La collection est en lecture seule.</exception>
        void ICollection<BeanPropertyDescriptor>.Clear() {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Indique si la collection contient une propriété.
        /// </summary>
        /// <param name="item">Propriété.</param>
        /// <returns>True si la propriété fait partie de la collection.</returns>
        bool ICollection<BeanPropertyDescriptor>.Contains(BeanPropertyDescriptor item) {
            return _properties.ContainsValue(item);
        }

        /// <summary>
        /// Indique si la collection contient une propriété nommée propertyName.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <returns>True si la collection contient la propriété.</returns>
        public bool Contains(string propertyName) {
            return _properties.ContainsKey(propertyName);
        }

        /// <summary>
        /// Copie la collection dans un tableau.
        /// </summary>
        /// <param name="array">Tableau.</param>
        /// <param name="arrayIndex">Position de début de copie.</param>
        void ICollection<BeanPropertyDescriptor>.CopyTo(BeanPropertyDescriptor[] array, int arrayIndex) {
            _properties.Values.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Supprime une propriété de la collection.
        /// </summary>
        /// <param name="item">Propriété.</param>
        /// <returns>True si la propriété a été supprimée.</returns>
        /// <exception cref="NotSupportedException">La collection est en lecture seule.</exception>
        bool ICollection<BeanPropertyDescriptor>.Remove(BeanPropertyDescriptor item) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Retourne un énumerateur sur la collection.
        /// </summary>
        /// <returns>Enumerateur.</returns>
        IEnumerator<BeanPropertyDescriptor> IEnumerable<BeanPropertyDescriptor>.GetEnumerator() {
            return _properties.Values.GetEnumerator();
        }

        /// <summary>
        /// Retourne un énumerateur sur la collection.
        /// </summary>
        /// <returns>Enumerateur.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return _properties.GetEnumerator();
        }

        /// <summary>
        /// Ajoute une nouvelle propriété à la collection.
        /// </summary>
        /// <param name="property">Propriété.</param>
        internal void Add(BeanPropertyDescriptor property) {
            _properties.Add(property.PropertyName, property);
        }
    }
}
