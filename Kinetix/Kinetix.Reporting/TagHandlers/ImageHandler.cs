using System;
using System.Collections.Generic;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Kinetix.Reporting.TagHandlers {

    /// <summary>
    /// Handler du tag Image, permet d'injecter des images dans un document OpenXML.
    /// </summary>
    internal class ImageHandler : AbstractTagHandler {

        private string _guid;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="currentPart">OpenXmlPart courant.</param>
        /// <param name="currentXmlElement">XmlElement courant.</param>
        /// <param name="currentDataSource">Source de données courante.</param>
        /// <param name="documentId">Id document en cours.</param>
        /// <param name="isXmlData">Si la source en xml.</param>
        public ImageHandler(OpenXmlPart currentPart, CustomXmlElement currentXmlElement, object currentDataSource, Guid documentId, bool isXmlData)
            : base(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData) {
            this.ImageName = this["name"];
        }

        /// <summary>
        /// Obtient le nom de la propriété contenant le byte[] de définition de l'image.
        /// </summary>
        public string ImageName {
            get;
            private set;
        }

        /// <summary>
        /// Prise en charge du tag.
        /// </summary>
        /// <returns>Le contenu en OpenXML.</returns>
        protected override IEnumerable<OpenXmlElement> ProcessTag() {
            object propertyValue = GetPropertyValue(this.DataSource, this.ImageName, this.IsXmlData);
            if (propertyValue == null) {
                return null;
            }

            byte[] b;
            if (propertyValue.GetType() == typeof(byte[])) {
                b = (byte[])propertyValue;
            } else {
                b = Convert.FromBase64String(propertyValue.ToString());
            }

            IEnumerable<Blip> list = this.CurrentElement.Descendants<Blip>();
            foreach (Blip item in list) {
                item.Embed = AddDocumentPart(b);
                item.CompressionState = new EnumValue<BlipCompressionValues>(BlipCompressionValues.HighQualityPrint);
            }

            return this.PrepareClone();
        }

        /// <summary>
        /// Ajoute un élément binaire au document principal.
        /// </summary>
        /// <param name="fichier">Fichier.</param>
        /// <returns>Code embed.</returns>
        private string AddDocumentPart(byte[] fichier) {
            _guid = Guid.NewGuid().ToString();

            long width = 0;
            long height = 0;
            ImagePartType imagePartType;
            if (fichier != null && fichier.Length > 0) {
                using (MemoryStream imgStream = new MemoryStream((byte[])fichier.Clone())) {
                    System.Drawing.Image img = System.Drawing.Image.FromStream(imgStream);
                    if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png)) {
                        imagePartType = ImagePartType.Png;
                    } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif)) {
                        imagePartType = ImagePartType.Gif;
                    } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg)) {
                        imagePartType = ImagePartType.Jpeg;
                    } else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Tiff)) {
                        imagePartType = ImagePartType.Tiff;
                    } else {
                        imagePartType = ImagePartType.Png;
                    }

                    width = (long)(img.Width * 914400 / img.HorizontalResolution);
                    height = (long)(img.Height * 914400 / img.VerticalResolution);
                }

                IEnumerable<Extent> extentList = this.CurrentElement.Descendants<Extent>();
                foreach (Extent item in extentList) {
                    item.Cx = new Int64Value(width);
                    item.Cy = new Int64Value(height);
                    break;
                }

                IEnumerable<Extents> extentsList = this.CurrentElement.Descendants<Extents>();
                foreach (Extents item in extentsList) {
                    item.Cx = new Int64Value(width);
                    item.Cy = new Int64Value(height);
                    break;
                }

                IEnumerable<DocProperties> docPropertiesList = this.CurrentElement.Descendants<DocProperties>();
                foreach (DocProperties item in docPropertiesList) {
                    item.Id = Convert.ToUInt32(ModelProcessor.GetNextImageCounter(this.DocumentId.ToString()));
                    item.Description = string.Empty;
                    break;
                }

                IEnumerable<GraphicData> graphicDataList = this.CurrentElement.Descendants<GraphicData>();
                foreach (GraphicData item in graphicDataList) {
                    item.Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture";
                    break;
                }

                IEnumerable<GraphicFrameLocks> graphicFrameLocksList = this.CurrentElement.Descendants<GraphicFrameLocks>();
                foreach (GraphicFrameLocks item in graphicFrameLocksList) {
                    item.NoChangeAspect = true;
                    item.NoResize = true;
                    break;
                }

                IEnumerable<NonVisualDrawingProperties> nonVisualDrawingPropertiesList = this.CurrentElement.Descendants<NonVisualDrawingProperties>();
                foreach (NonVisualDrawingProperties item in nonVisualDrawingPropertiesList) {
                    item.Description = string.Empty;
                    break;
                }

                using (MemoryStream msImagePart = new MemoryStream(fichier)) {
                    if (this.CurrentPart.GetType() == typeof(HeaderPart)) {
                        ImagePart imagePart = ((HeaderPart)this.CurrentPart).AddImagePart(imagePartType, "img" + _guid);
                        imagePart.FeedData(msImagePart);
                    } else if (this.CurrentPart.GetType() == typeof(FooterPart)) {
                        ImagePart imagePart = ((FooterPart)this.CurrentPart).AddImagePart(imagePartType, "img" + _guid);
                        imagePart.FeedData(msImagePart);
                    } else {
                        ImagePart imagePart = ((MainDocumentPart)this.CurrentPart).AddImagePart(imagePartType, "img" + _guid);
                        imagePart.FeedData(msImagePart);
                    }
                }

                return "img" + _guid;
            }

            return string.Empty;
        }
    }
}
