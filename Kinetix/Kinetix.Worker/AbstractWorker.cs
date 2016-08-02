using System;
using System.Globalization;
using System.Threading;
using System.Web.Hosting;
using log4net;

namespace Kinetix.Worker {

    /// <summary>
    /// Classe d'abstaction d'un job sur IIS.
    /// </summary>
    public abstract class AbstractWorker : IRegisteredObject {

        private ManualResetEvent _stopping = new ManualResetEvent(false);
        private ParameterizedWorkerStart _start;
        private string _name;
        private Thread _workerThread = null;
        private bool _isRunning = false;

        /// <summary>
        /// Crée un nouveau job.
        /// </summary>
        /// <param name="name">Nom du job.</param>
        /// <param name="start">Paramétre de démarrage.</param>
        protected AbstractWorker(string name, ParameterizedWorkerStart start) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            if (start == null) {
                throw new ArgumentNullException("start");
            }

            _name = name;
            _start = start;
        }

        /// <summary>
        /// Obtient ou définit la culture du job.
        /// </summary>
        public CultureInfo CurrentCulture {
            get {
                return _workerThread.CurrentCulture;
            }

            set {
                _workerThread.CurrentCulture = value;
            }
        }

        /// <summary>
        /// Obtient ou définit la culture du job.
        /// </summary>
        public CultureInfo CurrentUICulture {
            get {
                return _workerThread.CurrentUICulture;
            }

            set {
                _workerThread.CurrentUICulture = value;
            }
        }

        /// <summary>
        /// Obtient ou définit la priorité du job.
        /// </summary>
        public string Name {
            get {
                return _workerThread.Name;
            }
        }

        /// <summary>
        /// Evènement signalant l'arrêt imminant.
        /// </summary>
        public WaitHandle StoppingEvent {
            get {
                return _stopping;
            }
        }

        /// <summary>
        /// Indique si le job est interruptible.
        /// </summary>
        protected virtual bool IsInterruptible {
            get {
                return true;
            }
        }

        /// <summary>
        /// Thread de travail.
        /// </summary>
        protected Thread WorkerThread {
            get {
                return _workerThread;
            }
        }

        /// <summary>
        /// Indique si le thread est en cours d'exécution.
        /// </summary>
        protected bool IsRunning {
            get {
                return _isRunning;
            }
        }

        /// <summary>
        /// Prend en charge la notification de l'arrêt de l'hôte.
        /// </summary>
        /// <param name="immediate">Indique si l'arrêt est immédiat.</param>
        void IRegisteredObject.Stop(bool immediate) {
            if (immediate) {
                return;
            }

            _stopping.Set();

            if (!this.IsInterruptible) {
                _workerThread.Join();
            }
        }

        /// <summary>
        /// Démarre le thread de travail.
        /// </summary>
        protected void DoStart() {
            this.DoStart(null);
        }

        /// <summary>
        /// Démarre le thread de travail.
        /// </summary>
        /// <param name="parameter">Paramétre.</param>
        protected void DoStart(object parameter) {
            HostingEnvironment.RegisterObject(this);

            _stopping.Reset();
            _workerThread = new Thread(new ParameterizedThreadStart(Run));
            _workerThread.Name = _name;
            _workerThread.Start(parameter);
        }

        /// <summary>
        /// Arrête le traitement.
        /// </summary>
        protected void DoStop() {
            _stopping.Set();
            _workerThread.Join();
        }

        /// <summary>
        /// Exécute le job.
        /// </summary>
        /// <param name="parameter">Paramètre.</param>
        private void Run(object parameter) {
            try {
                _isRunning = true;
                _start(this.StoppingEvent, parameter);
            } catch (Exception e) {
                ILog log = LogManager.GetLogger("Facturation.Application");
                if (log.IsErrorEnabled) {
                    log.Error("Erreur applicative", e);
                }
            } finally {
                HostingEnvironment.UnregisterObject(this);
                _isRunning = false;
            }
        }
    }
}
