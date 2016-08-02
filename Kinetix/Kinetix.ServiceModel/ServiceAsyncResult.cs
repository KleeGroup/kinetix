using System;
using System.Threading;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Résultat d'un appel asynchrone à un service.
    /// </summary>
    public sealed class ServiceAsyncResult : IAsyncResult, IDisposable {

        private readonly AsyncCallback _callback;
        private readonly object _state;
        private readonly ManualResetEvent _event;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="callback">Callback à appeler.</param>
        /// <param name="state">State.</param>
        public ServiceAsyncResult(AsyncCallback callback, object state) {
            _callback = callback;
            _state = state;
            _event = new ManualResetEvent(false);
        }

        /// <summary>
        /// Identifiant du workflow en charge du traitement.
        /// </summary>
        public Guid InstanceId {
            get;
            set;
        }

        /// <summary>
        /// Exception d'annulation.
        /// </summary>
        public Exception AbortException {
            get;
            private set;
        }

        /// <summary>
        /// Retourne les données du service.
        /// </summary>
        public object Data {
            get;
            private set;
        }

        /// <summary>
        /// Retourne l'état.
        /// </summary>
        object IAsyncResult.AsyncState {
            get {
                return _state;
            }
        }

        /// <summary>
        /// Retourne le handle d'attente.
        /// </summary>
        WaitHandle IAsyncResult.AsyncWaitHandle {
            get {
                return _event;
            }
        }

        /// <summary>
        /// Indique si le traitement se termine de manière synchrone.
        /// </summary>
        bool IAsyncResult.CompletedSynchronously {
            get {
                return false;
            }
        }

        /// <summary>
        /// Indique si le traitement est terminé.
        /// </summary>
        bool IAsyncResult.IsCompleted {
            get {
                return _event.WaitOne(0, false);
            }
        }

        /// <summary>
        /// Termine le traitement.
        /// </summary>
        /// <param name="data">Données liées.</param>
        public void Complete(object data) {
            Data = data;
            _event.Set();
            if (_callback != null) {
                _callback(this);
            }
        }

        /// <summary>
        /// Annule le traitement.
        /// </summary>
        /// <param name="exception">Exception.</param>
        public void Abort(Exception exception) {
            AbortException = exception;
            _event.Set();
            if (_callback != null) {
                _callback(this);
            }
        }

        /// <summary>
        /// Dispose l'objet.
        /// </summary>
        public void Dispose() {
            // Dispose any nested instances
            _event.Close();
            GC.SuppressFinalize(this);
        }
    }
}
