using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;

namespace Kinetix.Reporting {
    /// <summary>
    /// Gestion des données pour les éditions.
    /// </summary>
    /// <remarks>Pour réaliser des éditions grouper, il faut utiliser la méthode publique statique ReportDocument.GetXmlValue.</remarks>
    public sealed class ReportDocument : ICustomTypeDescriptor, IReportBean {

        /*
        <root>
            <document>
                <property name="...">...</property>
                <object name="...">
                    <property name="...">...</property>
                    <collection name="...">
                        <object name="...">
                            <property name="...">...</property>
                        </object>
                        <object name="...">
                            <property name="...">...</property>
                        </object>
                    </collection>
                </object>
                <collection name="...">
                    <object name="...">
                        <property name="...">...</property>
                    </object>
                    <object name="...">
                        <property name="...">...</property>
                    </object>
                </collection>
            </document>
            <document>
                ...
            </document>
        </root>
        */

        /// <summary>
        /// Identifiant du noeud racine.
        /// </summary>
        public const string XmlNodeRoot = "root";

        /// <summary>
        /// Identifiant du noeud document.
        /// </summary>
        public const string XmlNodeDocument = "document";

        /// <summary>
        /// Identifiant du noeud représentant un objet.
        /// </summary>
        public const string XmlNodeObject = "object";

        /// <summary>
        /// Identifiant du noeud représentant une collection.
        /// </summary>
        public const string XmlNodeCollection = "collection";

        /// <summary>
        /// Identifiant du noeud représentant une propriété.
        /// </summary>
        public const string XmlNodeProperty = "property";

        /// <summary>
        /// Identifiant de l'attribut name d'un noeud.
        /// </summary>
        public const string XmlNameAttribute = "name";

        private readonly Dictionary<string, object> _propertyMap = new Dictionary<string, object>();
        private readonly Dictionary<string, PropertyDescriptor> _properties = new Dictionary<string, PropertyDescriptor>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        public ReportDocument() {
        }

        /// <summary>
        /// Retourne la représentation Xml du document.
        /// </summary>
        public string XmlValue {
            get {
                List<ReportDocument> list = new List<ReportDocument>();
                list.Add(this);
                return ReportDocument.GetXml((ICollection)list);
            }
        }

        /// <summary>
        /// Retourne le nombre de propriété du document.
        /// </summary>
        public int PropertiesCount {
            get {
                return _propertyMap.Count;
            }
        }

        /// <summary>
        /// Crée une collection de document à partir d'une commande.
        /// </summary>
        /// <param name="command">Commande.</param>
        /// <returns>Collection de document.</returns>
        public static ICollection<ReportDocument> CreateCollection(SqlServerCommand command) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            List<ReportDocument> coll = new List<ReportDocument>();
            using (IDataReader reader = command.ExecuteReader()) {
                while (reader.Read()) {
                    ReportDocument document = new ReportDocument();
                    document.AddProperties(reader);
                    coll.Add(document);
                }
            }

            return coll;
        }

        /// <summary>
        /// Retourne la représentation Xml d'une collection de document.
        /// </summary>
        /// <param name="documentCollection">Collection de document.</param>
        /// <returns>Représentation Xml de la collection.</returns>
        public static string GetXml(IEnumerable documentCollection) {
            if (documentCollection == null) {
                throw new ArgumentNullException("documentCollection");
            }

            using (StringWriter sw = new StringWriter(CultureInfo.CurrentCulture)) {
                using (XmlTextWriter xmlWriter = new XmlTextWriter(sw)) {
                    xmlWriter.Formatting = Formatting.Indented;

                    xmlWriter.WriteStartElement(XmlNodeRoot);
                    foreach (object document in documentCollection) {
                        ReportDocument reportDoc = document as ReportDocument;
                        if (reportDoc != null && reportDoc._propertyMap.Count == 0) {
                            throw new ReportException("Les documents vides ne sont pas supportés.");
                        }

                        WriteDocument(xmlWriter, document);
                    }

                    xmlWriter.WriteEndElement();
                }

                return sw.ToString();
            }
        }

