using System;
using System.Collections.Generic;
using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Kinetix.Reporting.TagHandlers {

    /// <summary>
    /// Handler du tag field, permet de remplacer le tag par du texte.
    /// </summary>
    internal class FieldNewLineHandler : AbstractTagHandler {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="currentPart">OpenXmlPart courant.</param>
        /// <param name="currentXmlElement">Tag Custom OpenXML.</param>
        /// <param name="currentDataSource">Source de données courante.</param>
        /// <param name="documentId">Id document en cours.</param>
        /// <param name="isXmlData">Si la source en xml.</param>
        public FieldNewLineHandler(OpenXmlPart currentPart, CustomXmlElement currentXmlElement, object currentDataSource, Guid documentId, bool isXmlData)
            : base(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData) {
            this.NbreNewLine = int.Parse(this["nbreNewLine"], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        /// NbreNewLine.
        /// </summary>
        public int NbreNewLine {
            get;
            private set;
        }

        /// <summary>
        /// Prise en charge du tag.
        /// </summary>
        /// <returns>Le contenu en OpenXML.</returns>
        protected override IEnumerable<OpenXmlElement> ProcessTag() {
            OpenXmlElement newElement = this.CurrentElement;
            if (newElement.GetFirstChild<CustomXmlProperties>() != null) {
                newElement.GetFirstChild<CustomXmlProperties>().Remove();
            }

            PrepareElement(newElement);
            List<OpenXmlElement> returnResult = new List<OpenXmlElement>();
            foreach (OpenXmlElement current in newElement.ChildElements) {
                ReplaceChild(current);
                returnResult.Add((OpenXmlElement)current.Clone());
                break;
            }

            newElement.RemoveAllChildren();
            newElement = null;
            return returnResult;
        }

        /// <summary>
        /// Parcourir les éléments enfant de façon récursive et remplacer le premier élément trouvé de type Text par ça valeur.
        /// </summary>
        /// <param name="current">Current open xml element.</param>
        protected void ReplaceChild(OpenXmlElement current) {
            foreach (OpenXmlElement currentChild in current.ChildElements) {
                OpenXmlElement parent = currentChild.Parent;
                if (currentChild.GetType() == typeof(Text)) {
                    for (int i = 0; i < NbreNewLine; i++) {
                        Break bk = new Break();
                        parent.InsertBefore(bk, currentChild);
                    }

                    currentChild.Remove();
                    break;
                } else {
                    ReplaceChild(currentChild);
                }
            }
        }
    }
}
