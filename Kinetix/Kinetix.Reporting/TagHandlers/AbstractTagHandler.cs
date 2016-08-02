using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Kinetix.ComponentModel;
using log4net;

namespace Kinetix.Reporting.TagHandlers {

    /// <summary>
    /// Classe abstraite de handler de tag OpenXML "Custom".
    /// </summary>
    internal abstract class AbstractTagHandler : ITagHandler {

#pragma warning disable SA1401

        /// <summary>
        /// Id document OpenXml en cours de génération.
        /// </summary>
        protected readonly Guid DocumentId;

        /// <summary>
        /// Id document OpenXml en cours de génération.
        /// </summary>
        protected readonly bool IsXmlData;

#pragma warning restore SA1401

        /// <summary>
        /// Logger d'inteprétation des tags.
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger("Tags");

        /// <summary>
        /// Dictionnaire des attributs de tags.
        /// </summary>
        private readonly Dictionary<string, string> _tagAttributes = new Dictionary<string, string>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="currentPart">OpenXmlPart courant.</param>
        /// <param name="currentXmlElement">Element représentant le tag.</param>
        /// <param name="currentDataSource">Source de données courante.</param>
        /// <param name="documentId">Id document en cours.</param>
        /// <param name="isXmlData">Si la source en xml.</param>
        public AbstractTagHandler(OpenXmlPart currentPart, CustomXmlElement currentXmlElement, object currentDataSource, Guid documentId, bool isXmlData) {
            if (currentPart == null) {
                throw new ArgumentNullException("currentPart");
            }

            if (currentXmlElement == null) {
                throw new ArgumentNullException("currentXmlElement");
            }

            if (documentId == null) {
                throw new ArgumentNullException("documentId");
            }

            if (currentDataSource == null) {
                throw new ArgumentNullException("currentDataSource");
            }

            if (string.IsNullOrEmpty(currentXmlElement.Element.Value)) {
                throw new NotSupportedException("The CustomXmlElement tag doesn't have a value for the element property.");
            }

            DocumentId = documentId;
            this.IsXmlData = isXmlData;

            this.CurrentPart = currentPart;
            this.TagName = currentXmlElement.Element.Value;
            this.CurrentElement = currentXmlElement;
            this.DataSource = currentDataSource;
            if (currentXmlElement.CustomXmlProperties != null) {
                foreach (CustomXmlAttribute tagAttribute in currentXmlElement.CustomXmlProperties.Elements<CustomXmlAttribute>()) {
                    _tagAttributes.Add(tagAttribute.Name.Value, tagAttribute.Val.Value);
                }
            }
        }

        /// <summary>
        /// Nom du tag.
        /// </summary>
        public string TagName {
            get;
            private set;
        }

        /// <summary>
        /// OpenXmlPart courant du tag.
        /// </summary>
        protected OpenXmlPart CurrentPart {
            get;
            private set;
        }

        /// <summary>
        /// CustomXmlElement courant.
        /// </summary>
        protected CustomXmlElement CurrentElement {
            get;
            private set;
        }

        /// <summary>
        /// Source de données liée au tag.
        /// </summary>
        protected object DataSource {
            get;
            private set;
        }

        /// <summary>
        /// Clef d'accès à une propriété.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <returns>Valeur de la propriété.</returns>
        public string this[string propertyName] {
            get {
                if (string.IsNullOrEmpty(propertyName)) {
                    throw new ArgumentNullException("propertyName");
                }

                if (!_tagAttributes.ContainsKey(propertyName)) {
                    if (propertyName != "expression" && propertyName != "orcondition" && propertyName != "orexpression" && propertyName != "precision" && propertyName != "nbreNewLine") {
                        throw new KeyNotFoundException("The tag " + this.TagName + " has no attribute named " + propertyName);
                    } else if (propertyName == "precision") {
                        return ConfigurationManager.AppSettings["DefaultPrecision"];
                    } else if (propertyName == "nbreNewLine") {
                        return ConfigurationManager.AppSettings["DefaultNbreNewLine"];
                    } else {
                        return string.Empty;
                    }
                }

                return _tagAttributes[propertyName];
            }
        }

        /// <summary>
        /// Prend en charge le traitement du tag, injecte des données de trace.
        /// </summary>
        /// <returns>Contenu du tag, <code>null</code> si rien ne doit être rendu.</returns>
        public IEnumerable<OpenXmlElement> HandleTag() {
            if (_log.IsDebugEnabled) {
                _log.DebugFormat("Begin ProcessTag {0}", this.TagName);
            }

            IEnumerable<OpenXmlElement> retValue = this.ProcessTag();
            if (_log.IsDebugEnabled) {
                _log.DebugFormat("End ProcessTag {0}", this.TagName);
            }

            return retValue;
        }

