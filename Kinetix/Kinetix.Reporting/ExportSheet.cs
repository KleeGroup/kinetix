using System;
using System.Collections;
using System.Collections.Generic;
using Kinetix.ComponentModel;

namespace Kinetix.Reporting {

    /// <summary>
    /// Feuille d'export.
    /// </summary>
    public class ExportSheet {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="name">Nom de la feuille de données.</param>
        /// <param name="dataSource">Source de données de la feuille.</param>
        /// <param name="properties">Propriétés affichées.</param>
        /// <exception cref="System.ArgumentException">Si une des propriétés à afficher n'existe pas dans le bean.</exception>
        public ExportSheet(string name, object dataSource, ICollection<ExportPropertyDefinition> properties) {
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name");
            }

            if (dataSource == null) {
                throw new ArgumentNullException("dataSource");
            }

            if (properties == null) {
                throw new ArgumentNullException("properties");
            }

            this.DisplayedProperties = new List<ExportPropertyDefinition>();
            this.Name = name;
            this.DataSource = dataSource;
            this.Orientation = ExportOrientation.Portrait;

            BeanDefinition definition = this.IsCollection ? BeanDescriptor.GetCollectionDefinition(this.DataSource) : BeanDescriptor.GetDefinition(this.DataSource);

            foreach (ExportPropertyDefinition property in properties) {
                if (property.PropertyPath.IndexOf('.') != -1) {
                    string[] propertyTab = property.PropertyPath.Split('.');
                    BeanPropertyDescriptor composedProperty = definition.Properties[propertyTab[0]];
                    BeanDefinition composedBeanDefinition = BeanDescriptor.GetDefinition(composedProperty.PropertyType);
                    if (!composedBeanDefinition.Properties.Contains(propertyTab[1])) {
                        throw new ArgumentException("Unable to find the property " + property.PropertyPath + " in type " + definition.BeanType.FullName);
                    }
                } else if (!definition.Properties.Contains(property.PropertyPath)) {
                    throw new ArgumentException("Unable to find the property " + property.PropertyPath + " in type " + definition.BeanType.FullName);
                }

                DisplayedProperties.Add(property);
            }
        }

        /// <summary>
        /// Nom de la feuille.
        /// </summary>
        public string Name {
            get;
            private set;
        }

        /// <summary>
        /// Source de données de la feuille.
        /// </summary>
        public object DataSource {
            get;
            private set;
        }

        /// <summary>
        /// Liste des nom de propriétés affichées.
        /// </summary>
        public IList<ExportPropertyDefinition> DisplayedProperties {
            get;
            private set;
        }

        /// <summary>
        /// Orientation de l'export.
        /// </summary>
        public ExportOrientation Orientation {
            get;
            set;
        }

        /// <summary>
        /// Retourne si la source de données est une collection.
        /// </summary>
        internal bool IsCollection {
            get {
                return this.DataSource is ICollection;
            }
        }
    }
}
