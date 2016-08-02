using System;
using System.Collections.Generic;
using Kinetix.Monitoring.Storage;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Cube de données.
    /// Ce cube est placé sur deux axes :
    /// - un axe fonctionnel,
    /// - un axe temporel.
    /// Ces axes sont représentés par une clé.
    ///
    /// Si les données sont étendues alors les temps min, max sont aussi stockées.
    /// </summary>
    internal sealed class Cube : ICube {

        /// <summary>
        /// Format d'affichage des dates.
        /// </summary>
        private static readonly IFormatProvider DateFormatSecond = TimeLevel.Second.CreateFormatProvider();

        /// <summary>
        /// Clé identifiant du cube.
        /// </summary>
        private readonly CubeKey _key;

        // La String représente le code du compteur.
        private readonly Dictionary<string, CounterHit> _extendedMap;
        private readonly CounterTime _elapsedTimeCounter;
        private readonly string _databaseName;

        // Date des premiers et derniers hits.
        private long _firstMilliSeconds = -1;
        private long _lastMilliSeconds = -1;
        private bool _isExpired;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="key">Clé du cube.</param>
        /// <param name="databaseName">Nom de la base de données.</param>
        internal Cube(CubeKey key, string databaseName) {
            this._databaseName = databaseName;
            this._key = key;

            _elapsedTimeCounter = new CounterTime();

            // Etendu signifie que les statistiques sont collectées et agrégées aussi par compteur.
            if (_key.Level.IsExtended) {
                _extendedMap = new Dictionary<string, CounterHit>();
            } else {
                _extendedMap = null;
            }
        }

        /// <summary>
        /// Heure en millisecondes du premier évènement ajouté sur ce cube.
        /// </summary>
        public long FirstHitMsec {
            get {
                return _firstMilliSeconds;
            }
        }

        /// <summary>
        /// Heure en millisecondes du dernier évènement ajouté sur ce cube.
        /// </summary>
        public long LastHitMsec {
            get {
                return _lastMilliSeconds;
            }
        }

        /// <summary>
        /// Indique si le cube a été modifié.
        /// </summary>
        internal bool IsModified {
            get;
            set;
        }

        /// <summary>
        /// Indique si le cube n'est plus actif.
        /// </summary>
        internal bool IsExpired {
            get {
                if (_isExpired) {
                    return true;
                }

                if (_key.DateKey.AddSeconds(_key.Level.TimeStampInterval) > DateTime.Now) {
                    _isExpired = true;
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Retourne le nombre de hits du cube.
        /// </summary>
        internal double Hits {
            get {
                return _elapsedTimeCounter.GetValue(CounterStatType.Hits);
            }
        }

        /// <summary>
        /// Retourne les données du compteur temps.
        /// </summary>
        internal CounterData TimeData {
            get {
                CounterData data = new CounterData();

                InitCounterData(_elapsedTimeCounter, data, _key, _databaseName);
                data.SubAvg = _elapsedTimeCounter.GetValue(CounterStatType.SubAvg);

                int count;
                if ((count = (int)_elapsedTimeCounter.GetValue(CounterStatType.Hits50)) > 0) {
                    data.Sample.Add(new CounterSampleData() { SampleValue = 50, SampleCount = count });
                }

                if ((count = (int)_elapsedTimeCounter.GetValue(CounterStatType.Hits100)) > 0) {
                    data.Sample.Add(new CounterSampleData() { SampleValue = 100, SampleCount = count });
                }

                if ((count = (int)_elapsedTimeCounter.GetValue(CounterStatType.Hits200)) > 0) {
                    data.Sample.Add(new CounterSampleData() { SampleValue = 200, SampleCount = count });
                }

                if ((count = (int)_elapsedTimeCounter.GetValue(CounterStatType.Hits500)) > 0) {
                    data.Sample.Add(new CounterSampleData() { SampleValue = 500, SampleCount = count });
                }

                if ((count = (int)_elapsedTimeCounter.GetValue(CounterStatType.Hits1000)) > 0) {
                    data.Sample.Add(new CounterSampleData() { SampleValue = 1000, SampleCount = count });
                }

                if ((count = (int)_elapsedTimeCounter.GetValue(CounterStatType.Hits2000)) > 0) {
                    data.Sample.Add(new CounterSampleData() { SampleValue = 2000, SampleCount = count });
                }

                if ((count = (int)_elapsedTimeCounter.GetValue(CounterStatType.Hits5000)) > 0) {
                    data.Sample.Add(new CounterSampleData() { SampleValue = 5000, SampleCount = count });
                }

                if ((count = (int)_elapsedTimeCounter.GetValue(CounterStatType.Hits10000)) > 0) {
                    data.Sample.Add(new CounterSampleData() { SampleValue = 10000, SampleCount = count });
                }

                return data;
            }
        }

        /// <summary>
        /// Retourne le compteur d'une valeur.
        /// Valable uniquement si mode étendu.
        /// </summary>
        /// <param name="counterDefinitionCode">Code de la valeur.</param>
        /// <returns>Compteur.</returns>
        public ICounter GetCounter(string counterDefinitionCode) {
            if (Analytics.ElapsedTime.Equals(counterDefinitionCode)) {
                return _elapsedTimeCounter;
            }

            if (!_key.Level.IsExtended) {
                throw new NotSupportedException();
            }

            CounterHit counter;
            _extendedMap.TryGetValue(counterDefinitionCode, out counter);
            return counter;
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

            counterDataList.Add(this.TimeData);

            if (_extendedMap == null) {
                return;
            }

            foreach (string code in _extendedMap.Keys) {
                CounterHit counter = _extendedMap[code];
                CounterData data = new CounterData();
                data.CounterCode = code;
                data.CounterLabel = Analytics.Instance.DefinitionRepository.ValueOf(code).Label;
                InitCounterData(counter, data, _key, _databaseName);
                counterDataList.Add(data);
            }
        }

        /// <summary>
        /// Initialise les données à partir d'un compteur.
        /// </summary>
        /// <param name="counter">Compteur.</param>
        /// <param name="data">Objet de données.</param>
        /// <param name="key">Clef du cube.</param>
        /// <param name="databaseName">Nom de la base de données.</param>
        internal static void InitCounterData(ICounter counter, CounterData data, CubeKey key, string databaseName) {
            data.Axis = key.Axis;
            data.DatabaseName = databaseName;
            data.Hits = counter.GetValue(CounterStatType.Hits);
            data.Last = counter.GetValue(CounterStatType.Last);
            data.Level = (key.Level == TimeLevel.Minute) ? "MIN" : "HEU";
            data.Max = counter.GetValue(CounterStatType.Max);
            data.MaxName = counter.GetInfo(CounterStatType.Max);
            data.Min = counter.GetValue(CounterStatType.Min);
            data.MinName = counter.GetInfo(CounterStatType.Min);
            data.StartDate = key.DateKey;
            data.Total = counter.GetValue(CounterStatType.Total);
            data.TotalOfSquares = counter.GetValue(CounterStatType.TotalOfSquares);
        }

        /// <summary>
        /// Ajoute une nouvelle valeur.
        /// </summary>
        /// <param name="process">Processus.</param>
        internal void AddValue(CounterProcess process) {
            this.IsModified = true;

            string info;
            if ("global".Equals(_key.Axis)) {
                info = process.Name;
            } else {
                info = Convert.ToString(process.Date, DateFormatSecond);
            }

            _lastMilliSeconds = process.Date.Ticks / 10000;
            if (_firstMilliSeconds == -1) {
                _firstMilliSeconds = _lastMilliSeconds;
            }

            _elapsedTimeCounter.AddValue(process, info);

            if (_key.Level.IsExtended) {
                Dictionary<string, long> counters = process.Counters;
                foreach (string code in counters.Keys) {
                    CounterHit counter;
                    lock (_extendedMap) {
                        if (!_extendedMap.TryGetValue(code, out counter)) {
                            counter = new CounterHit(this);
                            _extendedMap.Add(code, counter);
                        }
                    }

                    counter.AddValue(counters[code], info);
                }
            }
        }
    }
}
