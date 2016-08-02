using System;
using System.Collections.Generic;
using Kinetix.Monitoring.Counter;

namespace Kinetix.Monitoring.Storage {
    /// <summary>
    /// Cube de données.
    /// Ce cube est placé sur deux axes :
    /// - un axe fonctionnel,
    /// - un axe temporel.
    /// Ces axes sont représentés par une clé.
    ///
    /// Si les données sont étendues alors les temps min, max sont aussi stockées.
    /// </summary>
    internal sealed class ExternalCube : ICube {

        private readonly Dictionary<string, ExternalCounter> _counters = new Dictionary<string, ExternalCounter>();
        private readonly string _databaseName;
        private readonly CubeKey _key;
        private ExternalCounter _timeCounter;
        private DateTime _firstHit = DateTime.Now;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="databaseName">Nom de la base.</param>
        /// <param name="key">Clef.</param>
        internal ExternalCube(string databaseName, CubeKey key) {
            _databaseName = databaseName;
            _key = key;
        }

        /// <summary>
        /// Heure en millisecondes du premier évènement ajouté sur ce cube.
        /// </summary>
        long ICube.FirstHitMsec {
            get {
                return _firstHit.Ticks / 10000;
            }
        }

        /// <summary>
        /// Heure en millisecondes du dernier évènement ajouté sur ce cube.
        /// </summary>
        long ICube.LastHitMsec {
            get {
                return 0;
            }
        }

        /// <summary>
        /// Retourne le nombre de hits pour le cube.
        /// </summary>
        internal double Hits {
            get {
                return ((ICounter)_timeCounter).GetValue(CounterStatType.Hits);
            }
        }

        /// <summary>
        /// Ajoute tous les compteurs à la collection.
        /// </summary>
        /// <param name="moduleKey">Clef de module.</param>
        /// <param name="counterDataList">Listes des compteurs.</param>
        void ICube.ExportCounters(object moduleKey, ICollection<CounterData> counterDataList) {
            if (counterDataList == null) {
                throw new ArgumentNullException("counterDataList");
            }

            CounterData counterData;

            if (_timeCounter != null) {
                counterData = new CounterData();
                counterData.ModuleKey = moduleKey;
                Cube.InitCounterData(_timeCounter, counterData, _key, _databaseName);
                counterDataList.Add(counterData);
            }

            foreach (string code in _counters.Keys) {
                ExternalCounter counter = _counters[code];

                counterData = new CounterData();
                counterData.ModuleKey = moduleKey;
                counterData.CounterCode = code;
                Cube.InitCounterData(counter, counterData, _key, _databaseName);

                counterDataList.Add(counterData);
            }
        }

        /// <summary>
        /// Retourne le compteur d'une valeur.
        /// Valable uniquement si mode étendu.
        /// </summary>
        /// <param name="counterDefinitionCode">Code de la valeur.</param>
        /// <returns>Compteur.</returns>
        ICounter ICube.GetCounter(string counterDefinitionCode) {
            if (Analytics.ElapsedTime.Equals(counterDefinitionCode)) {
                return _timeCounter;
            } else {
                ExternalCounter counter;
                _counters.TryGetValue(counterDefinitionCode, out counter);
                return counter;
            }
        }

        /// <summary>
        /// Ajoute un compteur au cube.
        /// </summary>
        /// <param name="counter">Données.</param>
        internal void AddCounter(CounterData counter) {
            if (counter.StartDate < _firstHit) {
                _firstHit = counter.StartDate;
            }

            string counterCode = counter.CounterCode;
            if (counterCode == null) {
                if (_timeCounter == null) {
                    _timeCounter = new ExternalCounter(this, counter);
                } else {
                    _timeCounter.Merge(counter);
                }
            } else {
                ExternalCounter externalCounter;
                if (_counters.TryGetValue(counterCode, out externalCounter)) {
                    externalCounter.Merge(counter);
                } else {
                    _counters[counterCode] = new ExternalCounter(this, counter);
                }
            }
        }
    }
}
