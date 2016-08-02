using System;
using System.Collections.Generic;
using Kinetix.Monitoring.Counter;

namespace Kinetix.Monitoring.Storage {
    /// <summary>
    /// API la base de stockage.
    /// </summary>
    public sealed class ExternalHyperCube : IHyperCube {

        private readonly string _name;
        private readonly Dictionary<string, string> _axis = new Dictionary<string, string>();
        private readonly Dictionary<string, ICounterDefinition> _counterDefinitions = new Dictionary<string, ICounterDefinition>();
        private readonly ICollection<ICounterDefinition> _counters;
        private readonly Dictionary<CubeKey, ICube> _cubes = new Dictionary<CubeKey, ICube>();

        /// <summary>
        /// Crée un nouvel hypercube à partir d'une collection de données.
        /// </summary>
        /// <param name="name">Nom de l'hypercube.</param>
        /// <param name="counters">Définition des compteurs.</param>
        public ExternalHyperCube(string name, ICollection<ICounterDefinition> counters) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            _counters = counters;
            _name = name;
        }

        /// <summary>
        /// Set de tous les axes (non triés).
        /// </summary>
        ICollection<string> IHyperCube.AllAxis {
            get {
                return _axis.Keys;
            }
        }

        /// <summary>
        /// Retourne la liste des définitions.
        /// </summary>
        ICollection<ICounterDefinition> IHyperCube.AllDefinitions {
            get {
                return (_counters != null) ? _counters : _counterDefinitions.Values;
            }
        }

        /// <summary>
        /// Retourne la liste des cubes de données.
        /// </summary>
        public ICollection<ICube> Cubes {
            get {
                return _cubes.Values;
            }
        }

        /// <summary>
        /// Nom de l'instance.
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
        /// Remet à zéro les compteurs.
        /// </summary>
        void IHyperCube.Reset() {
        }

        /// <summary>
        /// Retourne cube qui peut être null.
        /// </summary>
        /// <param name="key">CubeKey.</param>
        /// <returns>CounterCube.</returns>
        ICube IHyperCube.GetCube(CubeKey key) {
            ICube cube;
            _cubes.TryGetValue(key, out cube);
            return cube;
        }

        /// <summary>
        /// Ajoute un compteur.
        /// </summary>
        /// <param name="counter">Compteur.</param>
        /// <param name="mergeTimeAxis">Indique si l'axe de temps doit être mergé.</param>
        /// <param name="mergeDate">Date à utiliser pour le merge.</param>
        internal void AddCounter(CounterData counter, bool mergeTimeAxis, DateTime mergeDate) {
            // Alimentation du dictionnaire des axes.
            string axis = counter.Axis;
            if (!_axis.ContainsKey(axis)) {
                _axis.Add(axis, null);
            }

            if (_counters == null) {
                // Alimentation de la base de données des compteurs.
                string counterCode = counter.CounterCode;
                if (counterCode != null && !_counterDefinitions.ContainsKey(counterCode)) {
                    _counterDefinitions.Add(
                        counterCode,
                        new CounterDefinition(counterCode, counter.CounterLabel, 1, 0, 0));
                }
            }

            // Alimentation du cube de données.
            DateTime startDate = mergeTimeAxis ? mergeDate : counter.StartDate;
            CubeKey key = new CubeKey(startDate, axis, TimeLevel.ValueOf(counter.Level));
            ICube cube;
            if (!_cubes.TryGetValue(key, out cube)) {
                cube = new ExternalCube(_name, key);
                _cubes.Add(key, cube);
            }

            ((ExternalCube)cube).AddCounter(counter);
        }
    }
}
