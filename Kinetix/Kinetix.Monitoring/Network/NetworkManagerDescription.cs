using System.Web.UI;
using Kinetix.Monitoring.Manager;

namespace Kinetix.Monitoring.Network {
    /// <summary>
    /// Description d'un manager issue du réseau.
    /// </summary>
    internal class NetworkManagerDescription : IManagerDescription {
        /// <summary>
        /// Nom du manager.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Image du manager.
        /// </summary>
        public string Image {
            get;
            set;
        }

        /// <summary>
        /// Type mime de l'image.
        /// </summary>
        public string ImageMimeType {
            get;
            set;
        }

        /// <summary>
        /// Image.
        /// </summary>
        public byte[] ImageData {
            get;
            set;
        }

        /// <summary>
        /// Priorité d'affichage du manager.
        /// </summary>
        public int Priority {
            get;
            set;
        }

        /// <summary>
        /// Extension de la méthode toString().
        /// Permet à chaque Manager de présenter son propre état.
        /// </summary>
        /// <param name="writer">Writer HTML.</param>
        public void ToHtml(HtmlTextWriter writer) {
        }
    }
}
