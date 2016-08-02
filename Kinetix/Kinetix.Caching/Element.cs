/*
 *  Copyright 2003-2007 Greg Luck
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System;
using System.Runtime.Serialization;

namespace Kinetix.Caching {
    /// <summary>
    /// Element stocké dans un cache.
    /// </summary>
    [Serializable]
    public class Element {

        /// <summary>
        /// Constante représentant une seconde.
        /// </summary>
        public const long OneSecond = 10000000;

        private object _key;
        private object _value;
        private long _creationTime;
        private bool? _isSerializable;
        private long _hitCount;
        private bool _lifespanSet;
        private long _nextToLastAccessTime;
        private long _lastAccessTime;
        private long _lastUpdateTime;
        private int _timeToLive;
        private int _timeToIdle;
        private bool _eternal;

        /// <summary>
        /// Crée un nouvelle élément de cache.
        /// </summary>
        /// <param name="key">Clef.</param>
        /// <param name="value">Valeur.</param>
        public Element(object key, object value) {
            _key = key;
            _value = value;
            _creationTime = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Retourne la date de création de l'élément.
        /// </summary>
        public long CreationTime {
            get {
                return _creationTime;
            }
        }

        /// <summary>
        /// Indique la date d'expiration de l'élément.
        /// </summary>
        public long ExpirationTime {
            get {
                if (!_lifespanSet || _eternal || (_timeToIdle == 0 && _timeToLive == 0)) {
                    return long.MaxValue;
                }

                long ttlExpiry = _creationTime + (_timeToLive * OneSecond);
                long mostRecentTime = (_creationTime > _nextToLastAccessTime) ? _creationTime : _nextToLastAccessTime;
                long ttiExpiry = mostRecentTime + (_timeToIdle * OneSecond);

                if (_timeToLive != 0 && (TimeToIdle == 0 || _lastAccessTime == 0)) {
                    return ttlExpiry;
                } else if (_timeToLive == 0) {
                    return ttiExpiry;
                } else if (ttlExpiry > ttiExpiry) {
                    return ttlExpiry;
                } else {
                    return ttiExpiry;
                }
            }
        }

        /// <summary>
        /// Retourne le nombre de hit de l'élément.
        /// </summary>
        public long HitCount {
            get {
                return _hitCount;
            }
        }

        /// <summary>
        /// Indique si l'élément a expiré.
        /// </summary>
        public bool IsExpired {
            get {
                if (!_lifespanSet) {
                    return false;
                }

                return DateTime.Now.Ticks > this.ExpirationTime;
            }
        }

        /// <summary>
        /// Indique si le contenu de l'élément est sérialisable.
        /// </summary>
        public bool IsSerializable {
            get {
                if (_isSerializable == null) {
                    _isSerializable = IsObjectSerializable(_key) && IsObjectSerializable(_value);
                }

                return _isSerializable.Value;
            }
        }

        /// <summary>
        /// Retourne la clef de l'élément.
        /// </summary>
        public object Key {
            get {
                return _key;
            }
        }

        /// <summary>
        /// Retourne la date de dernier accès à l'élément.
        /// </summary>
        public long LastAccessTime {
            get {
                return _lastAccessTime;
            }
        }

        /// <summary>
        /// The time when the last update occured. If this is the original Element, the time will be null.
        /// If there is an Element in the Cache and it is replaced with a new Element for the same key,
        /// then both the version number and lastUpdateTime should be updated to reflect that. The creation time
        /// will be the creation time of the new Element, not the original one, so that TTL concepts still work.
        /// </summary>
        public long LastUpdateTime {
            get {
                return _lastUpdateTime;
            }
        }

        /// <summary>
        /// Gets the next to last access time.
        /// </summary>
        public long NextToLastAccessTime {
            get {
                return _nextToLastAccessTime;
            }
        }

        /// <summary>
        /// Retourne la valeur de l'élément.
        /// </summary>
        public object Value {
            get {
                return _value;
            }
        }

        /// <summary>
        /// The time to live, in seconds.
        /// </summary>
        public int TimeToLive {
            get {
                return _timeToLive;
            }

            set {
                _timeToLive = value;
                _lifespanSet = true;
            }
        }

        /// <summary>
        /// The time to idle, in seconds.
        /// </summary>
        public int TimeToIdle {
            get {
                return _timeToIdle;
            }

            set {
                _timeToIdle = value;
                _lifespanSet = true;
            }
        }

        /// <summary>
        /// True if the element is eternal.
        /// </summary>
        public bool Eternal {
            get {
                return _eternal;
            }

            set {
                _eternal = value;
                _lifespanSet = true;
            }
        }

        /// <summary>
        /// Whether any combination of eternal, TTL or TTI has been set.
        /// </summary>
        internal bool IsLifespanSet {
            get {
                return _lifespanSet;
            }
        }

        /// <summary>
        /// Réinitialise les statistiques d'accès.
        /// </summary>
        internal void ResetAccessStatistics() {
            _lastAccessTime = 0;
            _nextToLastAccessTime = 0;
            _hitCount = 0;
        }

        /// <summary>
        /// Met à jour les statistiques d'accès à l'élément.
        /// </summary>
        internal void UpdateAccessStatistics() {
            _nextToLastAccessTime = _lastAccessTime;
            _lastAccessTime = DateTime.Now.Ticks;
            _hitCount++;
        }

        /// <summary>
        /// Incrément les statistiques de mise à jour.
        /// </summary>
        internal void UpdateUpdateStatistics() {
            _lastUpdateTime = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Indique si un objet est sérialisable.
        /// </summary>
        /// <param name="obj">Objet à vérifier.</param>
        /// <returns>True si l'objet est sérialisable.</returns>
        private static bool IsObjectSerializable(object obj) {
            return obj is ISerializable || obj is byte[] ||
                   obj.GetType().GetCustomAttributes(typeof(SerializableAttribute), false).Length == 1;
        }
    }
}
