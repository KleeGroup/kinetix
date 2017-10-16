namespace Kinetix.ClassGenerator.MsBuild {

    /// <summary>
    /// Représente un item de projet MSBuild.
    /// </summary>
    public class ProjectItem {

        /// <summary>
        /// Chemin relatif de l'item par rapport au projet.
        /// </summary>
        public string ItemPath {
            get;
            set;
        }

        /// <summary>
        /// Build Action de l'item dans le projet.
        /// </summary>
        public string BuildAction {
            get;
            set;
        }
    }
}
