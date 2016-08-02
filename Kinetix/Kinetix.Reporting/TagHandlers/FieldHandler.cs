using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Kinetix.Reporting.TagHandlers {

    /// <summary>
    /// Handler du tag field, permet de remplacer le tag par du texte.
    /// </summary>
    internal class FieldHandler : AbstractTagHandler {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="currentPart">OpenXmlPart courant.</param>
        /// <param name="currentXmlElement">Tag Custom OpenXML.</param>
        /// <param name="currentDataSource">Source de données courante.</param>
        /// <param name="documentId">Id document en cours.</param>
        /// <param name="isXmlData">Si la source en xml.</param>
        public FieldHandler(OpenXmlPart currentPart, CustomXmlElement currentXmlElement, object currentDataSource, Guid documentId, bool isXmlData)
            : base(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData) {
            this.FieldName = this["name"];
        }

        /// <summary>
        /// Nom du field courant.
        /// </summary>
        public string FieldName {
            get;
            protected set;
        }

        /// <summary>
        /// Récupère la liste des l'objet Text et Break à insérer avec un formatage.
        /// </summary>
        /// <param name="lineList">Liste des lignes.</param>
        /// <returns>Liste des Text et Break.</returns>
        public List<OpenXmlElement> GetOpenXmlElementList(string[] lineList) {
            List<OpenXmlElement> list = new List<OpenXmlElement>();
            for (int i = 0; i < lineList.Length; i++) {
                list.Add(GetText(lineList[i]));
                if (i != lineList.Length - 1) {
                    list.Add(new Break());
                }
            }

            return list;
        }

        /// <summary>
        /// Récupérer l'objet Text a inséré avec un formatage.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <returns>Text.</returns>
        public virtual Text GetText(object propertyValue) {
            return new Text(propertyValue.ToString());
        }

        /// <summary>
        /// Prise en charge du tag.
        /// </summary>
        /// <returns>Le contenu en OpenXML.</returns>
        protected override IEnumerable<OpenXmlElement> ProcessTag() {
            object propertyValue = GetPropertyValue(this.DataSource, this.FieldName, this.IsXmlData);
            if (propertyValue == null) {
                return null;
            }

            OpenXmlElement newElement = this.CurrentElement;
            newElement.GetFirstChild<CustomXmlProperties>().Remove();
            PrepareElement(newElement);
            List<OpenXmlElement> returnResult = new List<OpenXmlElement>();
            foreach (OpenXmlElement current in newElement.ChildElements) {
                ReplaceChild(propertyValue, current);
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
        /// <param name="propertyValue">Property value.</param>
        /// <param name="current">Current open xml element.</param>
        protected void ReplaceChild(object propertyValue, OpenXmlElement current) {
            foreach (OpenXmlElement currentChild in current.ChildElements) {
                OpenXmlElement parent = currentChild.Parent;
                if (currentChild.GetType() == typeof(Text)) {

                    // string[] lineList = propertyValue.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None);
                    List<string> newList = new List<string>();
                    string value = propertyValue.ToString();
                    while (!string.IsNullOrEmpty(value)) {
                        if (value.StartsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase)) {
                            newList.Add(Environment.NewLine);
                            value = value.Substring(2);
                        } else if (value.Contains(Environment.NewLine)) {
                            newList.Add(value.Substring(0, value.IndexOf(Environment.NewLine, StringComparison.OrdinalIgnoreCase)));
                            value = value.Substring(value.IndexOf(Environment.NewLine, StringComparison.OrdinalIgnoreCase) + 2);
                        } else {
                            newList.Add(value);
                            value = string.Empty;
                        }
                    }

                    if (newList.Count > 1) {
                        List<OpenXmlElement> list = GetOpenXmlElementList(newList.ToArray());
                        for (int i = 0; i < list.Count; i++) {
                            parent.InsertBefore(list[i], currentChild);
                        }
                    } else {
                        parent.InsertBefore(GetText(propertyValue), currentChild);
                    }

                    currentChild.Remove();
                    break;
                } else {
                    ReplaceChild(propertyValue, currentChild);
                }
            }
        }
    }
}
