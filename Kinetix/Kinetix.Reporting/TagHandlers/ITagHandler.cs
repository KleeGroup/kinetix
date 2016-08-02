using System;
using System.Collections.Generic;
using DocumentFormat.OpenXml;

namespace Kinetix.Reporting.TagHandlers {

    /// <summary>
    /// Interface contractualisant l'interprétation d'un tag "Custom" dans le document OpenXML.
    /// </summary>
    internal interface ITagHandler : IDisposable {

        /// <summary>
        /// Prend en charge le tag CustomOpenXML.
        /// </summary>
        /// <returns>Le contenu remplacé en OpenXML, null si rien ne doit être mis.</returns>
        IEnumerable<OpenXmlElement> HandleTag();
    }
}
