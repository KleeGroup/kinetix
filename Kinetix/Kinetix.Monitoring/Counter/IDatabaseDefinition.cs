namespace Kinetix.Monitoring.Counter {

    /// <summary>
    /// Définition d'une base de données de monitoring.
    /// </summary>
    public interface IDatabaseDefinition {
        /// <summary>
        /// Nom de la base de données.
        /// </summary>
        string Name {
            get;
        }

        /// <summary>
        /// Description de la base de données.
        /// </summary>
        string Description {
            get;
        }

        /// <summary>
        /// Type mime de l'image.
        /// </summary>
        string ImageMimeType {
            get;
        }

        /// <summary>
        /// Image associée à la base de données.
        /// </summary>
        byte[] ImageData {
            get;
        }

        /// <summary>
        /// Priorité d'affichage de la base de données.
        /// </summary>
        int Priority {
            get;
        }
    }
}
