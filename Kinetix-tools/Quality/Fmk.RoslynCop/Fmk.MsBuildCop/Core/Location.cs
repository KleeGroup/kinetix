namespace Fmk.MsBuildCop.Core {

    /// <summary>
    /// Localisation d'un diagnostic dans un fichier.
    /// </summary>
    public class Location {

        /// <summary>
        /// Chemin du fichier relativement au projet.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Index de la ligne de début.
        /// </summary>
        public int StartLine { get; set; }

        /// <summary>
        /// Index du premier caractère sur la première ligne.
        /// </summary>
        public int StartCharacter { get; set; }

        /// <summary>
        /// Index de la ligne de fin.
        /// </summary>
        public int EndLine { get; set; }

        /// <summary>
        /// Index du dernier caractère sur la dernière ligne.
        /// </summary>
        public int EndCharacter { get; set; }
    }
}
