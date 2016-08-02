namespace Kinetix.Worker {

    /// <summary>
    /// Classe permettant l'exécution d'une tâche simultannément sur plusieurs noeuds d'un cluster.
    /// </summary>
    public sealed class DistributedBackgroundWorker : AbstractWorker {

        /// <summary>
        /// Crée un nouveau job.
        /// </summary>
        /// <param name="name">Nom du job.</param>
        /// <param name="start">Paramétre de démarrage.</param>
        public DistributedBackgroundWorker(string name, ParameterizedWorkerStart start)
            : base(name, start) {
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
        /// <param name="parameter">Paramètre.</param>
        public void Start(object parameter) {
            this.DoStart(parameter);
        }

        /// <summary>
        /// Arrête le thread de travail.
        /// </summary>
        internal void Stop() {
            this.DoStop();
        }
    }
}
