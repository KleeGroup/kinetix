namespace Kinetix.ComponentModel {
    /// <summary>
    /// Interface contractualisant un objet d'une liste de référence statique.
    /// </summary>
    public interface IStaticBean {

        /// <summary>
        /// Retourne ou définit le code de l'item.
        /// </summary>
        string Code { get; set; }

        /// <summary>
        /// Retourne ou définit le libellé de l'item.
        /// </summary>
        string Libelle { get; set; }
    }
}
