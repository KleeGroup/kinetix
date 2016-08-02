namespace Kinetix.Tfs.Tools.MsBuild {

    /// <summary>
    /// Actions de build pour un fichier dans un projet Visual Studio.
    /// </summary>
    public static class BuildActions {

        /// <summary>
        /// Action de build pour un fichier SQL de modèle SSDT.
        /// </summary>
        public const string Build = "Build";

        /// <summary>
        /// Action de compilation pour un fichier CSharp.
        /// </summary>
        public const string Compile = "Compile";

        /// <summary>
        /// Action none (aucune action directe).
        /// </summary>
        public const string None = "None";
    }
}
