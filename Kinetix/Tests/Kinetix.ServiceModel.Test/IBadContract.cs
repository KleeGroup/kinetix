namespace Kinetix.ServiceModel.Test {
    /// <summary>
    /// Contrat d'exemple éronné pour le test de la factory de proxy.
    /// </summary>
    public interface IBadContract {
        /// <summary>
        /// Retourne une liste d'objets.
        /// </summary>
        /// <returns>Liste d'objets.</returns>
        [ReferenceAccessor]
        object GetObjectList();
    }
}
