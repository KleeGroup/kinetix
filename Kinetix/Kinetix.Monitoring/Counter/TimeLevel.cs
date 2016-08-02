using System;
using System.Globalization;

namespace Kinetix.Monitoring.Counter {

    /// <summary>
    /// Niveaux heures, minutes ou secondes de la dimension hiérarchique temps.
    /// </summary>
    public sealed class TimeLevel {

        /// <summary>
        /// Heure.
        /// </summary>
        internal static readonly TimeLevel Hour = new TimeLevel("Hour", "HH", 3600, true);

        /// <summary>
        /// Minute.
        /// </summary>
        internal static readonly TimeLevel Minute = new TimeLevel("Minute", "HH:mm", 60, true);

        /// <summary>
        /// Seconde.
        /// </summary>
        internal static readonly TimeLevel Second = new TimeLevel("Second", "HH:mm:ss", 1, false);

        /// <summary>
        /// Durée de l'interval en secondes.
        /// </summary>
        private readonly int _period;
        private readonly bool _extended;
        private readonly string _name;
        private readonly string _format;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="name">Nom.</param>
        /// <param name="format">Format définissant le grain du niveau.</param>
        /// <param name="period">Durée de l'intervalle en secondes.</param>
        /// <param name="extended">Indique si des données étendues sont disponibles.</param>
        private TimeLevel(string name, string format, int period, bool extended) {
            this._extended = extended;
            this._format = format;
            this._name = name;
            this._period = period;
        }

        /// <summary>
        /// Indique si des données étendues sont disponibles.
        /// </summary>
        internal bool IsExtended {
            get {
                return _extended;
            }
        }

        /// <summary>
        /// Durée en secondes pour une heure ou une minute.
        /// </summary>
        internal int TimeStampInterval {
            get {
                return _period;
            }
        }

        /// <summary>
        /// Nom du TimeLevel.
        /// </summary>
        /// <returns>Nom du TimeLevel.</returns>
        public override string ToString() {
            return _name;
        }

        /// <summary>
        /// Retourne un instance de TimeLevel en fonction de la représentation texte.
        /// </summary>
        /// <param name="level">Nom du TimeLevel.</param>
        /// <returns>TimeLevel.</returns>
        internal static TimeLevel ValueOf(string level) {
            if (Hour._name.Equals(level) || "HEU".Equals(level)) {
                return Hour;
            } else if (Minute._name.Equals(level) || "MIN".Equals(level)) {
                return Minute;
            } else if (Second._name.Equals(level)) {
                return Second;
            } else {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// DateFormat précisant les heures, minutes ou secondes.
        /// </summary>
        /// <returns>Formateur.</returns>
        internal IFormatProvider CreateFormatProvider() {
            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.LongTimePattern = _format;
            return culture;
        }

        /// <summary>
        /// Retourne la date servant de clef pour la date courante.
        /// </summary>
        /// <param name="date">Date.</param>
        /// <returns>Date clef.</returns>
        internal DateTime GetDateKey(DateTime date) {
            if (this == TimeLevel.Hour) {
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
            } else if (this == TimeLevel.Minute) {
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
            } else if (this == TimeLevel.Second) {
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
            } else {
                throw new NotSupportedException();
            }
        }
    }
}
