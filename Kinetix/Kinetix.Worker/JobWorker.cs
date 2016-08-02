namespace Kinetix.Worker {

    /// <summary>
    /// Classe permettant l'exécution d'un job : tâche unique, non intérruptible, limitée dans le temps.
    /// </summary>
    public sealed class JobWorker : AbstractWorker {

        /// <summary>
        /// Crée un nouveau job.
        /// </summary>
        /// <param name="name">Nom du job.</param>
        /// <param name="start">Paramétre de démarrage.</param>
        public JobWorker(string name, ParameterizedWorkerStart start)
            : base(name, start) {
        }

        /// <summary>
        /// Indique si le job est interruptible.
        /// </summary>
        protected override bool IsInterruptible {
            get {
                return false;
            }
        }

        /// <summary>
        /// Démarre le thread de travail.
        /// </summary>
        public void Start() {
            this.DoStart();
        }

        /// <summary>
        /// Démarre le thread de travail.
        /// </summary>
        /// <param name="parameter">Paramétre.</param>
        public void Start(object parameter) {
            this.DoStart(parameter);
        }
    }
}
