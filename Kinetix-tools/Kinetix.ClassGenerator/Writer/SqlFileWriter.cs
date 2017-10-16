using Kinetix.ClassGenerator.MsBuild;

namespace Kinetix.ClassGenerator.Writer {

    /// <summary>
    /// Writer pour l'écriture de fichier.
    /// Spécifique pour les fichiers SQL (usage du token commentaire SQL).
    /// </summary>
    internal class SqlFileWriter : FileWriter {

        private readonly string _sqlprojFileName;
        private readonly string _buildAction;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="fileName">Nom du fichier à écrire.</param>
        /// <param name="sqlprojFileName">Nom du fichier sqlproj.</param>
        /// <param name="buildAction">Action de build pour le fichier.</param>
        public SqlFileWriter(string fileName, string sqlprojFileName = null, string buildAction = null)
            : base(fileName) {
            _buildAction = buildAction;
            _sqlprojFileName = sqlprojFileName;
        }

        /// <summary>
        /// Renvoie le token de début de ligne de commentaire dans le langage du fichier.
        /// </summary>
        /// <returns>Toket de début de ligne de commentaire.</returns>
        protected override string StartCommentToken => "----";

        /// <summary>
        /// Appelé après la création d'un nouveau fichier.
        /// </summary>
        /// <param name="fileName">Nom du fichier.</param>
        protected override void FinishFile(string fileName) {
            if (string.IsNullOrEmpty(_sqlprojFileName)) {
                return;
            }

            /* Chemin relatif au csproj */
            string localFileName = ProjectFileUtils.GetProjectRelativeFileName(fileName, _sqlprojFileName);

            /* Met à jour le fichier csproj. */
            new ProjectUpdater()
                .AddItem(_sqlprojFileName, new ProjectItem { ItemPath = localFileName, BuildAction = _buildAction });
        }
    }
}
