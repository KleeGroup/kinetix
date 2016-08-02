using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Kinetix.Reporting {

    /// <summary>
    /// Classe de génération de fichier XML.
    /// </summary>
    public sealed class ReportToXml : IDisposable {

        /// <summary>
        /// Flux mémoire.
        /// </summary>
        private MemoryStream _thread;

        /// <summary>
        /// Flux XML.
        /// </summary>
        private XmlTextWriter _writer;

        /// <summary>
        /// DocType du document généré.
        /// </summary>
        private string _docType = string.Empty;

        /// <summary>
        /// Nom du noeud racine.
        /// </summary>
        private string _rootElement = string.Empty;

        /// <summary>
        /// Fichier généré au format String.
        /// </summary>
        private string _dataString = string.Empty;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="rootElement">Nom du noeud racine.</param>
        public ReportToXml(string rootElement) {
            _rootElement = rootElement;
            OpenDocument();
            WriteHeader();
        }

        /// <summary>
        /// Constructeur avec docType.
        /// </summary>
        /// <param name="docType">La docType du fichier à généré.</param>
        /// <param name="rootElement">Nom du noeud racine.</param>
        public ReportToXml(string docType, string rootElement) {
            _docType = docType;
            _rootElement = rootElement;
            OpenDocument();
            WriteHeader();
        }

        /// <summary>
        /// Ajoute une feuille à l'arbre XML.
        /// </summary>
        /// <param name="name">Nom du noeud XML.</param>
        /// <param name="value">Valeur du noeud XML.</param>
        /// <param name="attributes">Liste des attributs du noeud.</param>
        public void WriteLeaf(string name, object value, params KeyValuePair<string, object>[] attributes) {
            _writer.WriteStartElement(name);
            if (attributes != null) {
                for (int i = 0; i < attributes.Length; i++) {
                    WriteAttribute(attributes[i].Key, attributes[i].Value);
                }
            }

            string strValue = Convert.ToString(value, CultureInfo.CurrentCulture);
            if (strValue != null && strValue.Length > 0) {
                _writer.WriteCData(strValue);
            }

            _writer.WriteEndElement();
        }

        /// <summary>
        /// Ouvre un noeud de l'arbre XML.
        /// </summary>
        /// <param name="name">Nom du noeud.</param>
        /// <param name="attributes">Attributs du noeud.</param>
        public void WriteStartNode(string name, params KeyValuePair<string, object>[] attributes) {
            _writer.WriteStartElement(name);
            if (attributes != null) {
                for (int i = 0; i < attributes.Length; i++) {
                    WriteAttribute(attributes[i].Key, attributes[i].Value);
                }
            }
        }

        /// <summary>
        /// Ferme un noeud de l'arbre XML.
        /// </summary>
        public void WriteEndNode() {
            _writer.WriteEndElement();
        }

        /// <summary>
        /// Génère le fichier XML.
        /// </summary>
        /// <returns>Le fichier généré.</returns>
        public string GenerateXml() {
            WriteFooter();
            string xmlData = string.Empty;
            _writer.Flush(); // Ecriture dans le thread en mémoire
            using (StreamReader reader = new StreamReader(_thread)) {
                _thread.Seek(0, SeekOrigin.Begin);
                xmlData = reader.ReadToEnd();
                reader.Close();
            }

            // Validation des données créées
            if (!string.IsNullOrEmpty(_docType)) {
                ValidateXml(xmlData);
            }

            _dataString = xmlData;
            return _dataString;
        }

        /// <summary>
        /// Impression du fichier XML généré.
        /// </summary>
        /// <returns>Le fichier généré au format chaîne de caractères.</returns>
        public override string ToString() {
            return _dataString;
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose() {
            CloseDocument();
        }

        /// <summary>
        /// Fonction de validation du fichier XML envoyé à partir de la DTD
        /// du fichier.
        /// </summary>
        /// <param name="data">Données à valider.</param>
        private static void ValidateXml(string data) {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            XmlParserContext context = new XmlParserContext(null, nsmgr, null, XmlSpace.None);
            using (XmlTextReader reader = new XmlTextReader(data, XmlNodeType.Document, context)) {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.DTD;
                XmlReader readerXml = XmlReader.Create(reader, settings);
                try {
                    while (readerXml.Read()) {
                    }
                } catch (XmlException xe) {
                    throw new NotSupportedException("La structure du fichier XML reçu n'est pas respectée : " + xe.Message);
                }
            }
        }

        /// <summary>
        /// Ecrit un attribut.
        /// </summary>
        /// <param name="name">Nom de l'attribut.</param>
        /// <param name="value">Valeur de l'attribut.</param>
        private void WriteAttribute(string name, object value) {
            _writer.WriteStartAttribute(name);
            _writer.WriteString(Convert.ToString(value, CultureInfo.CurrentCulture));
            _writer.WriteEndAttribute();
        }

        /// <summary>
        /// Ouverture du document.
        /// </summary>
        private void OpenDocument() {
            _thread = new MemoryStream();
            _writer = new XmlTextWriter(_thread, Encoding.UTF8);
        }

        /// <summary>
        /// Fermeture du document.
        /// </summary>
        private void CloseDocument() {
            if (_writer != null) {
                _writer.Close();
                _writer = null;
            }

            if (_thread != null) {
                _thread.Close();
                _thread = null;
            }
        }

        /// <summary>
        /// Ecrit la doctype dans le document généré.
        /// </summary>
        private void WriteDoctype() {
            if (string.IsNullOrEmpty(this._docType)) {
                return;
            }

            _writer.WriteDocType(_rootElement, null, null, this._docType);
        }

        /// <summary>
        /// Ecrit le noeud racine du document (unique).
        /// </summary>
        private void WriteRootElement() {
            if (string.IsNullOrEmpty(_rootElement)) {
                throw new ArgumentException("Le noeud racine doit être mentionné dans le constructeur.");
            }

            _writer.WriteStartElement(_rootElement);
        }

        /// <summary>
        /// Ouverture du document : écriture de l'entête et ouverture
        /// du noed racine.
        /// </summary>
        private void WriteHeader() {
            _writer.WriteStartDocument();
            _writer.Formatting = Formatting.Indented;
            WriteDoctype();
            WriteRootElement();
        }

        /// <summary>
        /// Fermeture du noeud racine et du document.
        /// </summary>
        private void WriteFooter() {
            _writer.WriteEndElement();
            _writer.WriteEndDocument();
        }
    }
}
