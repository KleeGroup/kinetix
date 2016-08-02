using System.Web.UI;

namespace Kinetix.Monitoring.Manager {
    /// <summary>
    /// Gestion des interactions avec les managers.
    /// Cette interactions s'effectue depuis la console d'administration.(Monitoring).
    /// </summary>
    public interface IManagerListener {
        /// <summary>
        /// Exécution d'un événement relatif au manager.
        /// </summary>
        /// <param name="actionName">Action.</param>
        /// <param name="writer">Writer HTML.</param>
        void Execute(string actionName, HtmlTextWriter writer);
    }
}
