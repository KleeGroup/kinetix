namespace Kinetix.ComponentModel {

    /// <summary>
    /// Représente un fichier présent sur un FileSystem.
    /// </summary>
    public sealed class FileSystemFile {

        /// <summary>
        /// Chemin complet du fichier.
        /// </summary>
        public string FilePath {
            get;
            set;
        }

        /// <summary>
        /// Clé de stockage du fichier.
        /// </summary>
        public string StorageKey {
            get;
            set;
        }
    }
}
