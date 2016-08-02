using System;
using System.Collections;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Kinetix.Reporting.TagHandlers {

    /// <summary>
    /// Handler du tag For, permet de réaliser une itération.
    /// </summary>
    internal class ForHandler : AbstractTagHandler {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="currentPart">OpenXmlPart courant.</param>
        /// <param name="currentXmlElement">Tag Custom OpenXML.</param>
        /// <param name="currentDataSource">Source de données courante.</param>
        /// <param name="documentId">Id document en cours.</param>
        /// <param name="isXmlData">Si la source en xml.</param>
        public ForHandler(OpenXmlPart currentPart, CustomXmlElement currentXmlElement, object currentDataSource, Guid documentId, bool isXmlData)
            : base(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData) {
            this.CollectionName = this["collection"];
        }

        /// <summary>
        /// Obtient le nom de la collection sur laquelle le tag itère.
        /// </summary>
        public string CollectionName {
            get;
            private set;
        }

        /// <summary>
        /// Prend en charge le tag.
        /// </summary>
        /// <returns>Le contenu en OpenXML.</returns>
        protected override IEnumerable<OpenXmlElement> ProcessTag() {
            IEnumerable dataSourceList = GetPropertyValue(this.DataSource, this.CollectionName, this.IsXmlData) as IEnumerable;
            if (dataSourceList == null) {
                return new List<OpenXmlElement>();
            }

            List<OpenXmlElement> list = new List<OpenXmlElement>();
            foreach (object item in dataSourceList) {
                OpenXmlElement newElement = (OpenXmlElement)this.CurrentElement.Clone();
                newElement.GetFirstChild<CustomXmlProperties>().Remove();
                CustomXmlElement customElement = null;
                do {
                    customElement = null;
                    foreach (CustomXmlElement item2 in newElement.Descendants<CustomXmlElement>()) {
                        if (item2 != null) {
                            customElement = item2;
                            break;
                        }
                    }

                    if (customElement != null) {
                        CustomNodeProcessor.Process(this.CurrentPart, customElement, item, this.DocumentId, this.IsXmlData);
                    }
                } while (customElement != null);
                foreach (OpenXmlElement item3 in newElement.ChildElements) {
                    list.Add((OpenXmlElement)item3.Clone());
                }

                newElement.RemoveAllChildren();
                newElement = null;
            }

            return list;
        }
    }
}
