using System;
using System.Collections.Generic;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Chaque action génère un événement destiné au monitoring.
    /// Cet événement est évalué selon plusieurs axes correspondant aux compteurs.
    ///
    /// Exemple :
    /// Telle requête aura :
    /// - une taille de page
    /// - un temps de réponse
    /// - un nombre d'accès à la bdd
    /// - un nombre de mails envoyés.
    /// </summary>
    internal sealed class CounterProcess {
        /// <summary>
        /// Nom de l'action.
        /// </summary>
        private string _name;

        /// <summary>
        /// Début du processus.
        /// </summary>
        private long _start;

        /// <summary>
        /// Durée du processus.
        /// </summary>
        private long _duration;

        /// <summary>
        /// Date de fin du processus.
        /// La date est renseignée lorsque le processus est cloturé.
        /// Après clôture aucune action n'est possible.
        /// </summary>
        private DateTime _date;

        /// <summary>
        /// Compteurs relatifs à cette action.
        /// Code du compteur / valeur.
        /// </summary>
        private Dictionary<string, long> _map = new Dictionary<string, long>();

        /// <summary>
        /// Evénement parent.
        /// </summary>
        private CounterProcess _parent;

        /// <summary>
        /// Durée des sous processus.
        /// </summary>
        private long _subProcessesDuration;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="name">Nom du processus.</param>
        internal CounterProcess(string name) {
            this._name = name;
            _start = DateTime.Now.Ticks / 10000;
        }

        /// <summary>
        /// Date de fin du processus.
        /// </summary>
        internal DateTime Date {
            get {
                return _date;
            }
        }

        /// <summary>
        /// Durée du processus.
        /// </summary>
        internal long Duration {
            get {
                return _duration;
            }
        }

        /// <summary>
        /// Nom du processus.
        /// </summary>
        internal string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// Processus parent ou null si aucun.
        /// </summary>
        internal CounterProcess Parent {
            get {
                return _parent;
            }
        }

        /// <summary>
        /// Durée des sous processus.
        /// </summary>
        internal long SubProcessesDuration {
            get {
                return _subProcessesDuration;
            }
        }

        /// <summary>
        /// Retourne les compteurs du processus.
        /// </summary>
        internal Dictionary<string, long> Counters {
            get {
                return _map;
            }
        }

        /// <summary>
        /// Fermeture du processus.
        /// </summary>
        internal void Close() {
            this._date = DateTime.Now;
            _duration = (_date.Ticks / 10000) - _start;
            this.Flush();
        }

        /// <summary>
        /// Création d'un sous-processus.
        /// </summary>
        /// <param name="subProcessName">Nom du sous processus.</param>
        /// <returns>Sous-processus.</returns>
        internal CounterProcess CreateSubProcess(string subProcessName) {
            CounterProcess child = new CounterProcess(subProcessName);
            child._parent = this;
            return child;
        }

        /// <summary>
        /// Incrémente le compteur.
        /// </summary>
        /// <param name="counterDefinitionCode">Compteur.</param>
        /// <param name="value">Increment du compteur.</param>
        internal void IncValue(string counterDefinitionCode, long value) {
            long newValue = GetValue(counterDefinitionCode) + value;
            _map[counterDefinitionCode] = newValue;
        }

        /// <summary>
        /// Récupère la valeur du compteur.
        /// </summary>
        /// <param name="counterDefinitionCode">Compteur.</param>
        /// <returns>Valeur courante du compteur.</returns>
        private long GetValue(string counterDefinitionCode) {
            long value;
            _map.TryGetValue(counterDefinitionCode, out value);
            return value;
        }

        /// <summary>
        /// Flush le processus.
        /// C'est a dire que l'on ajoute les données du processus à son parent si ce dernier existe.
        /// </summary>
        private void Flush() {
            if (_parent != null) {
                // On ajoute le processus à son parent direct
                _parent._subProcessesDuration += _duration;

                foreach (string key in _map.Keys) {
                    _parent.IncValue(key, _map[key]);
                }
            }
        }
    }
}
