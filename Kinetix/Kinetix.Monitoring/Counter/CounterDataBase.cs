using System.Collections.Generic;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Gestion des valeurs des compteurs.
    /// Il s'agit d'une base de données alimentée uniquement par un événement de Monitoring.
    /// Toutes les méthodes d'accès public sont synchronisées.
    /// </summary>
    internal sealed class CounterDataBase {
        private readonly IWritableHyperCube _hyperCube;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="dbName">Nom de la base de données.</param>
        internal CounterDataBase(string dbName) {
            this._hyperCube = new HyperCubeRam(dbName);
            this.Reset();
        }

        /// <summary>
        /// Retourne l'hypercube associé à la base.
        /// </summary>
        internal IHyperCube HyperCube {
            get {
                return _hyperCube;
            }
        }

        /// <summary>
        /// Exécute le garbage.
        /// </summary>
        /// <param name="collection">Liste des cubes modifiés depuis le dernier garbage.</param>
        internal void RunStorage(ICollection<Cube> collection) {
            _hyperCube.RunStorage(collection);
        }

        /// <summary>
        /// Ajout d'un processus à la base.
        /// </summary>
        /// <param name="process">Processus.</param>
        /// <returns>Returne la durée du processus.</returns>
        internal long AddProcess(CounterProcess process) {
            process.Close();
            return _hyperCube.AddProcess(process);
        }

        /// <summary>
        /// Remet à zéro les compteurs.
        /// </summary>
        internal void Reset() {
            lock (this) {
                _hyperCube.Reset();
            }
        }
    }
}
