using System;

namespace Kinetix.Caching {
    /// <summary>
    /// Argument pour les évènements sur le cache.
    /// </summary>
    public class CacheEventArgs : EventArgs {
        private Element _element;
        private bool _remoteEvent;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="element">Elément source.</param>
        /// <param name="remoteEvent">Indique si l'évènement est distant.</param>
        public CacheEventArgs(Element element, bool remoteEvent) {
            this._element = element;
            this._remoteEvent = remoteEvent;
        }

        /// <summary>
        /// Obtient l'évènement source.
        /// </summary>
        public Element SourceElement {
            get {
                return _element;
            }
        }

        /// <summary>
        /// Indique si l'évènement est distant.
        /// </summary>
        public bool RemoteEvent {
            get {
                return _remoteEvent;
            }
        }
    }
}