        /// <summary>
        /// Ajoute toutes les propriétés de l'objet au document.
        /// </summary>
        /// <param name="data">Objet.</param>
        public void AddProperties(object data) {
            BeanDefinition definition = BeanDescriptor.GetDefinition(data);
            foreach (BeanPropertyDescriptor property in definition.Properties) {
                object value = property.GetValue(data);
                if (value != null && !DBNull.Value.Equals(value)) {
                    this.Add(property.PropertyName, string.IsNullOrEmpty(property.DomainName) || (value.GetType() == typeof(bool)) || (value.GetType() == typeof(byte[])) ? value : property.ConvertToString(value));
                } else {
                    this.Add(property.PropertyName, null);
                }
            }
        }

        /// <summary>
        /// Ajoute tous les champs retournée par la commande comme propriété du document.
        /// </summary>
        /// <param name="command">Commande Sql.</param>
        public void AddProperties(SqlServerCommand command) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            using (IDataReader reader = command.ExecuteReader()) {
                if (reader.Read()) {
                    this.AddProperties(reader);
                }
            }
        }

        /// <summary>
        /// Ajoute tous les champs lus dans le reader comme propriété du document.
        /// </summary>
        /// <param name="record">Record Sql.</param>
        public void AddProperties(IDataRecord record) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            for (int i = 0; i < record.FieldCount; i++) {
                object value = record.GetValue(i);
                if (value != null && !DBNull.Value.Equals(value)) {
                    this.Add(record.GetName(i), value);
                } else {
                    this.Add(record.GetName(i), null);
                }
            }
        }

        /// <summary>
        /// Ajoute tous les champs retournée par la commande comme propriété du document.
        /// </summary>
        /// <param name="command">Commande Sql.</param>
        /// <param name="collectionName">Nom de la collection.</param>
        public void AddCollection(SqlServerCommand command, string collectionName) {
            if (command == null) {
                throw new ArgumentNullException("command");
            }

            using (IDataReader reader = command.ExecuteReader()) {
                List<ReportDocument> collection = new List<ReportDocument>();
                while (reader.Read()) {
                    ReportDocument document = new ReportDocument();
                    for (int i = 0; i < reader.FieldCount; i++) {
                        document.Add(reader.GetName(i), reader.GetValue(i));
                    }

                    collection.Add(document);
                }

                this.Add(collectionName, collection);
            }
        }

        /// <summary>
        /// Ajoute une propriété au document.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <param name="propertyValue">Valeur de la propriété.</param>
        public void Add(string propertyName, object propertyValue) {
            this.Add(propertyName, propertyValue, null);
        }

        /// <summary>
        /// Ajoute une propriété au document de type Boolean.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <param name="propertyValue">Valeur de la propriété.</param>
        public void AddBoolean(string propertyName, bool propertyValue) {
            this.Add(propertyName, propertyValue, null);
        }

        /// <summary>
        /// Ajoute une propriété au document.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <param name="propertyValue">Valeur de la propriété.</param>
        /// <param name="domainName">Nom du domaine.</param>
        public void Add(string propertyName, object propertyValue, string domainName) {
            if (string.IsNullOrEmpty(propertyName)) {
                throw new ArgumentNullException("propertyName");
            }

            if (_propertyMap.ContainsKey(propertyName)) {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Le document a déjà une propriété nommée '{0}'.", propertyName), "propertyName");
            }

            if (!string.IsNullOrEmpty(domainName)) {
                DomainAttribute domainAttr = new DomainAttribute(domainName);
                _properties[propertyName] = new ReportPropertyDescriptor(
                        propertyName,
                        (propertyValue == null) ? typeof(string) : propertyValue.GetType(),
                        this.GetType(),
                        new Attribute[] { domainAttr });
            } else {
                _properties[propertyName] = new ReportPropertyDescriptor(
                        propertyName,
                        (propertyValue == null) ? typeof(string) : propertyValue.GetType(),
                        this.GetType(),
                        new Attribute[] { });
            }

            _propertyMap.Add(propertyName, propertyValue);
        }

        /// <summary>
        /// Retourne la valeur d'une propriété.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété du composant.</param>
        /// <returns>Valeur de la propriété pour ce composant.</returns>
        public object GetValue(string propertyName) {
            return _propertyMap[propertyName];
        }

        /// <summary>
        /// Retourne la liste des attributs.
        /// </summary>
        /// <returns>Liste vide : pas d'attribut.</returns>
        AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return System.ComponentModel.AttributeCollection.Empty;
        }

        /// <summary>
        /// Retourne le nom de la classe.
        /// </summary>
        /// <returns>Null : null pas de nom de classe.</returns>
        string ICustomTypeDescriptor.GetClassName() {
            return this.GetType().Name;
        }

        /// <summary>
        /// Retourne le nom du composant.
        /// </summary>
        /// <returns>Nom du noeud Xml.</returns>
        string ICustomTypeDescriptor.GetComponentName() {
            return this.GetType().Name;
        }

        /// <summary>
        /// Retourne le formatter pour l'objet.
        /// </summary>
        /// <returns>Null : pas de formatter.</returns>
        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return null;
        }

        /// <summary>
        /// Retourne l'évènement par défaut.
        /// </summary>
        /// <returns>Null : pas d'évènement.</returns>
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return null;
        }

        /// <summary>
        /// Retourne la propriété par défaut.
        /// Non implémenté.
        /// </summary>
        /// <returns>Propriété par défaut.</returns>
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return null;
        }

        /// <summary>
        /// Retourne un éditeur pour le composant.
        /// </summary>
        /// <param name="editorBaseType">Type d'éditeur.</param>
        /// <returns>Null : pas d'éditeur.</returns>
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return null;
        }

        /// <summary>
        /// Retourne la liste des évènements.
        /// </summary>
        /// <param name="attributes">Attribut de filtrage.</param>
        /// <returns>Liste vide.</returns>
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
            return EventDescriptorCollection.Empty;
        }

        /// <summary>
        /// Retourne la liste des évènements.
        /// </summary>
        /// <returns>Retourne une liste vide.</returns>
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return EventDescriptorCollection.Empty;
        }

        /// <summary>
        /// Retourne la liste des propriétés du bean ayant un attribut.
        /// </summary>
        /// <param name="attributes">Attributs.</param>
        /// <returns>Propriétés.</returns>
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Retourne la liste des propriétés du bean.
        /// </summary>
        /// <returns>Propriétés du composant.</returns>
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            PropertyDescriptor[] array = new PropertyDescriptor[_properties.Count];
            _properties.Values.CopyTo(array, 0);
            return new PropertyDescriptorCollection(array);
        }

        /// <summary>
        /// Retourne le propriétaire de la propriété.
        /// </summary>
        /// <param name="propertyDescriptor">Property descriptor.</param>
        /// <returns>Le bean.</returns>
        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor propertyDescriptor) {
            return this;
        }

        /// <summary>
        /// Retourne la valeur d'une propriété.
        /// </summary>
        /// <param name="propertyDescriptor">Propriété du composant.</param>
        /// <returns>Valeur de la propriété pour ce composant.</returns>
        object IReportBean.GetValue(PropertyDescriptor propertyDescriptor) {
            return _propertyMap[propertyDescriptor.Name];
        }

        /// <summary>
        /// Définit la valeur d'une propriété.
        /// </summary>
        /// <param name="propertyDescriptor">PRopriété du composant.</param>
        /// <param name="value">Valeur à affecter à la propriété.</param>
        void IReportBean.SetValue(PropertyDescriptor propertyDescriptor, object value) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Ecrit la représentation Xml d'un document.
        /// </summary>
        /// <param name="xmlWriter">Writer XML.</param>
        /// <param name="document">Document.</param>
        private static void WriteDocument(XmlTextWriter xmlWriter, object document) {
            xmlWriter.WriteStartElement(XmlNodeDocument);
            if (document is IEnumerable) {
                throw new NotSupportedException("Les collections ne sont pas supportées ici.");
            } else {
                WriteProperties(xmlWriter, document);
            }

            xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Ecrit la représentation Xml d'un objet.
        /// </summary>
        /// <param name="xmlWriter">Writer XML.</param>
        /// <param name="data">Objet.</param>
        private static void WriteProperties(XmlTextWriter xmlWriter, object data) {
            BeanDefinition definition = BeanDescriptor.GetDefinition(data);
            foreach (BeanPropertyDescriptor property in definition.Properties) {
                WriteProperty(xmlWriter, property, property.GetValue(data));
            }
        }

        /// <summary>
        /// Ecrit la représentation Xml d'une propriété.
        /// </summary>
        /// <param name="xmlWriter">Writer XML.</param>
        /// <param name="property">Propriété.</param>
        /// <param name="propertyValue">Valeur de la propriété.</param>
        private static void WriteProperty(XmlTextWriter xmlWriter, BeanPropertyDescriptor property, object propertyValue) {
            ICollection collection = propertyValue as ICollection;
            if (property.PrimitiveType == typeof(byte[])) {
                xmlWriter.WriteStartElement(XmlNodeProperty);
                xmlWriter.WriteStartAttribute(XmlNameAttribute);
                xmlWriter.WriteName(property.PropertyName);
                xmlWriter.WriteEndAttribute();
                if (propertyValue != null) {
                    xmlWriter.WriteCData(Convert.ToBase64String((byte[])propertyValue));
                } else {
                    xmlWriter.WriteCData(string.Empty);
                }

                xmlWriter.WriteEndElement();
            } else if (collection != null) {

                xmlWriter.WriteStartElement(XmlNodeCollection);
                xmlWriter.WriteStartAttribute(XmlNameAttribute);
                xmlWriter.WriteName(property.PropertyName);
                xmlWriter.WriteEndAttribute();
                foreach (object data in collection) {
                    WriteProperty(xmlWriter, property, data);
                }

                xmlWriter.WriteEndElement();
            } else if (property.PrimitiveType == null) {
                xmlWriter.WriteStartElement(XmlNodeObject);
                xmlWriter.WriteStartAttribute(XmlNameAttribute);
                xmlWriter.WriteName(property.PropertyName);
                xmlWriter.WriteEndAttribute();

                WriteProperties(xmlWriter, propertyValue);

                xmlWriter.WriteEndElement();
            } else {
                xmlWriter.WriteStartElement(XmlNodeProperty);
                xmlWriter.WriteStartAttribute(XmlNameAttribute);
                xmlWriter.WriteName(property.PropertyName);
                xmlWriter.WriteEndAttribute();

                if (propertyValue != null) {
                    if (propertyValue.GetType() == typeof(ExtendedValue)) {
                        xmlWriter.WriteCData(property.ConvertToString(propertyValue));
                    } else if (propertyValue.GetType() == typeof(DateTime)) {
                        if (property.DomainName == "DO_JOUR_MOIS") {
                            xmlWriter.WriteCData(((DateTime)propertyValue).ToString("dd/MM", DateTimeFormatInfo.CurrentInfo));
                        } else {
                            xmlWriter.WriteCData(property.ConvertToString(propertyValue));
                        }
                    } else if (propertyValue.GetType() == typeof(bool)) {
                        xmlWriter.WriteCData(Convert.ToString(propertyValue, CultureInfo.CurrentCulture));
                    } else {
                        xmlWriter.WriteCData(property.ConvertToString(propertyValue));
                    }
                }

                xmlWriter.WriteEndElement();
            }
        }
    }
}