        /// <summary>
        /// Libération des ressources.
        /// </summary>
        public void Dispose() {
            this.CurrentElement = null;
            this.CurrentPart = null;
            this.DataSource = null;
        }

        /// <summary>
        /// Recherche et suppression des Run en plus dans les enfants des current element.
        /// </summary>
        /// <param name="element">Current open xml element.</param>
        protected static void PrepareElement(OpenXmlElement element) {
            // Recherche des Run parallèles.
            List<Run> listRun = new List<Run>();
            foreach (OpenXmlElement current in element.ChildElements) {
                if (current.GetType() == typeof(Run)) {
                    listRun.Add((Run)current);
                } else if (current.GetType() == typeof(Paragraph)) {
                    foreach (OpenXmlElement currentChild in current.ChildElements) {
                        if (currentChild.GetType() == typeof(Run)) {
                            listRun.Add((Run)currentChild);
                        }
                    }
                } else if (current.GetType() == typeof(ProofError)) {
                    current.Remove();
                }
            }

            // Suppression des Run en plus.
            if (listRun.Count > 1) {
                for (int i = 0; i < listRun.Count; i++) {
                    if (i != 0) {
                        listRun[i].Remove();
                    }
                }
            }
        }

        /// <summary>
        /// Recursive get property value.
        /// </summary>
        /// <param name="dataSource">Source de données courante.</param>
        /// <param name="fieldName">Nom du Field.</param>
        /// <param name="isXmlData">Si la source en xml.</param>
        /// <returns>Value.</returns>
        protected object GetPropertyValue(object dataSource, string fieldName, bool isXmlData) {
            if (fieldName.Contains(".")) {

                string firstFieldName = fieldName.Substring(0, fieldName.IndexOf('.'));
                string lastFieldName = fieldName.Substring(fieldName.IndexOf('.') + 1);
                PropertyDescriptor property = TypeDescriptor.GetProperties(dataSource)[firstFieldName];
                object newDataSource = property.GetValue(dataSource);
                return GetPropertyValue(newDataSource, lastFieldName, isXmlData);
            } else {
                if (isXmlData) {
                    PropertyDescriptor property = TypeDescriptor.GetProperties(dataSource)[fieldName];
                    if (property != null) {
                        return property.GetValue(dataSource);
                    } else {
                        throw new KeyNotFoundException("The tag " + this.TagName + " has no attribute named " + fieldName);
                    }
                } else {
                    BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(dataSource);
                    BeanPropertyDescriptor descriptor = beanDefinition.Properties[fieldName];
                    if (descriptor != null) {
                        object propertyValue = descriptor.GetValue(dataSource);
                        if (propertyValue == null) {
                            return null;
                        } else {
                            ICollection collection = propertyValue as ICollection;
                            if (collection != null) {
                                if (collection.Count == 0) {
                                    return null;
                                } else {
                                    return propertyValue;
                                }
                            } else if (descriptor.PrimitiveType == typeof(byte[])) {
                                return propertyValue;
                            } else if (propertyValue.GetType() == typeof(DateTime)) {
                                if (descriptor.DomainName == "DO_JOUR_MOIS") {
                                    return ((DateTime)propertyValue).ToString("dd/MM", DateTimeFormatInfo.CurrentInfo);
                                } else {
                                    return descriptor.ConvertToString(propertyValue);
                                }
                            } else if (propertyValue.GetType() == typeof(bool)) {
                                return Convert.ToString(propertyValue, CultureInfo.InvariantCulture);
                            } else {
                                return descriptor.ConvertToString(propertyValue);
                            }
                        }
                    } else {
                        throw new KeyNotFoundException("The tag " + this.TagName + " has no attribute named " + fieldName);
                    }
                }
            }
        }

        /// <summary>
        /// Prise en charge partial du tag.
        /// </summary>
        /// <returns>Le contenu en OpenXML.</returns>
        protected IEnumerable<OpenXmlElement> PrepareClone() {
            this.CurrentElement.GetFirstChild<CustomXmlProperties>().Remove();
            List<OpenXmlElement> returnResult = new List<OpenXmlElement>();
            foreach (OpenXmlElement current in this.CurrentElement.ChildElements) {
                returnResult.Add((OpenXmlElement)current.Clone());
            }

            this.CurrentElement.RemoveAllChildren();
            return returnResult;
        }

        /// <summary>
        /// Prend en charge le traitement du tag.
        /// </summary>
        /// <returns>Contenu du tag interprété, <code>null</code> si rien ne doit être rendu.</returns>
        protected abstract IEnumerable<OpenXmlElement> ProcessTag();
    }
}