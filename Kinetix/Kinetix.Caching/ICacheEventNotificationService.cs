namespace Kinetix.Caching {
    /// <summary>
    /// Interface de gestion des évènements pour les caches.
    /// </summary>
    public interface ICacheEventNotificationService {
        /// <summary>
        /// Indique si l'évènement est écouté.
        /// </summary>
        /// <returns>True.</returns>
        bool HasElementEvictedListeners();

        /// <summary>
        /// Indique si l'évènement est écouté.
        /// </summary>
        /// <returns>True.</returns>
        bool HasElementExpiredListeners();

        /// <summary>
        /// Notifie l'arrêt du cache.
        /// </summary>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void NotifyDisposed(bool remoteEvent);

        /// <summary>
        /// Notifie l'éviction d'un élément.
        /// </summary>
        /// <param name="element">Evicted Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void NotifyElementEvicted(Element element, bool remoteEvent);

        /// <summary>
        /// Notifie l'expiration d'un élément.
        /// </summary>
        /// <param name="element">Expiry Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void NotifyElementExpiry(Element element, bool remoteEvent);

        /// <summary>
        /// Notifie la mise à jour d'un élément.
        /// </summary>
        /// <param name="element">Updated Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void NotifyElementUpdate(Element element, bool remoteEvent);

        /// <summary>
        /// Notifie l'ajout d'un élément.
        /// </summary>
        /// <param name="element">Added Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void NotifyElementPut(Element element, bool remoteEvent);

        /// <summary>
        /// Notifie la suppression d'un élément.
        /// </summary>
        /// <param name="element">Removed Element.</param>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void NotifyElementRemoved(Element element, bool remoteEvent);

        /// <summary>
        /// Notifie la fin de l'écriture dans le cache disque.
        /// </summary>
        void NotifyBackupCompete();

        /// <summary>
        /// Notifie la suppression de tous les éléments.
        /// </summary>
        /// <param name="remoteEvent">Si l'évènement est distant.</param>
        void NotifyRemoveAll(bool remoteEvent);
    }
}
