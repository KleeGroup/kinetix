using System;
using System.Globalization;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Compteur représentant une grandeur sommable évoluant dans le temps.
    /// </summary>
    [Serializable]
    public class CounterHit : ICounter {
        [NonSerialized]
        private readonly Cube _cube;

        // Valeurs initialisées à 0.
        private long _hits;
        private long _total;
        private long _totalOfSquares;

        // Valeurs non initialisées.
        private long _last = -1;
        private long _min = -1;
        private long _max = -1;

        private string _minName;
        private string _maxName;

        /// <summary>
        /// Crée un nouveau compteur.
        /// </summary>
        /// <param name="cube">Cube contenant le compteur.</param>
        internal CounterHit(Cube cube) {
            _cube = cube;
        }

        /// <summary>
        /// Retourne le nombre de hits.
        /// </summary>
        protected double Hits {
            get {
                return _hits;
            }
        }

        /// <summary>
        /// Retourne le nom de l'événement ayant déclenché la valeur min ou max
        /// (null si non renseigné).
        /// </summary>
        /// <param name="statType">CounterStatType.</param>
        /// <returns>Nom de l'évènement.</returns>
        public string GetInfo(CounterStatType statType) {
            switch (statType) {
                case CounterStatType.Min:
                    return _minName;
                case CounterStatType.Max:
                    return _maxName;
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
        public virtual double GetValue(CounterStatType statType) {
            double v;
            switch (statType) {
                case CounterStatType.Last:
                    v = _last;
                    break;
                case CounterStatType.Min:
                    v = _min;
                    break;
                case CounterStatType.Max:
                    v = _max;
                    break;
                case CounterStatType.Total:
                    v = _total;
                    break;
                case CounterStatType.TotalOfSquares:
                    v = _totalOfSquares;
                    break;
                case CounterStatType.Hits:
                    v = _hits;
                    break;
                case CounterStatType.Avg:
                    long hits = (_cube == null) ? _hits : (long)_cube.Hits;
                    v = (hits > 0) ? ((double)_total) / hits : double.NaN;
                    break;
                default:
                    throw new NotSupportedException(string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionUnknowType,
                        statType));
            }

            return v;
        }

        /// <summary>
        /// Indique si la statistique gère des infos (min, max).
        /// </summary>
        /// <param name="statType">CounterStatType.</param>
        /// <returns>True si l'info est gérée.</returns>
        public virtual bool HasInfo(CounterStatType statType) {
            switch (statType) {
                case CounterStatType.Min:
                    return true;
                case CounterStatType.Max:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Ajout d'une valeur sur un événement.
        /// </summary>
        /// <param name="value">Valeur.</param>
        /// <param name="info">Info sur l'événement.</param>
        protected internal void AddValue(long value, string info) {
            _last = value;
            if (_hits == 0) {
                _min = value;
                _max = value;
                _minName = info;
                _maxName = info;
            } else {
                if (value < _min) {
                    _min = value;
                    _minName = info;
                }

                if (value > _max) {
                    _max = value;
                    _maxName = info;
                }
            }

            _total += value;
            _totalOfSquares += value * value;
            _hits++;
        }
    }
}
