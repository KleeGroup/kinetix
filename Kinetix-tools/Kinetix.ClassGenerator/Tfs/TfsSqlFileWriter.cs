using Kinetix.Tfs.Tools.MsBuild;

namespace Kinetix.ClassGenerator.Tfs {

    /// <summary>
    /// Writer pour l'écriture de fichier avec entête TFS.
    /// Spécifique pour les fichiers SQL (usage du token commentaire SQL).
    /// </summary>
    internal class TfsSqlFileWriter : TfsFileWriter {

        private readonly string _sqlprojFileName;
        private readonly string _buildAction;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="fileName">Nom du fichier à écrire.</param>
        /// <param name="sqlprojFileName">Nom du fichier sqlproj.</param>
        /// <param name="buildAction">Action de build pour le fichier.</param>
        public TfsSqlFileWriter(string fileName, string sqlprojFileName = null, string buildAction = null)
            : base(fileName) {
            _buildAction = buildAction;
            _sqlprojFileName = sqlprojFileName;
        }

        /// <summary>
        /// Renvoie le token de début de ligne de commentaire dans le langage du fichier.
        /// </summary>
        /// <returns>Toket de début de ligne de commentaire.</returns>
        protected override string StartCommentToken {
            get {
                return "----";
            }
        }

        /// <summary>
        /// Appelé après la création d'un nouveau fichier.
        /// </summary>
        /// <param name="fileName">Nom du fichier.</param>
        protected override void FinishFile(string fileName) {
            /* Ajoute le fichier dans TFS */
            base.FinishFile(fileName);

            if (string.IsNullOrEmpty(_sqlprojFileName)) {
                return;
            }

            /* Chemin relatif au csproj */
            string localFileName = ProjectFileUtils.GetProjectRelativeFileName(fileName, _sqlprojFileName);

            /* Met à jour le fichier csproj. */
            ProjectUpdater
                .Create(TfsManager.Client)
                .AddItem(_sqlprojFileName, new ProjectItem { ItemPath = localFileName, BuildAction = _buildAction });
        }
    }
}
