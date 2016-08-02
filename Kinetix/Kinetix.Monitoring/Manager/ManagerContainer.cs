using System;
using System.Collections.Generic;

namespace Kinetix.Monitoring.Manager {
    /// <summary>
    /// Conteneur de tous les gestionnaires.
    /// </summary>
    public sealed class ManagerContainer {
        private static readonly ManagerContainer _instance = new ManagerContainer();
        private readonly List<IManager> _managerList = new List<IManager>();

        /// <summary>
        /// Retourne l'instance du container.
        /// </summary>
        public static ManagerContainer Instance {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Liste de tous les gestionnaires enregistrés.
        /// </summary>
        public ICollection<IManager> ManagerList {
            get {
                return new List<IManager>(_managerList);
            }
        }

        /// <summary>
        /// Récupère le listener par son nom.
        /// </summary>
        /// <param name="listenerClassName">Nom du listener.</param>
        /// <returns>Listener.</returns>
        public IManagerListener GetListener(string listenerClassName) {
            if (listenerClassName == null) {
                throw new ArgumentNullException("listenerClassName");
            }

            foreach (IManager manager in _managerList) {
                if (listenerClassName.Equals(manager.GetType().Name)) {
                    return (IManagerListener)manager;
                }
            }

            throw new NotSupportedException("Aucun manager trouvé");
        }

        /// <summary>
        /// Enregistrement d'un gestionnaire.
        /// </summary>
        /// <param name="manager">Gestionnaire.</param>
        public void RegisterManager(IManager manager) {
            if (manager == null) {
                throw new ArgumentNullException("manager");
            }

            _managerList.Add(manager);
        }

        /// <summary>
        /// Fermeture de tous les gestionnaires.
        /// </summary>
        public void Close() {
            foreach (IManager manager in _managerList) {
                manager.Close();
            }

            _managerList.Clear();
        }
    }
}
