using System;
using System.Collections.Generic;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Représente un ensemble de grandeurs évoluant dans le temps.
    /// </summary>
    internal sealed class HyperCubeRamCounters {
        private readonly HyperCubeRamCounterDataStore _minuteGrapheur;
        private readonly HyperCubeRamCounterDataStore _heureGrapheur;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="axis">Nom de l'ensemble des compteurs.</param>
        /// <param name="databaseName">Nom de la base de données.</param>
        internal HyperCubeRamCounters(string axis, string databaseName) {
            _minuteGrapheur = new HyperCubeRamCounterDataStore(axis, databaseName, TimeLevel.Minute, 12);
            _heureGrapheur = new HyperCubeRamCounterDataStore(axis, databaseName, TimeLevel.Hour, 2);
        }

        /// <summary>
        /// Ajoute un processus au compteur.
        /// </summary>
        /// <param name="process">Processus.</param>
        internal void AddProcess(CounterProcess process) {
            _minuteGrapheur.AddValue(process);
            _heureGrapheur.AddValue(process);
        }

        /// <summary>
        /// Exécute le garbage.
        /// </summary>
        /// <param name="collection">Liste des cubes modifiés depuis le dernier garbage.</param>
        internal void RunStorage(ICollection<Cube> collection) {
            _minuteGrapheur.RunStorage(collection);
        }

        /// <summary>
        /// Retourne le cube pour une clef.
        /// </summary>
        /// <param name="key">Clef.</param>
        /// <returns>Cube.</returns>
        internal Cube GetCube(CubeKey key) {
            return GetCounterDataStore(key.Level).GetCube(key);
        }

        /// <summary>
        /// Retourne le datastore associé à un level.
        /// </summary>
        /// <param name="level">Unité de temps.</param>
        /// <returns>Datastore.</returns>
        private HyperCubeRamCounterDataStore GetCounterDataStore(TimeLevel level) {
            if (level == TimeLevel.Hour) {
                return _heureGrapheur;
            } else if (level == TimeLevel.Minute) {
                return _minuteGrapheur;
            } else {
                throw new NotSupportedException();
            }
        }
    }
}
