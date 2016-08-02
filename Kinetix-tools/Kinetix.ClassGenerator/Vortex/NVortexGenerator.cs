using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml;

namespace Kinetix.ClassGenerator.NVortex {

    /// <summary>
    /// Génère le fichier nvortex.
    /// </summary>
    public static class NVortexGenerator {

        /// <summary>
        /// Génère le fichier nvortex a partir de la liste d'erreurs.
        /// </summary>
        /// <param name="liste">Liste d'erreurs.</param>
        /// <param name="outputFile">Nom du fichier de sortie.</param>
        /// <param name="cvsRepository">Repository CVS.</param>
        /// <param name="source">Nom de la source.</param>
        public static void Generate(ICollection<NVortexMessage> liste, string outputFile, string cvsRepository, string source) {
            if (liste == null) {
                throw new ArgumentNullException("liste");
            }

            XmlTextWriter xmlWriter = new XmlTextWriter(outputFile, Encoding.UTF8);
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("vortex");

            foreach (NVortexMessage message in liste) {
                xmlWriter.WriteStartElement("jdtmessage");
                string msgCat;
                switch (message.Category) {
                    case Category.Error:
                        msgCat = "ERROR";
                        break;
                    case Category.Perf:
                        msgCat = "PERF";
                        break;
                    case Category.Unclassified:
                        msgCat = "UNCLASSIFIED";
                        break;
                    case Category.Bug:
                        msgCat = "BUG";
                        break;
                    case Category.Doc:
                        msgCat = "DOC";
                        break;
                    case Category.CodeStyle:
                        msgCat = "CODESTYLE";
                        break;
                    default:
                        throw new NotSupportedException();
                }

                xmlWriter.WriteAttributeString("msgcat", msgCat);
                xmlWriter.WriteAttributeString("priority", message.IsError ? "error" : "warn");
                xmlWriter.WriteAttributeString("source", source); // <jdtmessage>

                xmlWriter.WriteStartElement("problem"); // <problem>
                xmlWriter.WriteString(string.Empty);
                xmlWriter.WriteEndElement();            // </problem>

                xmlWriter.WriteStartElement("message"); // <message>
                xmlWriter.WriteString(message.Description);
                xmlWriter.WriteEndElement();            // </message>

                xmlWriter.WriteStartElement("lineNumber");
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("who");
                xmlWriter.WriteString(ConfigurationManager.AppSettings["ModelMaster"]);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("cvsRepository");
                xmlWriter.WriteString(cvsRepository);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("filename");
                xmlWriter.WriteString(message.FileName);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();            // </jdtmessage>
            }

            xmlWriter.WriteEndElement();
            xmlWriter.Close();
        }
    }
}
