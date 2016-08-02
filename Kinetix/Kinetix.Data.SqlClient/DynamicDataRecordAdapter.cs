using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using Kinetix.ComponentModel;
using log4net;

namespace Kinetix.Data.SqlClient {
    /// <summary>
    /// Classe utilitaire liée au constructeur de collection.
    /// </summary>
    /// <typeparam name="T">Type de collection à construire.</typeparam>
    internal class DynamicDataRecordAdapter<T> : IDataRecordAdapter<T>
        where T : new() {

        private readonly List<PropertyDescriptor>[] _propertyArray;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="record">Datarecord.</param>
        public DynamicDataRecordAdapter(IDataRecord record) {
            PropertyDescriptor prop;
            BeanDefinition definition = BeanDescriptor.GetDefinition(typeof(T), true);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

            int fieldCount = record.FieldCount;
            List<PropertyDescriptor>[] propertyArray = new List<PropertyDescriptor>[fieldCount];

            // Création d'un dictionnaire d'accès aux propriétés du bean.
            Dictionary<string, PropertyDescriptor> dic = new Dictionary<string, PropertyDescriptor>();
            foreach (BeanPropertyDescriptor property in definition.Properties) {
                prop = properties[property.PropertyName];
                dic.Add(property.PropertyName, prop);
                if (property.MemberName != null) {
                    dic.Add(property.MemberName.ToUpper(CultureInfo.InvariantCulture), prop);
                }
            }

            // Itération sur les noms des champs ramenés pour construire l'index d'accès.
            for (int i = 0; i < fieldCount; i++) {
                string fieldName = record.GetName(i);
                string[] fieldPathItems = fieldName.Split('.');
                propertyArray[i] = new List<PropertyDescriptor>(fieldPathItems.Length);

                if (dic.TryGetValue(fieldPathItems[0], out prop)) {
                    propertyArray[i].Add(prop);
                } else if (dic.TryGetValue(fieldPathItems[0].ToUpper(CultureInfo.InvariantCulture), out prop)) {
                    propertyArray[i].Add(prop);
                }

                if (fieldPathItems.Length != 1 && propertyArray[i].Count == 1) {
                    PropertyDescriptor composedObjectProperty = properties[fieldPathItems[0]];
                    for (int j = 1; j < fieldPathItems.Length; ++j) {
                        string pathItem = fieldPathItems[j];
                        PropertyDescriptor property = TypeDescriptor.GetProperties(composedObjectProperty.PropertyType)[pathItem];
                        if (property == null) {
                            BeanDefinition composedObjectDefinition = BeanDescriptor.GetDefinition(composedObjectProperty.PropertyType);
                            if (composedObjectDefinition != null) {
                                BeanPropertyDescriptor desc = composedObjectDefinition.Properties.First(x => pathItem.Equals(x.MemberName));
                                if (desc != null) {
                                    property = TypeDescriptor.GetProperties(composedObjectProperty.PropertyType)[desc.PropertyName];
                                }
                            }
                        }

                        if (property == null) {
                            throw new NotSupportedException("Property '" + fieldName + "' was not found in descriptor '" + typeof(T).Name + "' !");
                        }

                        propertyArray[i].Add(property);
                        composedObjectProperty = property;
                    }
                }
            }

            // Log si l'élément remonté ne correspond a aucune propriété de l'objet de transfert.
            ILog log = LogManager.GetLogger("Sql");
            bool throwExceptionOnPropertyNotFound = ConfigurationManager.AppSettings["ThrowExceptionOnRequestBindingPropertyNotFound"] == bool.TrueString;
            if (log.IsDebugEnabled || throwExceptionOnPropertyNotFound) {
                for (int i = 0; i < fieldCount; i++) {
                    if (propertyArray[i] == null || propertyArray[i].Count == 0) {
                        if (throwExceptionOnPropertyNotFound) {
                            throw new NotSupportedException("The entry '" + record.GetName(i) + "' doesn't match any property name for " + definition.BeanType.FullName);
                        }

                        if (log.IsDebugEnabled) {
                            log.Debug("The entry '" + record.GetName(i) + "' doesn't match any property name for " + definition.BeanType.FullName);
                        }
                    }
                }
            }

            _propertyArray = propertyArray;
        }

        /// <summary>
        /// Lit un record et écrit ces données dans un bean.
        /// </summary>
        /// <param name="destination">Bean à charger.</param>
        /// <param name="record">Enregistrement.</param>
        /// <returns>Bean.</returns>
        public T Read(object destination, IDataRecord record) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            T bean = destination == null ? new T() : (T)destination;
            for (int i = 0; i < _propertyArray.Length; i++) {
                List<PropertyDescriptor> fieldPath = _propertyArray[i];
                if (fieldPath == null || fieldPath.Count == 0) {
                    continue;
                }

                object value = record.GetValue(i);
                if (!DBNull.Value.Equals(value)) {
                    object beanToSet = bean;
                    for (int j = 0; j < fieldPath.Count - 1; ++j) {
                        beanToSet = fieldPath[j].GetValue(beanToSet);
                        if (beanToSet == null) {
                            string[] propArray = new string[fieldPath.Count];
                            for (int k = 0; k < fieldPath.Count; ++k) {
                                propArray[k] = fieldPath[k].Name;
                            }

                            throw new NotSupportedException("Impossible de lire la propriété " + string.Join(".", propArray));
                        }
                    }

                    try {
                        fieldPath[fieldPath.Count - 1].SetValue(beanToSet, value);
                    } catch (ArgumentException e) {
                        throw new NotSupportedException("Erreur sur la propriété " + fieldPath[fieldPath.Count - 1].ComponentType.Name + "." + fieldPath[fieldPath.Count - 1].Name, e);
                    }
                }
            }

            return bean;
        }
    }
}
