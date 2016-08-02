using System;
using System.Collections.Generic;
using Kinetix.Monitoring.Counter;

namespace Kinetix.Monitoring.Storage {
    /// <summary>
    /// Ensemble de base de données de monitoring.
    /// </summary>
    public class ExternalDatabaseSet {
        private readonly Dictionary<string, ExternalHyperCube> _map;

        /// <summary>
        /// Crée un nouvelle instance.
        /// </summary>
        /// <param name="counters">Liste de compteurs.</param>
        /// <param name="counterDefinitions">Définition des compteurs.</param>
        /// <param name="mergeTimeAxis">Indique si l'axe de temps doit être mergé.</param>
        public ExternalDatabaseSet(ICollection<CounterData> counters, ICollection<ICounterDefinition> counterDefinitions, bool mergeTimeAxis) {
            if (counters == null) {
                throw new ArgumentNullException("counters");
            }

            _map = new Dictionary<string, ExternalHyperCube>();

            ExternalHyperCube hyperCube;
            foreach (CounterData counter in counters) {
                string databaseName = counter.DatabaseName;
                if (!_map.TryGetValue(databaseName, out hyperCube)) {
                    hyperCube = new ExternalHyperCube(databaseName, counterDefinitions);
                    _map.Add(databaseName, hyperCube);
                }

                hyperCube.AddCounter(counter, mergeTimeAxis, DateTime.Now);
            }
        }

        /// <summary>
        /// Retourne la liste des bases de données.
        /// </summary>
        public ICollection<string> DatabaseNames {
            get {
                return _map.Keys;
            }
        }

        /// <summary>
        /// Retourne la base de données.
        /// </summary>
        /// <param name="databaseName">Nom de la base de données.</param>
        /// <returns>Base de données.</returns>
        public IHyperCube GetDatabase(string databaseName) {
            ExternalHyperCube hyperCube;
            _map.TryGetValue(databaseName, out hyperCube);
            return hyperCube;
        }
    }
}
