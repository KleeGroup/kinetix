namespace Kinetix.ClassGenerator.Model {

    /// <summary>
    /// Contrat des objets ayant des données de persistance.
    /// </summary>
    public interface IPersistenceData {

        /// <summary>
        /// Obtient ou définit le type persistant.
        /// </summary>
        string PersistentDataType { get; set; }

        /// <summary>
        /// Retourne la longueur persistente du domaine.
        /// </summary>
        int? PersistentLength { get; set; }

        /// <summary>
        /// Retourne la précision du dommaine.
        /// </summary>
        int? PersistentPrecision { get; set; }
    }
}
