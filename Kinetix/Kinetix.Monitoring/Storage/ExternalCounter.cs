using System;
using System.Collections.Generic;
using System.Globalization;
using Kinetix.Monitoring.Counter;

namespace Kinetix.Monitoring.Storage {
    /// <summary>
    /// Compteur représentant une grandeur sommable évoluant dans le temps.
    /// </summary>
    internal sealed class ExternalCounter : ICounter {

        private readonly ExternalCube _cube;
        private readonly CounterData _counter;
        private readonly Dictionary<double, CounterSampleData> _sampleData;

        /// <summary>
        /// Crée un nouveau compteur.
        /// </summary>
        /// <param name="cube">Cube contenant le compteur.</param>
        /// <param name="counter">Données du compteur.</param>
        internal ExternalCounter(ExternalCube cube, CounterData counter) {
            _counter = counter;
            _cube = cube;

            // Indexation de l'échantillonnage.
            _sampleData = new Dictionary<double, CounterSampleData>();
            foreach (CounterSampleData data in counter.Sample) {
                _sampleData[data.SampleValue] = data;
            }
        }

        /// <summary>
        /// Retourne le nom de l'événement ayant déclenché la valeur min ou max
        /// (null si non renseigné).
        /// </summary>
        /// <param name="statType">CounterStatType.</param>
        /// <returns>Nom de l'évènement.</returns>
        string ICounter.GetInfo(CounterStatType statType) {
            switch (statType) {
                case CounterStatType.Min:
                    return _counter.MinName;
                case CounterStatType.Max:
                    return _counter.MaxName;
                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionUnknowType,
                        statType));
            }
        }

        /// <summary>
        /// Retourne une valeur du compteur.
        /// </summary>
        /// <param name="statType">CounterStatType.</param>
        /// <returns>Valeur.</returns>
        double ICounter.GetValue(CounterStatType statType) {
            switch (statType) {
                case CounterStatType.Last:
                    return _counter.Last;
                case CounterStatType.Min:
                    return _counter.Min;
                case CounterStatType.Max:
                    return _counter.Max;
                case CounterStatType.Total:
                    return _counter.Total;
                case CounterStatType.TotalOfSquares:
                    return _counter.TotalOfSquares;
                case CounterStatType.Hits:
                    return _counter.Hits;
                case CounterStatType.Avg:
                    double hits = (_cube == null) ? _counter.Hits : _cube.Hits;
                    return _counter.Total / hits;
                default:
                    return GetTimeValue(statType);
            }
        }

        /// <summary>
        /// Indique si la statistique gère des infos (min, max).
        /// </summary>
        /// <param name="statType">CounterStatType.</param>
        /// <returns>True si l'info est gérée.</returns>
        bool ICounter.HasInfo(CounterStatType statType) {
            switch (statType) {
                case CounterStatType.SubAvg:
                case CounterStatType.Min:
                case CounterStatType.Max:
                    return true;
                case CounterStatType.Hits50:
                case CounterStatType.Hits100:
                case CounterStatType.Hits200:
                case CounterStatType.Hits500:
                case CounterStatType.Hits1000:
                case CounterStatType.Hits2000:
                case CounterStatType.Hits5000:
                case CounterStatType.Hits10000:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Merge les données d'un compteur avec le compteur courant.
        /// </summary>
        /// <param name="counter">Compteur à merger.</param>
        internal void Merge(CounterData counter) {
            if (counter.StartDate > _counter.StartDate) {
                _counter.StartDate = counter.StartDate;
                _counter.Last = counter.Last;
            }

            if (_counter.Min > counter.Min) {
                _counter.Min = counter.Min;
                _counter.MinName = counter.MinName;
            }

            if (_counter.Max < counter.Max) {
                _counter.Max = counter.Max;
                _counter.MaxName = counter.MaxName;
            }

            _counter.Total += counter.Total;
            _counter.TotalOfSquares += counter.TotalOfSquares;
            _counter.Hits += counter.Hits;

            // Indexation des données d'échantillonnage.
            foreach (CounterSampleData data in counter.Sample) {
                CounterSampleData existingData;
                if (_sampleData.TryGetValue(data.SampleValue, out existingData)) {
                    existingData.SampleCount += data.SampleCount;
                } else {
                    _sampleData[data.SampleValue] = data;
                }
            }
        }

        /// <summary>
        /// Retourne une valeur du compteur d'un compteur de temps.
        /// </summary>
        /// <param name="statType">CounterStatType.</param>
        /// <returns>Valeur.</returns>
        private double GetTimeValue(CounterStatType statType) {
            switch (statType) {
                case CounterStatType.Hits50:
                    return this.GetSampleCount(50);
                case CounterStatType.Hits100:
                    return this.GetSampleCount(100);
                case CounterStatType.Hits200:
                    return this.GetSampleCount(200);
                case CounterStatType.Hits500:
                    return this.GetSampleCount(500);
                case CounterStatType.Hits1000:
                    return this.GetSampleCount(1000);
                case CounterStatType.Hits2000:
                    return this.GetSampleCount(2000);
                case CounterStatType.Hits5000:
                    return this.GetSampleCount(5000);
                case CounterStatType.Hits10000:
                    return this.GetSampleCount(10000);
                case CounterStatType.SubAvg:
                    return _counter.SubAvg;
                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionUnknowType,
                        statType));
            }
        }

        /// <summary>
        /// Retourne le nombre de hits pour une valeur d'échantillonnage.
        /// </summary>
        /// <param name="sampleValue">Valeur d'échantillonnage.</param>
        /// <returns>Nombre de hits.</returns>
        private double GetSampleCount(double sampleValue) {
            CounterSampleData data;
            if (_sampleData.TryGetValue(sampleValue, out data)) {
                return data.SampleCount;
            }

            return 0;
        }
    }
}
