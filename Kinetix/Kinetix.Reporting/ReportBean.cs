using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

namespace Kinetix.Reporting {
    /// <summary>
    /// Arbre d'objets représentant les données de l'édition.
    /// </summary>
    public class ReportBean : ICustomTypeDescriptor, IReportBean {
        private readonly Hashtable _valueTable = new Hashtable(); // valeurs des propriétés
        private readonly PropertyDescriptorCollection _propertyDescriptors; // propriétés du composant

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="name">Nom de l'arbre.</param>
        /// <param name="absoluteName">Nom absolu du bean dans l'arbre XML.</param>
        /// <param name="reader">Reader sur la racine de l'arbre XML correspondant aux données.</param>
        public ReportBean(string name, string absoluteName, XmlTextReader reader) {
            if (reader == null) {
                throw new ArgumentNullException("reader");
            }

            if (!reader.Read()) {
                return;
            }

            this.Name = name;
            this.AbsoluteName = absoluteName;
            List<PropertyDescriptor> propertyList = new List<PropertyDescriptor>();

            while (reader.NodeType != XmlNodeType.EndElement) {
                CheckElement(reader);
                string childName = reader.GetAttribute(ReportDocument.XmlNameAttribute);
                string type = reader.Name;
                if (ReportDocument.XmlNodeCollection.Equals(type)) {
                    if (reader.IsEmptyElement) {
                        _valueTable.Add(childName, null);
                        reader.Read();
                    } else {
                        _valueTable.Add(childName, CreateCollection(childName, reader));
                    }

                    propertyList.Add(new ReportPropertyDescriptor(childName, typeof(ICollection<ICustomTypeDescriptor>), this.GetType(), null));
                } else if (ReportDocument.XmlNodeObject.Equals(type)) {
                    if (reader.IsEmptyElement) {
                        _valueTable.Add(childName, null);
                    } else {
                        _valueTable.Add(childName, new ReportBean(childName, this.AbsoluteName + "." + childName, reader));
                    }

                    propertyList.Add(new ReportPropertyDescriptor(childName, typeof(ICustomTypeDescriptor), this.GetType(), null));
                    reader.Read();
                } else if (ReportDocument.XmlNodeProperty.Equals(type)) {
                    propertyList.Add(new ReportPropertyDescriptor(childName, typeof(string), this.GetType(), null));
                    reader.Read();
                    if (XmlNodeType.CDATA.Equals(reader.NodeType)) {
                        // Si la valeur existe, elle est ajoutée
                        _valueTable.Add(childName, reader.Value);
                        reader.Read(); // Fin CDATA
                        reader.Read(); // Fin Property Element
                    }
                } else if (ReportDocument.XmlNodeDocument.Equals(type)) {
                    _valueTable.Add(type, new ReportBean(type, this.AbsoluteName + "." + type, reader));
                    propertyList.Add(new ReportPropertyDescriptor(type, typeof(ICustomTypeDescriptor), this.GetType(), null));
                } else {
                    throw new NotSupportedException(this.AbsoluteName + ": " + type);
                }
            }

            PropertyDescriptor[] propertyArray = new PropertyDescriptor[propertyList.Count];
            propertyList.CopyTo(propertyArray);
            _propertyDescriptors = new PropertyDescriptorCollection(propertyArray);
        }

        /// <summary>
        /// Retourne le nom du bean.
        /// </summary>
        public string Name {
            get;
            private set;
        }

