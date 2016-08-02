using System;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Compteur de type temps de réponse.
    /// On ajoute non pas une valeur mais un processus.
    /// On compte :
    /// - les temps de réponse inférieurs à 50 ms, 100ms etc...
    /// - les sous processus participant au processus.
    /// </summary>
    public sealed class CounterTime : CounterHit {
        /// <summary>
        /// Nombre de hits sous les 50ms.
        /// </summary>
        private long _hitsCounts50;

        /// <summary>
        /// Nombre de hits sous les 100ms.
        /// </summary>
        private long _hitsCounts100;

        /// <summary>
        /// Nombre de hits sous les 200ms.
        /// </summary>
        private long _hitsCounts200;

        /// <summary>
        /// Nombre de hits sous les 500ms.
        /// </summary>
        private long _hitsCounts500;

        /// <summary>
        /// Nombre de hits sous les 1000ms = 1s.
        /// </summary>
        private long _hitsCounts1000;

        /// <summary>
        /// Nombre de hits sous les 2000ms = 2s.
        /// </summary>
        private long _hitsCounts2000;

        /// <summary>
        /// Nombre de hits sous les 5000ms = 5s.
        /// </summary>
        private long _hitsCounts5000;

        /// <summary>
        /// Nombre de hits sous les 10000ms =10s.
        /// </summary>
        private long _hitsCounts10000;

        /// <summary>
        /// Temps passé dans les sous-processus.
        /// </summary>
        private long _subProcessTimes;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        internal CounterTime()
            : base(null) {
        }

        /// <summary>
        /// Retourne une valeur du compteur.
        /// </summary>
        /// <param name="statType">CounterStatType.</param>
        /// <returns>Valeur.</returns>
        public override double GetValue(CounterStatType statType) {
            double value;
            switch (statType) {
                case CounterStatType.Hits50:
                    value = _hitsCounts50;
                    break;
                case CounterStatType.Hits100:
                    value = _hitsCounts100;
                    break;
                case CounterStatType.Hits200:
                    value = _hitsCounts200;
                    break;
                case CounterStatType.Hits500:
                    value = _hitsCounts500;
                    break;
                case CounterStatType.Hits1000:
                    value = _hitsCounts1000;
                    break;
                case CounterStatType.Hits2000:
                    value = _hitsCounts2000;
                    break;
                case CounterStatType.Hits5000:
                    value = _hitsCounts5000;
                    break;
                case CounterStatType.Hits10000:
                    value = _hitsCounts10000;
                    break;
                case CounterStatType.SubAvg:
                    value = (Hits == 0) ? double.NaN : (_subProcessTimes / Hits);
                    break;
                default:
                    // Sinon les données statistiques sont gérés plus nativement.
                    value = base.GetValue(statType);
                    break;
            }

            return value;
        }

        /// <summary>
        /// Indique si la statistique gère des infos (min, max).
        /// </summary>
        /// <param name="statType">CounterStatType.</param>
        /// <returns>True si l'info est gérée.</returns>
        public override bool HasInfo(CounterStatType statType) {
            switch (statType) {
                case CounterStatType.Hits50:
                    return true;
                case CounterStatType.Hits100:
                    return true;
                case CounterStatType.Hits200:
                    return true;
                case CounterStatType.Hits500:
                    return true;
                case CounterStatType.Hits1000:
                    return true;
                case CounterStatType.Hits2000:
                    return true;
                case CounterStatType.Hits5000:
                    return true;
                case CounterStatType.Hits10000:
                    return true;
                case CounterStatType.SubAvg:
                    return true;
                default:
                    return base.HasInfo(statType);
            }
        }

        /// <summary>
        /// Ajoute une valeur.
        /// </summary>
        /// <param name="process">Processus.</param>
        /// <param name="name">Nom.</param>
        internal void AddValue(CounterProcess process, string name) {
            long processDuration = process.Duration;
            this.AddValue(processDuration, name);

            // Incrément
            _hitsCounts50 += processDuration <= 50 ? 1 : 0;
            _hitsCounts100 += processDuration <= 100 ? 1 : 0;
            _hitsCounts200 += processDuration <= 200 ? 1 : 0;
            _hitsCounts500 += processDuration <= 500 ? 1 : 0;
            _hitsCounts1000 += processDuration <= 1000 ? 1 : 0;
            _hitsCounts2000 += processDuration <= 2000 ? 1 : 0;
            _hitsCounts5000 += processDuration <= 5000 ? 1 : 0;
            _hitsCounts10000 += processDuration <= 10000 ? 1 : 0;

            // Sommes
            _subProcessTimes += process.SubProcessesDuration;
        }
    }
}
