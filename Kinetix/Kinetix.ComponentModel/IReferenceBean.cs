namespace Kinetix.ComponentModel {

    /// <summary>
    /// Interface contractualisant un objet d'une liste de référence administrable.
    /// </summary>
    public interface IReferenceBean : IActivable {

        /// <summary>
        /// Retourne ou définit le code de l'item.
        /// </summary>
        int? Id { get; set; }

        /// <summary>
        /// Retourne ou définit le libellé de l'item.
        /// </summary>
        string Libelle { get; set; }
    }
}
