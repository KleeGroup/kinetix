using System.Linq;

namespace Fmk.MsBuildCop.Core {

    /// <summary>
    /// Build Action dans un fichier MsBuild.
    /// </summary>
    public static class BuildAction {

        /// <summary>
        /// SSDT : prise en compte dans le modèle.
        /// </summary>
        public const string Build = "Build";

        /// <summary>
        /// C# : compilation.
        /// </summary>
        public const string Compile = "Compile";

        /// <summary>
        /// Contenu pris tel quel.
        /// </summary>
        public const string Content = "Content";

        /// <summary>
        /// C# : ressource intégrée dans la DLL.
        /// </summary>
        public const string EmbeddedResource = "EmbeddedResource";

        /// <summary>
        /// Dossier.
        /// </summary>
        public const string Folder = "Folder";

        /// <summary>
        /// Aucune action.
        /// </summary>
        public const string None = "None";

        /// <summary>
        /// SSDT : fichier postdeploy.
        /// </summary>
        public const string PostDeploy = "PostDeploy";

        /// <summary>
        /// SSDT : fichier predeploy.
        /// </summary>
        public const string PreDeploy = "PreDeploy";

        /// <summary>
        /// C# : référence vers un projet de la solution.
        /// </summary>
        public const string ProjectReference = "ProjectReference";

        private static readonly string[] ProjectBuildActions = {
            Build,
            Compile,
            Content,
            EmbeddedResource,
            PostDeploy,
            PreDeploy,
            None
        };

        /// <summary>
        /// Indique si l'action correspond à un fichier contenu dans le projet.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns><code>True</code> si le fichier est contenu dans le projet.</returns>
        public static bool IsProjectFile(string action) {
            if (string.IsNullOrEmpty(action)) {
                return false;
            }

            return ProjectBuildActions.Contains(action);
        }
    }
}
