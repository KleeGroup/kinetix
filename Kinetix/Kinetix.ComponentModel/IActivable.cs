namespace Kinetix.ComponentModel {

    /// <summary>
    /// Interface contractualisant les objets (dés)activables.
    /// </summary>
    public interface IActivable {
        /// <summary>
        /// Retourne ou définit l'état actif de l'item.
        /// </summary>
        bool? IsActif { get; set; }
    }
}
