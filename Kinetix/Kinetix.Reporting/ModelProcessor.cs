using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using Kinetix.Reporting.TagHandlers;
using log4net;

namespace Kinetix.Reporting {

    /// <summary>
    /// Classe prenant en charge la fusion du document.
    /// </summary>
    public sealed class ModelProcessor : IDisposable {

        /// <summary>
        /// Contexte du document.
        /// </summary>
        private static readonly Dictionary<string, object> _contextDocumentParams = new Dictionary<string, object>();

        private static readonly object _lock = new object();

        /// <summary>
        /// Document OpenXml en cours de génération.
        /// </summary>
        private readonly WordprocessingDocument _document;

        /// <summary>
        /// Id document OpenXml en cours de génération.
        /// </summary>
        private readonly Guid _documentId;

        /// <summary>
        /// Validateur de document OpenXML, permet de vérifier que le fichier sera correctement interprété.
        /// </summary>
        private readonly OpenXmlValidator _validator = new OpenXmlValidator(FileFormatVersions.Office2007);

        /// <summary>
        /// Log applicatif.
        /// </summary>
        private readonly ILog _log = LogManager.GetLogger("Application");

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="stream">Flux content le modèle au format OpenXML.</param>
        public ModelProcessor(Stream stream) {
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }

            _documentId = Guid.NewGuid();
            int i = 1000;
            _contextDocumentParams.Add(_documentId.ToString(), i);
            _document = WordprocessingDocument.Open(stream, true);
        }

        /// <summary>
        /// Retourne la liste des tags à interpréter.
        /// </summary>
        private KeyValuePair<OpenXmlPart, CustomXmlElement> NextElement {
            get {
                foreach (HeaderPart headerPart in _document.MainDocumentPart.HeaderParts) {
                    foreach (CustomXmlElement customElement in headerPart.Header.Descendants<CustomXmlElement>()) {
                        return new KeyValuePair<OpenXmlPart, CustomXmlElement>(headerPart, customElement);
                    }
                }

                foreach (CustomXmlElement customElement in _document.MainDocumentPart.Document.Body.Descendants<CustomXmlElement>()) {
                    return new KeyValuePair<OpenXmlPart, CustomXmlElement>(_document.MainDocumentPart, customElement);
                }

                foreach (FooterPart footerPart in _document.MainDocumentPart.FooterParts) {
                    foreach (CustomXmlElement customElement in footerPart.Footer.Descendants<CustomXmlElement>()) {
                        return new KeyValuePair<OpenXmlPart, CustomXmlElement>(footerPart, customElement);
                    }
                }

                return new KeyValuePair<OpenXmlPart, CustomXmlElement>(null, null);
            }
        }

        /// <summary>
        /// Compteur en cours des id image du document.
        /// </summary>
        /// <param name="documentId">Id document.</param>
        /// <returns>Id image suivant.</returns>
        public static int GetNextImageCounter(string documentId) {
            _contextDocumentParams[documentId] = (int)_contextDocumentParams[documentId] + 1;
            return (int)_contextDocumentParams[documentId];
        }

        /// <summary>
        /// Prend en charge la fusion du document.
        /// </summary>
        /// <param name="xmlData">Données devant être injectées dans le document au format XML.</param>
        public void ProcessXml(string xmlData) {
            if (string.IsNullOrEmpty(xmlData)) {
                throw new ArgumentNullException("xmlData");
            }

            object currentDataSource = CreateDataSource(xmlData);
            Process(currentDataSource, true);
        }

        /// <summary>
        /// Prend en charge la fusion du document.
        /// </summary>
        /// <param name="currentDataSource">Données devant être injectées dans le document.</param>
        public void Process(object currentDataSource) {
            if (currentDataSource == null) {
                throw new ArgumentNullException("currentDataSource");
            }

            Process(currentDataSource, false);
        }

        /// <summary>
        /// Prend en charge la fusion du document.
        /// </summary>
        /// <param name="currentDataSource">Source de données courante.</param>
        /// <param name="isXmlData">Si la source en xml.</param>
        public void Process(object currentDataSource, bool isXmlData) {
            KeyValuePair<OpenXmlPart, CustomXmlElement> tagItem = NextElement;
            List<SectionProperties> listSp1 = new List<SectionProperties>();
            foreach (SectionProperties sp in _document.MainDocumentPart.Document.Body.Descendants<SectionProperties>()) {
                listSp1.Add((SectionProperties)sp.Clone());
            }

            while (tagItem.Value != null) {
                OpenXmlPart currentPart = tagItem.Key;
                CustomXmlElement currentElement = tagItem.Value;
                CustomNodeProcessor.Process(currentPart, currentElement, currentDataSource, _documentId, isXmlData);
                tagItem = NextElement;
            }

            List<Paragraph> list = new List<Paragraph>();
            foreach (Paragraph paragraph in _document.MainDocumentPart.Document.Body.Elements<Paragraph>()) {
                if (string.IsNullOrEmpty(paragraph.InnerText)) {
                    if (paragraph.ParagraphProperties == null) {
                        list.Add(paragraph);
                    }
                }
            }

            foreach (Paragraph item in list) {
                item.Remove();
            }

            List<SectionProperties> listSp2 = new List<SectionProperties>();
            foreach (SectionProperties sp in _document.MainDocumentPart.Document.Body.Descendants<SectionProperties>()) {
                listSp2.Add(sp);
            }

            if (listSp2.Count == 1) {
                OpenXmlElement element = (OpenXmlElement)listSp2[0];
                OpenXmlElement parent = element.Parent;
                parent.InsertBefore(listSp1[0], element);
                element.Remove();
            }

            bool hasError = false;
            IEnumerable<ValidationErrorInfo> validationErrors = _validator.Validate(_document);
            foreach (ValidationErrorInfo error in validationErrors) {
                hasError = true;
                _log.ErrorFormat(CultureInfo.InvariantCulture, "Erreur de validation du tag {0} : \r\nErrorType={1}\r\nDescription={2}\r\nPath={3}\r\nUri={4}", error.ToString(), error.ErrorType, error.Description, error.Path, error.Part.Uri);
            }

            if (hasError) {
                throw new ReportException("Le document généré n'est pas valide, consultez le log pour plus de détails.");
            }

            lock (_lock) {
                _document.MainDocumentPart.Document.Save();
                _document.Close();
            }
        }

        /// <summary>
        /// Libère les ressources.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libère les ressources.
        /// </summary>
        /// <param name="disposing">Dispose.</param>
        public void Dispose(bool disposing) {
            if (disposing) {
                if (_document != null) {
                    _contextDocumentParams.Remove(_document.ToString());
                    _document.Dispose();
                }
            }
        }

        /// <summary>
        /// Lit les données sérialisées en XML de représentation des objets de transfert et les mappe dans un ReportBean.
        /// </summary>
        /// <param name="xmlData">Données XML transmises pour la génération de l'édition.</param>
        /// <returns>Les données lues.</returns>
        private object CreateDataSource(object xmlData) {
            try {
                using (StringReader sr = new StringReader(xmlData.ToString()))
                using (XmlTextReader reader = new XmlTextReader(sr)) {
                    reader.WhitespaceHandling = WhitespaceHandling.None;
                    while (!ReportDocument.XmlNodeDocument.Equals(reader.Name) && !reader.EOF) {
                        reader.Read();
                    }

                    return new ReportBean("Document", "Document", reader);
                }
            } catch (Exception ex) {
                if (_log.IsErrorEnabled) {
                    _log.Error("Erreur lors de la lecture du fichier de données", ex);
                }

                throw new ReportException(ex.Message, ex);
            }
        }
    }
}
