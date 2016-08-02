using System.Web.UI;

namespace Kinetix.Monitoring.Manager {
    /// <summary>
    /// Interface de description d'un manager.
    /// </summary>
    public interface IManagerDescription {
        /// <summary>
        /// Nom du manager.
        /// </summary>
        string Name {
            get;
        }

        /// <summary>
        /// Image du manager.
        /// </summary>
        string Image {
            get;
        }

        /// <summary>
        /// Type mime de l'image.
        /// </summary>
        string ImageMimeType {
            get;
        }

        /// <summary>
        /// Image.
        /// </summary>
        byte[] ImageData {
            get;
        }

        /// <summary>
        /// Priorité d'affichage du manager.
        /// </summary>
        int Priority {
            get;
        }

        /// <summary>
        /// Extension de la méthode toString().
        /// Permet à chaque Manager de présenter son propre état.
        /// </summary>
        /// <param name="writer">Writer HTML.</param>
        void ToHtml(HtmlTextWriter writer);
    }
}
