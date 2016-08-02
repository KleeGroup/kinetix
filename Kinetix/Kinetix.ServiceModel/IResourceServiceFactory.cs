namespace Kinetix.ServiceModel {

    /// <summary>
    /// Factory des services d'accès aux ressources de libellés.
    /// </summary>
    public interface IResourceServiceFactory {

        /// <summary>
        /// Renvoie le service de chargement.
        /// </summary>
        /// <returns>Service de chargement.</returns>
        IResourceLoader GetLoaderService();

        /// <summary>
        /// Renvoie le service de sauvegarde.
        /// </summary>
        /// <returns>Service de sauvegarde.</returns>
        IResourceWriter GetWriterService();
    }
}
