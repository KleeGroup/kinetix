using System;
using System.Runtime.Serialization;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Clé idenditiant d'un cube.
    /// </summary>
    [Serializable]
    public sealed class CubeKey : ISerializable {

        // Axe temporel.
        private readonly TimeLevel _level;
        private readonly DateTime _dateKey;

        // Axe métier.
        private readonly string _axis;

        private readonly int _hashCode;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="date">Date de début du cube.</param>
        /// <param name="axis">Clé de l'axe fonctionnel.</param>
        /// <param name="level">Période de temps couverte par le cube.</param>
        internal CubeKey(DateTime date, string axis, TimeLevel level) {
            this._axis = axis;
            this._level = level;
            this._dateKey = level.GetDateKey(date);

            _hashCode = _axis.GetHashCode() + (5 * _level.GetHashCode()) + (7 * _dateKey.GetHashCode());
        }

        /// <summary>
        /// Crée une nouvelle à partir de la valeur sérialisée.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        private CubeKey(SerializationInfo info, StreamingContext context) {
            _axis = info.GetString("axis");
            _dateKey = info.GetDateTime("startDate");
            _level = TimeLevel.ValueOf(info.GetString("level"));
        }

        /// <summary>
        /// Gestion des axes.
        /// </summary>
        internal TimeLevel Level {
            get {
                return _level;
            }
        }

        /// <summary>
        /// Retourne la clef de l'axe temporel.
        /// </summary>
        internal DateTime DateKey {
            get {
                return _dateKey;
            }
        }

        /// <summary>
        /// Retourne la clef de l'axe métier.
        /// </summary>
        internal string Axis {
            get {
                return _axis;
            }
        }

        /// <summary>
        /// Test l'égalité de deux objets.
        /// </summary>
        /// <param name="obj">Objet à tester.</param>
        /// <returns>True si les deux objets sont égaux.</returns>
        public override bool Equals(object obj) {
            CubeKey cube = obj as CubeKey;
            if (cube != null) {
                return cube._axis.Equals(_axis) &&
                       cube._level.Equals(_level) &&
                       cube._dateKey.Equals(_dateKey);
            }

            return false;
        }

        /// <summary>
        /// Retourne une valeur de hash pour l'objet.
        /// </summary>
        /// <returns>HashCode.</returns>
        public override int GetHashCode() {
            return _hashCode;
        }

        /// <summary>
        /// Retourne une représentation texte de l'objet.
        /// </summary>
        /// <returns>Description.</returns>
        public override string ToString() {
            return _axis + ":" + _dateKey;
        }

        /// <summary>
        /// Sérialise l'objet.
        /// </summary>
        /// <param name="info">Information de sérialisation.</param>
        /// <param name="context">Contexte de sérialisation.</param>
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }

            info.AddValue("axis", _axis);
            info.AddValue("startDate", _dateKey);
            info.AddValue("level", _level.ToString());
        }
    }
}
