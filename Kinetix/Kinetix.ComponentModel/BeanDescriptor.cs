using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Fournit la description d'un bean.
    /// </summary>
    public sealed class BeanDescriptor {

        /// <summary>
        /// Nom par défaut de la propriété par défaut d'un bean, pour l'affichage du libellé de l'objet.
        /// </summary>
        private const string DefaultPropertyDefaultName = "Libelle";

        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _resourceTypeMap = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        private static BeanDescriptor _instance;

        private readonly Dictionary<Type, BeanDefinition> _beanDefinitionDictionnary;
        private readonly Dictionary<Type, IMetadataTypeProvider> _metadataProviders;

        /// <summary>
        /// Crée un nouvelle instance.
        /// </summary>
        private BeanDescriptor() {
            _beanDefinitionDictionnary = new Dictionary<Type, BeanDefinition>();
            _metadataProviders = new Dictionary<Type, IMetadataTypeProvider>();
        }

        /// <summary>
        /// Retourne une instance unique.
        /// </summary>
        private static BeanDescriptor Instance {
            get { return _instance ?? (_instance = new BeanDescriptor()); }
        }

        /// <summary>
        /// Vérifie les contraintes sur un bean.
        /// </summary>
        /// <param name="bean">Bean à vérifier.</param>
        /// <param name="allowPrimaryKeyNull">True si la clef primaire peut être null (insertion).</param>
        public static void Check(object bean, bool allowPrimaryKeyNull = true) {
            if (bean != null) {
                try {
                    GetDefinition(bean).Check(bean, allowPrimaryKeyNull);
                } catch (ConstraintException e) {
                    throw new ConstraintException(e.Property, e.Property.Description + " " + e.Message, e);
                }
            }
        }

        /// <summary>
        /// Vérifie les contraintes sur les éléments contenus dans une collection.
        /// </summary>
        /// <param name="collection">Collection à vérifier.</param>
        /// <param name="allowPrimaryKeyNull">True si la clef primaire peut être null (insertion).</param>
        /// <typeparam name="T">Type des éléments de la collection.</typeparam>
        public static void CheckAll<T>(ICollection<T> collection, bool allowPrimaryKeyNull = true) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            foreach (T obj in collection) {
                Check(obj, allowPrimaryKeyNull);
            }
        }

        /// <summary>
        /// Retourne la definition des beans d'une collection générique.
        /// </summary>
        /// <param name="collection">Collection générique de bean.</param>
        /// <returns>Description des propriétés des beans.</returns>
        public static BeanDefinition GetCollectionDefinition(object collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            Type collectionType = collection.GetType();
            if (collectionType.IsArray) {
                return GetDefinition(collectionType.GetElementType());
            }

            if (!collectionType.IsGenericType) {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionTypeDescription,
                        collection.GetType().FullName),
                    "collection");
            }

            Type genericDefinition = collectionType.GetGenericTypeDefinition();
            if (genericDefinition.GetInterface(typeof(ICollection<>).Name) == null) {
                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionNotSupportedGeneric,
                        genericDefinition.Name));
            }

            Type objectType = collectionType.GetGenericArguments()[0];
            ICollection coll = (ICollection)collection;
            if (typeof(ICustomTypeDescriptor).IsAssignableFrom(objectType) && coll.Count != 0) {
                object customObject = coll.Cast<object>().FirstOrDefault();
                return GetDefinition(customObject);
            }

            foreach (object obj in coll) {
                objectType = obj.GetType();
                break;
            }

            return GetDefinition(objectType, true);
        }

        /// <summary>
        /// Retourne la definition d'un bean.
        /// </summary>
        /// <param name="bean">Objet.</param>
        /// <returns>Description des propriétés.</returns>
        public static BeanDefinition GetDefinition(object bean) {
            if (bean == null) {
                throw new ArgumentNullException("bean");
            }

            return Instance.GetDefinitionInternal(bean.GetType(), Instance.GetMetadataType(bean.GetType(), bean), bean);
        }

        /// <summary>
        /// Retourne la definition d'un bean.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <param name="ignoreCustomTypeDesc">Si true, retourne un définition même si le type implémente ICustomTypeDescriptor.</param>
        /// <returns>Description des propriétés.</returns>
        public static BeanDefinition GetDefinition(Type beanType, bool ignoreCustomTypeDesc = false) {
            if (beanType == null) {
                throw new ArgumentNullException("beanType");
            }

            if (!ignoreCustomTypeDesc && typeof(ICustomTypeDescriptor).IsAssignableFrom(beanType)) {
                throw new NotSupportedException(SR.ExceptionICustomTypeDescriptorNotSupported);
            }

            return Instance.GetDefinitionInternal(beanType, Instance.GetMetadataType(beanType), null);
        }

        /// <summary>
        /// Efface la définition d'un bean du singleton.
        /// </summary>
        /// <param name="descriptionType">Type portant la description.</param>
        public static void ClearDefinition(Type descriptionType) {
            Instance.ClearDefinitionCore(descriptionType);
        }

        /// <summary>
        /// Crée la collection des descripteurs de propriétés.
        /// </summary>
        /// <param name="properties">PropertyDescriptors.</param>
        /// <param name="defaultProperty">Propriété par défaut.</param>
        /// <param name="beanType">Type du bean.</param>
        /// <param name="metadataProperties">Métadonnées.</param>
        /// <returns>Collection.</returns>
        private static BeanPropertyDescriptorCollection CreateCollection(PropertyDescriptorCollection properties, PropertyDescriptor defaultProperty, Type beanType, PropertyDescriptorCollection metadataProperties) {
            BeanPropertyDescriptorCollection coll = new BeanPropertyDescriptorCollection(beanType);
            for (int i = 0; i < properties.Count; i++) {
                PropertyDescriptor property = properties[i];

                KeyAttribute keyAttr = (KeyAttribute)property.Attributes[typeof(KeyAttribute)];
                DisplayAttribute displayAttr = (DisplayAttribute)property.Attributes[typeof(DisplayAttribute)];
                ReferencedTypeAttribute attr = (ReferencedTypeAttribute)property.Attributes[typeof(ReferencedTypeAttribute)];
                ColumnAttribute colAttr = (ColumnAttribute)property.Attributes[typeof(ColumnAttribute)];
                DomainAttribute domainAttr = (DomainAttribute)property.Attributes[typeof(DomainAttribute)];
                RequiredAttribute requiredAttr = (RequiredAttribute)property.Attributes[typeof(RequiredAttribute)];
                TranslatableAttribute translatableAttr = (TranslatableAttribute)property.Attributes[typeof(TranslatableAttribute)];
                Type[] genericArgumentArray = beanType.GetGenericArguments();

                string display = null;
                if (displayAttr != null) {
                    if (displayAttr.ResourceType != null && displayAttr.Name != null) {
                        Dictionary<string, PropertyInfo> resourceProperties;
                        if (!_resourceTypeMap.TryGetValue(displayAttr.ResourceType, out resourceProperties)) {
                            resourceProperties = new Dictionary<string, PropertyInfo>();
                            _resourceTypeMap[displayAttr.ResourceType] = resourceProperties;

                            foreach (PropertyInfo p in displayAttr.ResourceType.GetProperties(BindingFlags.Public | BindingFlags.Static)) {
                                resourceProperties.Add(p.Name, p);
                            }
                        }

                        display = resourceProperties[displayAttr.Name].GetValue(null, null).ToString();
                    } else {
                        display = displayAttr.Name;
                    }
                }

                string memberName = (colAttr == null) ? null : colAttr.Name;
                bool isPrimaryKey = keyAttr != null;
                bool isRequired = requiredAttr != null;
                bool isTranslatable = translatableAttr != null;
                string domainName = (domainAttr == null) ? null : domainAttr.Name;
                bool isDefault = property.Equals(defaultProperty) || (DefaultPropertyDefaultName.Equals(property.Name) && defaultProperty == null);
                Type referenceType = attr == null ? null : attr.ReferenceType;
                //// Type dtoType = genericArgumentArray.Length > 0 ? genericArgumentArray[0] : beanType;
                bool isBrowsable = property.IsBrowsable;
                bool isReadonly = property.IsReadOnly;

                // Traitement des métadonnées.
                if (metadataProperties != null) {
                    PropertyDescriptor metadata = metadataProperties[property.Name];
                    if (metadata != null) {
                        if (!metadata.IsBrowsable) {
                            isBrowsable = false;
                        }

                        if (metadata.Attributes[typeof(RequiredAttribute)] != null) {
                            isRequired = true;
                        }

                        if (metadata.Attributes[typeof(TranslatableAttribute)] != null) {
                            isTranslatable = true;
                        }

                        ReadOnlyAttribute readonlyAttr = (ReadOnlyAttribute)metadata.Attributes[typeof(ReadOnlyAttribute)];
                        if (readonlyAttr != null && readonlyAttr.IsReadOnly) {
                            isReadonly = true;
                        }
                    }
                }

                BeanPropertyDescriptor description = new BeanPropertyDescriptor(
                            property.Name,
                            memberName,
                            property.PropertyType,
                            display,
                            domainName,
                            isPrimaryKey,
                            isDefault,
                            isRequired,
                            referenceType,
                            isReadonly,
                            isBrowsable,
                            isTranslatable);
                if (domainName != null) {
                    DomainManager.Instance.GetDomain(description);
                }

                coll.Add(description);
            }

            return coll;
        }

        /// <summary>
        /// Retourne la description des propriétés d'un objet sous forme d'une collection.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <param name="metadataType">Type portant les compléments de description.</param>
        /// <param name="bean">Bean dynamic.</param>
        /// <returns>Description des propriétés.</returns>
        private static BeanPropertyDescriptorCollection CreateBeanPropertyCollection(Type beanType, Type metadataType, object bean) {
            PropertyDescriptor defaultProperty;
            PropertyDescriptorCollection properties;
            PropertyDescriptorCollection metadataProperties = null;

            if (bean != null && bean is ICustomTypeDescriptor) {
                properties = TypeDescriptor.GetProperties(bean);
                defaultProperty = TypeDescriptor.GetDefaultProperty(bean);
            } else {
                properties = TypeDescriptor.GetProperties(beanType);
                defaultProperty = TypeDescriptor.GetDefaultProperty(beanType);

                if (metadataType != null) {
                    metadataProperties = TypeDescriptor.GetProperties(metadataType);
                }
            }

            return CreateCollection(properties, defaultProperty, beanType, metadataProperties);
        }

        /// <summary>
        /// Retourne la definition d'un bean.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <param name="metadataType">Type portant les compléments de description.</param>
        /// <param name="bean">Bean.</param>
        /// <returns>Description des propriétés.</returns>
        private BeanDefinition GetDefinitionInternal(Type beanType, Type metadataType, object bean) {

            Type descriptionType = metadataType;
            if (descriptionType == null) {
                descriptionType = beanType;
            }

            BeanDefinition definition;
            if (!_beanDefinitionDictionnary.TryGetValue(descriptionType, out definition)) {
                TableAttribute table = (TableAttribute)TypeDescriptor.GetAttributes(beanType)[typeof(TableAttribute)];
                string contractName = (table == null) ? null : table.Name;

                object[] attrs = beanType.GetCustomAttributes(typeof(ReferenceAttribute), false);
                bool isReference = attrs.Length == 1;
                bool isStatic = isReference ? ((ReferenceAttribute)attrs[0]).IsStatic : false;
                BeanPropertyDescriptorCollection properties = CreateBeanPropertyCollection(beanType, metadataType, bean);

                definition = new BeanDefinition(beanType, properties, contractName, isReference, isStatic);
                if (bean == null && !typeof(ICustomTypeDescriptor).IsAssignableFrom(beanType)) {
                    _beanDefinitionDictionnary[descriptionType] = definition;
                }
            }

            return definition;
        }

        /// <summary>
        /// Efface la description d'un type.
        /// </summary>
        /// <param name="descriptionType">Type portant la description.</param>
        private void ClearDefinitionCore(Type descriptionType) {
            _beanDefinitionDictionnary.Remove(descriptionType);
        }

        /// <summary>
        /// Retourne le type de la classe portant les métadonnées.
        /// Se base sur l'attribut standard MetdataTypeAttribute.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <returns>Le type.</returns>
        private Type GetMetadataType(Type beanType) {
            object[] metadataType = beanType.GetCustomAttributes(typeof(MetadataTypeAttribute), true);
            if (metadataType.Length == 1) {
                return ((MetadataTypeAttribute)metadataType[0]).MetadataClassType;
            }

            return null;
        }

        /// <summary>
        /// Retourne le type de la classe portant les métadonnées.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <param name="bean">Bean.</param>
        /// <returns>Classe de métadonnées ou null.</returns>
        private Type GetMetadataType(Type beanType, object bean) {
            Type meta = GetMetadataType(beanType);
            if (meta != null) {
                return meta;
            }

            if (bean == null) {
                return null;
            }

            object[] metadataProvider = beanType.GetCustomAttributes(typeof(MetadataTypeProviderAttribute), true);
            if (metadataProvider.Length == 0) {
                return null;
            }

            Type providerType = ((MetadataTypeProviderAttribute)metadataProvider[0]).ProviderType;

            IMetadataTypeProvider provider;
            if (!_metadataProviders.TryGetValue(providerType, out provider)) {
                provider = (IMetadataTypeProvider)Activator.CreateInstance(providerType);
                _metadataProviders[providerType] = provider;
            }

            return provider.GetMetadataType(bean);
        }
    }
}
