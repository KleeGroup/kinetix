using Kinetix.Monitoring.Manager;

namespace Kinetix.Monitoring.Counter {

    /// <summary>
    /// Définition d'une base de données.
    /// </summary>
    internal class DatabaseDefinition : IDatabaseDefinition {

        private readonly string _name;
        private readonly IManagerDescription _managerDescription;

        /// <summary>
        /// Crée une nouvelle définition de base de données associée à un manager.
        /// </summary>
        /// <param name="name">Nom de la base de données.</param>
        /// <param name="managerDescription">Description du manager.</param>
        internal DatabaseDefinition(string name, IManagerDescription managerDescription) {
            _managerDescription = managerDescription;
            _name = name;
        }

        /// <summary>
        /// Nom de la base de données.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// Description de la base de données.
        /// </summary>
        public string Description {
            get {
                if (_managerDescription == null) {
                    return null;
                }

                return _managerDescription.Name;
            }
        }

        /// <summary>
        /// Type mime de l'image.
        /// </summary>
        public string ImageMimeType {
            get {
                if (_managerDescription == null) {
                    return null;
                }

                return _managerDescription.ImageMimeType;
            }
        }

        /// <summary>
        /// Image associée à la base de données.
        /// </summary>
        public byte[] ImageData {
            get {
                if (_managerDescription == null) {
                    return null;
                }

                return _managerDescription.ImageData;
            }
        }

        /// <summary>
        /// Priorité d'affichage de la base de données.
        /// </summary>
        public int Priority {
            get {
                if (_managerDescription == null) {
                    return 0;
                }

                return _managerDescription.Priority;
            }
        }
    }
}
