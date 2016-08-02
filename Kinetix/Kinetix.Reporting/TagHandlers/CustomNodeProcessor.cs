using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Kinetix.Reporting.TagHandlers {

    /// <summary>
    /// Utility TagHandler.
    /// </summary>
    internal static class CustomNodeProcessor {

        /// <summary>
        /// Nom du tag CustomOpenXml représentant les Fields text.
        /// </summary>
        private const string FieldTagName = "field";

        /// <summary>
        /// Nom du tag CustomOpenXml représentant les Fields NewLine.
        /// </summary>
        private const string FieldNewLine = "fieldNewLine";

        /// <summary>
        /// Nom du tag CustomOpenXml représentant les Fields Currency.
        /// </summary>
        private const string FieldCurrencyTagName = "fieldCurrency";

        /// <summary>
        /// Nom du tag CustomOpenXml représentant les boucles.
        /// </summary>
        private const string ForTagName = "for";

        /// <summary>
        /// Nom du tag CustomOpenXml représentant les images.
        /// </summary>
        private const string ImageTagName = "image";

        /// <summary>
        /// Nom du tag CustomOpenXml représentant les conditions.
        /// </summary>
        private const string IfTagName = "if";

        /// <summary>
        /// Remplacer les éléments trouvés.
        /// </summary>
        /// <param name="currentPart">OpenXmlPart courant.</param>
        /// <param name="currentXmlElement">XmlElement courant.</param>
        /// <param name="currentDataSource">Source de données courante.</param>
        /// <param name="documentId">Id document en cours.</param>
        /// <param name="isXmlData">Si la source en xml.</param>
        public static void Process(OpenXmlPart currentPart, CustomXmlElement currentXmlElement, object currentDataSource, Guid documentId, bool isXmlData) {
            using (ITagHandler tagHandler = CreateTagHandler(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData)) {
                IEnumerable<OpenXmlElement> newElementList = tagHandler.HandleTag();
                OpenXmlElement parent = currentXmlElement.Parent;
                if (newElementList == null) {
                    if (currentXmlElement.Parent.GetType() != typeof(Paragraph) && currentXmlElement.Parent.GetType() != typeof(TableRow) && currentXmlElement.Parent.GetType() != typeof(Table) && currentXmlElement.Parent.GetType() != typeof(Body) && currentXmlElement.Parent.GetType() != typeof(CustomXmlRow)) {
                        Paragraph p = new Paragraph();
                        if (currentXmlElement.Parent.GetType() == typeof(TableCell)) {
                            if (currentXmlElement.Descendants<ParagraphProperties>() != null) {
                                IEnumerator<ParagraphProperties> ppEnum = currentXmlElement.Descendants<ParagraphProperties>().GetEnumerator();
                                ppEnum.MoveNext();
                                if (ppEnum.Current != null) {
                                    p.AppendChild<OpenXmlElement>(ppEnum.Current.CloneNode(true));
                                }
                            }
                        }

                        parent.InsertBefore(p, currentXmlElement);
                    } else if (parent.GetType() == typeof(TableRow)) {
                        Paragraph p2 = new Paragraph();
                        TableCell tc = new TableCell();
                        if (currentXmlElement.Descendants<ParagraphProperties>() != null) {
                            IEnumerator<ParagraphProperties> ppEnum = currentXmlElement.Descendants<ParagraphProperties>().GetEnumerator();
                            ppEnum.MoveNext();
                            p2.AppendChild<OpenXmlElement>(ppEnum.Current.CloneNode(true));
                        }

                        tc.AppendChild<Paragraph>(p2);
                        parent.InsertBefore(tc, currentXmlElement);
                    }
                } else {
                    OpenXmlElement lastElement = currentXmlElement;
                    foreach (OpenXmlElement currentChild in newElementList) {
                        OpenXmlElement currentChildClone = (OpenXmlElement)currentChild;
                        parent.InsertAfter(currentChildClone, lastElement);
                        lastElement = currentChildClone;
                    }

                    newElementList = null;
                }

                currentXmlElement.Remove();
                currentXmlElement = null;
            }
        }

        /// <summary>
        /// Création du Handler.
        /// </summary>
        /// <param name="currentPart">OpenXmlPart courant.</param>
        /// <param name="currentXmlElement">XmlElement courant.</param>
        /// <param name="currentDataSource">Source de données courante.</param>
        /// <param name="documentId">Id document en cours.</param>
        /// <param name="isXmlData">Si la source en xml.</param>
        /// <returns>ITagHandler.</returns>
        private static ITagHandler CreateTagHandler(OpenXmlPart currentPart, CustomXmlElement currentXmlElement, object currentDataSource, Guid documentId, bool isXmlData) {
            if (currentXmlElement.Element == null) {
                throw new NotSupportedException("The property 'Element' of currentXmlElement is null.");
            }

            switch (currentXmlElement.Element.Value) {
                case FieldTagName:
                    return new FieldHandler(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData);
                case FieldNewLine:
                    return new FieldNewLineHandler(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData);
                case FieldCurrencyTagName:
                    return new FieldCurrencyHandler(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData);
                case ForTagName:
                    return new ForHandler(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData);
                case ImageTagName:
                    return new ImageHandler(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData);
                case IfTagName:
                    return new IfHandler(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData);
                default:
                    throw new ReportException("Tag " + currentXmlElement.Element.Value + " has no defined handler.");
            }
        }
    }
}
