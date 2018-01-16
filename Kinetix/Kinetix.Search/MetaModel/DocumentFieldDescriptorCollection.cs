using System;
using System.Collections;
using System.Collections.Generic;

namespace Kinetix.Search.MetaModel {

    /// <summary>
    /// Collection des descriptions de propriété d'un objet.
    /// La clef utilisé est le nom de la propriété.
    /// </summary>
    [Serializable]
    public sealed class DocumentFieldDescriptorCollection : IEnumerable<DocumentFieldDescriptor> {

        private readonly Type _beanType;
        private readonly Dictionary<string, DocumentFieldDescriptor> _properties;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <exception cref="System.ArgumentNullException">Si bean type est null.</exception>
        internal DocumentFieldDescriptorCollection(Type beanType) {
            if (beanType == null) {
                throw new ArgumentNullException("beanType");
            }

            _beanType = beanType;
            _properties = new Dictionary<string, DocumentFieldDescriptor>();
        }

        /// <summary>
        /// Retourne la description d'une propriété à partir de son nom.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <returns>Description de la propriété.</returns>
        public DocumentFieldDescriptor this[string propertyName] {
            get {
                try {
                    return _properties[propertyName];
                } catch (KeyNotFoundException e) {
                    throw new ArgumentException("Propriété " + propertyName + " non trouvée pour le type " + _beanType.FullName + ".", e);
                }
            }

            set {
                _properties[propertyName] = value;
            }
        }

        /// <summary>
        /// Indique si la collection contient une propriété.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <returns><code>True</code> si la collection contient la propriété.</returns>
        public bool HasProperty(string propertyName) {
            return _properties.ContainsKey(propertyName);
        }

        /// <summary>
        /// Obtient l'énumérateur.
        /// </summary>
        /// <returns>Enumérateur.</returns>
        public IEnumerator<DocumentFieldDescriptor> GetEnumerator() {
            return _properties.Values.GetEnumerator();
        }

        /// <summary>
        /// Obtient l'énumérateur.
        /// </summary>
        /// <returns>Enumérateur.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return _properties.Values.GetEnumerator();
        }
    }
}
