using System;
using System.Collections;
using System.Collections.Generic;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// HyperCube mémoire.
    /// </summary>
    internal sealed class HyperCubeRam : IWritableHyperCube {
        private readonly Dictionary<string, HyperCubeRamCounters> _requestCountersMap = new Dictionary<string, HyperCubeRamCounters>();

        private readonly string _name;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="name">Nom de l'hypercube.</param>
        internal HyperCubeRam(string name) {
            this._name = name;
        }

        /// <summary>
        /// Retourne le nom de l'hypercube.
        /// </summary>
        string IHyperCube.Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// Indique si le cube peut être remis à zéro.
        /// </summary>
        bool IHyperCube.IsResetable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Set de tous les axes (non triés).
        /// </summary>
        ICollection<string> IHyperCube.AllAxis {
            get {
                lock (_requestCountersMap) {
                    return _requestCountersMap.Keys;
                }
            }
        }

        /// <summary>
        /// Retourne la liste des définitions.
        /// </summary>
        ICollection<ICounterDefinition> IHyperCube.AllDefinitions {
            get {
                ICollection counters = (ICollection)Analytics.Instance.DefinitionRepository.SortValues;
                ICounterDefinition[] array = new ICounterDefinition[counters.Count];
                counters.CopyTo(array, 0);
                return new List<ICounterDefinition>(array);
            }
        }

        /// <summary>
        /// Ajout d'un processus clôtûré à la base.
        /// </summary>
        /// <param name="process">Processus obligatoirement clôtûré.</param>
        /// <returns>Retourne la durée du processus.</returns>
        long IWritableHyperCube.AddProcess(CounterProcess process) {
            string axis = "global";
            ObtainCounters(axis).AddProcess(process);
            axis = process.Name;
            ObtainCounters(axis).AddProcess(process);

            return process.Duration;
        }

        /// <summary>
        /// Exécute le garbage.
        /// </summary>
        /// <param name="collection">Liste des cubes modifiés depuis le dernier garbage.</param>
        void IWritableHyperCube.RunStorage(ICollection<Cube> collection) {
            lock (_requestCountersMap) {
                foreach (HyperCubeRamCounters counter in _requestCountersMap.Values) {
                    counter.RunStorage(collection);
                }
            }
        }

        /// <summary>
        /// Retourne cube qui peut être null.
        /// </summary>
        /// <param name="key">CubeKey.</param>
        /// <returns>CounterCube.</returns>
        ICube IHyperCube.GetCube(CubeKey key) {
            if (key == null) {
                throw new ArgumentNullException("key");
            }

            HyperCubeRamCounters counters = GetCounters(key.Axis);
            return (counters == null) ? null : counters.GetCube(key);
        }

        /// <summary>
        /// Remet à zéro les compteurs.
        /// </summary>
        void IHyperCube.Reset() {
            lock (_requestCountersMap) {
                _requestCountersMap.Clear();
            }
        }

        /// <summary>
        /// Récupération des compteurs relatifs à une requête avec instanciation si nécessaire.
        /// </summary>
        /// <param name="axis">Nom de la requête.</param>
        /// <returns>Compteurs relatifs à une requête.</returns>
        private HyperCubeRamCounters ObtainCounters(string axis) {
            HyperCubeRamCounters requestCounters;
            lock (_requestCountersMap) {
                requestCounters = GetCounters(axis);
                if (requestCounters == null) {
                    requestCounters = new HyperCubeRamCounters(axis, _name);
                    _requestCountersMap.Add(axis, requestCounters);
                }
            }

            return requestCounters;
        }

        /// <summary>
        /// Récupération des compteurs relatifs à une requête sans instanciation.
        /// </summary>
        /// <param name="axis">Nom de la requête.</param>
        /// <returns>Compteurs relatifs à une requête ou null.</returns>
        private HyperCubeRamCounters GetCounters(string axis) {
            HyperCubeRamCounters counters;
            _requestCountersMap.TryGetValue(axis, out counters);
            return counters;
        }
    }
}
