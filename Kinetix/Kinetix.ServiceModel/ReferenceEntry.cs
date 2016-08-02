using System;
using System.Collections;
using System.Collections.Generic;
using Kinetix.ComponentModel;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Entrée pour une liste de référence.
    /// </summary>
    /// <typeparam name="T">Type de la liste de référence.</typeparam>
    [Serializable]
    internal class ReferenceEntry<T> : IReferenceEntry
        where T : new() {

        private const string DefaultLocale = "Default";
        private const string PropertyIsActif = "IsActif";

        private readonly Dictionary<string, ICollection<T>> _localizedList = new Dictionary<string, ICollection<T>>();
        private readonly Dictionary<string, T> _resourceMap = new Dictionary<string, T>();

        /// <summary>
        /// Crée une nouvelle entrée pour le type.
        /// </summary>
        /// <param name="referenceList">Liste de référence.</param>
        /// <param name="resourceList">Liste des resources disponible.</param>
        void IReferenceEntry.Initialize(ICollection referenceList, ICollection<ReferenceResource> resourceList) {
            BeanDefinition definition = BeanDescriptor.GetDefinition(typeof(T));
            BeanPropertyDescriptor primaryKey = definition.PrimaryKey;
            if (primaryKey == null) {
                throw new NotSupportedException("Reference type " + typeof(T).FullName + " doesn't have a primary key defined. Use the ColumnAttribute to set the primary key property.");
            }

            BeanPropertyDescriptor propertyIsActif = null;
            if (definition.Properties.Contains(PropertyIsActif)) {
                propertyIsActif = definition.Properties[PropertyIsActif];
            }

            ICollection<T> initialList = (ICollection<T>)referenceList;
            ICollection<T> activeList = (propertyIsActif == null) ? initialList : new List<T>();

            foreach (T reference in initialList) {
                _resourceMap.Add(DefaultLocale + primaryKey.GetValue(reference), reference);

                if (propertyIsActif != null) {
                    bool? isActif = (bool?)propertyIsActif.GetValue(reference);
                    if (isActif.HasValue && isActif.Value) {
                        activeList.Add(reference);
                    }
                }
            }

            _localizedList.Add(DefaultLocale, activeList);

            if (resourceList == null) {
                return;
            }

            BeanFactory<T> factory = new BeanFactory<T>();
            foreach (ReferenceResource resource in resourceList) {
                T reference;
                string locale = resource.Locale.Trim();
                if (!_localizedList.ContainsKey(locale)) {
                    // Construction des entrées pour la locale.
                    List<T> list = new List<T>();
                    foreach (T initialReference in initialList) {
                        reference = factory.CloneBean(initialReference);
                        _resourceMap.Add(locale + primaryKey.GetValue(reference), reference);

                        if (propertyIsActif != null) {
                            bool? isActif = (bool?)propertyIsActif.GetValue(reference);
                            if (isActif.HasValue && isActif.Value) {
                                list.Add(reference);
                            }
                        } else {
                            list.Add(reference);
                        }
                    }

                    _localizedList.Add(locale, list);
                }

                reference = _resourceMap[locale + resource.Id];
                definition.Properties[resource.PropertyName].SetValue(reference, resource.Label);
            }
        }

        /// <summary>
        /// Retourne la liste de référence pour une locale.
        /// </summary>
        /// <param name="locale">Locale.</param>
        /// <returns>Liste de référence.</returns>
        ICollection IReferenceEntry.GetReferenceList(string locale) {
            return _localizedList.ContainsKey(locale) ? (ICollection)_localizedList[locale] : (ICollection)_localizedList[DefaultLocale];
        }

        /// <summary>
        /// Retourne un object de référence pour une locale.
        /// </summary>
        /// <param name="locale">Locale.</param>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <returns>Objet.</returns>
        object IReferenceEntry.GetReferenceValue(string locale, object primaryKey) {
            T value;
            /* Cherche la valeur pour la locale demandée. */
            if (_resourceMap.TryGetValue(locale + primaryKey, out value)) {
                return value;
            }

            /* Cherche la valeur pour la locale par défaut. */
            if (_resourceMap.TryGetValue(DefaultLocale + primaryKey, out value)) {
                return value;
            }

            /* Aucune valeur trouvée. */
            throw new NotSupportedException("Reference entry " + primaryKey + " is missing for " + typeof(T) + ".");
        }
    }
}
