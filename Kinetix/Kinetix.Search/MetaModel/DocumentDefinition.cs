using System;
using Kinetix.Search.ComponentModel;

namespace Kinetix.Search.MetaModel {

    /// <summary>
    /// Définition d'un bean.
    /// </summary>
    [Serializable]
    public class DocumentDefinition {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="beanType">Type du bean.</param>
        /// <param name="properties">Collection de propriétés.</param>
        /// <param name="documentTypeName">Nom du contrat (table).</param>
        internal DocumentDefinition(Type beanType, DocumentFieldDescriptorCollection properties, string documentTypeName) {
            this.BeanType = beanType;
            this.Fields = properties;
            this.DocumentTypeName = documentTypeName;
            foreach (DocumentFieldDescriptor property in properties) {
                switch (property.Category) {
                    case SearchFieldCategory.Id:
                        this.PrimaryKey = property;
                        break;
                    case SearchFieldCategory.Search:
                        this.TextField = property;
                        break;
                    case SearchFieldCategory.Security:
                        this.SecurityField = property;
                        break;
                    default:
                        break;
                }
            }

            if (this.PrimaryKey == null) {
                throw new NotSupportedException(beanType + " has no primary key defined.");
            }
        }

        /// <summary>
        /// Retourne le type du bean.
        /// </summary>
        public Type BeanType {
            get;
            private set;
        }

        /// <summary>
        /// Retourne le nom du contrat.
        /// </summary>
        public string DocumentTypeName {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la clef primaire si elle existe.
        /// </summary>
        public DocumentFieldDescriptor PrimaryKey {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la propriété de recherche textuelle.
        /// </summary>
        public DocumentFieldDescriptor TextField {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la propriété de filtrage de sécurité.
        /// </summary>
        public DocumentFieldDescriptor SecurityField {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la liste des propriétés d'un bean.
        /// </summary>
        public DocumentFieldDescriptorCollection Fields {
            get;
            private set;
        }
    }
}