        /// <summary>
        /// Retourne le nom absolu du bean.
        /// </summary>
        public string AbsoluteName {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la liste des attributs.
        /// </summary>
        /// <returns>Liste vide : pas d'attribut.</returns>
        public AttributeCollection GetAttributes() {
            return AttributeCollection.Empty;
        }

        /// <summary>
        /// Retourne le nom de la classe.
        /// </summary>
        /// <returns>Null : null pas de nom de classe.</returns>
        public string GetClassName() {
            return null;
        }

        /// <summary>
        /// Retourne le nom du composant.
        /// </summary>
        /// <returns>Nom du noeud Xml.</returns>
        public string GetComponentName() {
            return this.Name;
        }

        /// <summary>
        /// Retourne le formatter pour l'objet.
        /// </summary>
        /// <returns>Null : pas de formatter.</returns>
        public TypeConverter GetConverter() {
            return null;
        }

        /// <summary>
        /// Retourne l'évènement par défaut.
        /// </summary>
        /// <returns>Null : pas d'évènement.</returns>
        public EventDescriptor GetDefaultEvent() {
            return null;
        }

        /// <summary>
        /// Retourne la propriété par défaut.
        /// Non implémenté.
        /// </summary>
        /// <returns>Propriété par défaut.</returns>
        public PropertyDescriptor GetDefaultProperty() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne un éditeur pour le composant.
        /// </summary>
        /// <param name="editorBaseType">Type d'éditeur.</param>
        /// <returns>Null : pas d'éditeur.</returns>
        public object GetEditor(Type editorBaseType) {
            return null;
        }

        /// <summary>
        /// Retourne la liste des évènements.
        /// </summary>
        /// <param name="attributes">Attribut de filtrage.</param>
        /// <returns>Liste vide.</returns>
        public EventDescriptorCollection GetEvents(Attribute[] attributes) {
            return EventDescriptorCollection.Empty;
        }

        /// <summary>
        /// Retourne la liste des évènements.
        /// </summary>
        /// <returns>Retourne une liste vide.</returns>
        public EventDescriptorCollection GetEvents() {
            return EventDescriptorCollection.Empty;
        }

        /// <summary>
        /// Retourne la liste des propriétés du bean ayant un attribut.
        /// </summary>
        /// <param name="attributes">Attributs.</param>
        /// <returns>Propriétés.</returns>
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne la liste des propriétés du bean.
        /// </summary>
        /// <returns>Propriétés du composant.</returns>
        public PropertyDescriptorCollection GetProperties() {
            return _propertyDescriptors;
        }

        /// <summary>
        /// Retourne le propriétaire de la propriété.
        /// </summary>
        /// <param name="pd">Property descriptor.</param>
        /// <returns>Le bean.</returns>
        public object GetPropertyOwner(PropertyDescriptor pd) {
            return this;
        }

        /// <summary>
        /// Retourne la valeur d'une propriété.
        /// </summary>
        /// <param name="propertyDescriptor">Propriété du composant.</param>
        /// <returns>Valeur de la propriété pour ce composant.</returns>
        object IReportBean.GetValue(PropertyDescriptor propertyDescriptor) {
            if (_valueTable.ContainsKey(propertyDescriptor.Name)) {
                return _valueTable[propertyDescriptor.Name];
            }

            return null;
        }

        /// <summary>
        /// Définit la valeur d'une propriété.
        /// </summary>
        /// <param name="propertyDescriptor">PRopriété du composant.</param>
        /// <param name="value">Valeur à affecter à la propriété.</param>
        void IReportBean.SetValue(PropertyDescriptor propertyDescriptor, object value) {
            _valueTable[propertyDescriptor.Name] = value;
        }

        /// <summary>
        /// Crée une collection de composant sous ce noeud de l'arbre.
        /// </summary>
        /// <param name="collName">Nom de la collection.</param>
        /// <param name="reader">Début du XML représentant la collection.</param>
        /// <returns>Description de la collection créée.</returns>
        private ICollection<ICustomTypeDescriptor> CreateCollection(string collName, XmlTextReader reader) {
            List<ICustomTypeDescriptor> list = new List<ICustomTypeDescriptor>();
            if (!reader.Read()) {
                return list;
            }

            int index = 0;
            while (reader.NodeType != XmlNodeType.EndElement) {
                CheckElement(reader);
                string type = reader.Name;
                string childName = reader.GetAttribute(ReportDocument.XmlNameAttribute);
                if (ReportDocument.XmlNodeObject.Equals(type)) {
                    list.Add(new ReportBean(childName, this.AbsoluteName + "." + collName + "[" + index + "]", reader));
                    index++;
                    reader.Read();
                } else {
                    throw new NotSupportedException(this.AbsoluteName + ": " + type);
                }
            }

            reader.Read();
            return list;
        }

        /// <summary>
        /// Vérifie que le noeud XML en cours est bien un élément.
        /// </summary>
        /// <param name="reader">Noeud XML courant.</param>
        private void CheckElement(XmlTextReader reader) {
            if (reader.NodeType != XmlNodeType.Element) {
                throw new NotSupportedException(this.AbsoluteName + ": " + reader.NodeType.ToString());
            }
        }
    }
}
