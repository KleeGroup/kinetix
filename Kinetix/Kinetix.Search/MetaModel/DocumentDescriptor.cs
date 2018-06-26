using System;
using System.Collections.Generic;
using System.ComponentModel;
using Kinetix.Search.ComponentModel;

namespace Kinetix.Search.MetaModel {

    /// <summary>
    /// Fournit la description d'un document.
    /// </summary>
    public sealed class DocumentDescriptor {

        private static DocumentDescriptor _instance;

        private readonly Dictionary<Type, DocumentDefinition> _beanDefinitionDictionnary;

        /// <summary>
        /// Crée un nouvelle instance.
        /// </summary>
        private DocumentDescriptor() {
            _beanDefinitionDictionnary = new Dictionary<Type, DocumentDefinition>();
        }

        /// <summary>
        /// Retourne une instance unique.
        /// </summary>
        private static DocumentDescriptor Instance {
            get { return _instance ?? (_instance = new DocumentDescriptor()); }
        }

        /// <summary>
        /// Retourne la definition d'un document.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <returns>Description des propriétés.</returns>
        public static DocumentDefinition GetDefinition(Type beanType) {
            if (beanType == null) {
                throw new ArgumentNullException("beanType");
            }

            return Instance.GetDefinitionInternal(beanType);
        }

        /// <summary>
        /// Crée la collection des descripteurs de propriétés.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <returns>Collection.</returns>
        private static DocumentFieldDescriptorCollection CreateCollection(Type beanType) {
            DocumentFieldDescriptorCollection coll = new DocumentFieldDescriptorCollection(beanType);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(beanType);

            foreach (PropertyDescriptor property in properties) {

                SearchFieldAttribute fieldAttr = (SearchFieldAttribute)property.Attributes[typeof(SearchFieldAttribute)];
                if (fieldAttr == null) {
                    throw new NotSupportedException("Missing SearchFieldAttribute on property " + beanType + "." + property.Name);
                }

                DocumentFieldAttribute docAttr = (DocumentFieldAttribute)property.Attributes[typeof(DocumentFieldAttribute)];

                var fieldCategory = fieldAttr.Category;

                string fieldName = ToCamelCase(property.Name);
                DocumentFieldDescriptor description = new DocumentFieldDescriptor(
                            property.Name,
                            fieldName,
                            property.PropertyType,
                            docAttr?.Category,
                            fieldCategory);

                coll[description.PropertyName] = description;
            }

            return coll;
        }

        /// <summary>
        /// Convertit une chaîne en camelCase.
        /// </summary>
        /// <param name="raw">Chaîne source.</param>
        /// <returns>Chaîne en camelCase.</returns>
        private static string ToCamelCase(string raw) {
            if (string.IsNullOrEmpty(raw)) {
                return raw;
            }

            return char.ToLower(raw[0]) + raw.Substring(1);
        }

        /// <summary>
        /// Retourne la definition d'un bean.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <returns>Description des propriétés.</returns>
        private DocumentDefinition GetDefinitionInternal(Type beanType) {
            DocumentDefinition definition;
            if (!_beanDefinitionDictionnary.TryGetValue(beanType, out definition)) {
                SearchDocumentTypeAttribute documentType = (SearchDocumentTypeAttribute)TypeDescriptor.GetAttributes(beanType)[typeof(SearchDocumentTypeAttribute)];
                if (documentType == null) {
                    throw new NotSupportedException("Missing SearchDocumentTypeAttribute on type " + beanType);
                }

                string documentTypeName = documentType.DocumentTypeName;

                DocumentFieldDescriptorCollection properties = CreateCollection(beanType);
                definition = new DocumentDefinition(beanType, properties, documentTypeName);
                _beanDefinitionDictionnary[beanType] = definition;
            }

            return definition;
        }
    }
}
