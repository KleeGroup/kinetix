using Kinetix.Tfs.Tools.MsBuild;

namespace Kinetix.ClassGenerator.Tfs {

    /// <summary>
    /// Writer pour l'écriture d'un fichier CSharp.
    /// Le fichier est mis en check out / add dans TFS.
    /// Il est ajouté au fichier csproj fourni.
    /// </summary>
    internal class TfsCsharpFileWriter : TfsFileWriter {

        private readonly string _csprojFileName;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="fileName">Nom du fichier à écrire.</param>
        /// <param name="csprojFileName">Nom du fichier csproj.</param>
        public TfsCsharpFileWriter(string fileName, string csprojFileName = null)
            : base(fileName) {
            _csprojFileName = csprojFileName;
        }

        /// <summary>
        /// Renvoie le token de début de ligne de commentaire dans le langage du fichier.
        /// </summary>
        /// <returns>Toket de début de ligne de commentaire.</returns>
        protected override string StartCommentToken {
            get {
                return "////";
            }
        }

        /// <summary>
        /// Appelé après la création d'un nouveau fichier.
        /// </summary>
        /// <param name="fileName">Nom du fichier.</param>
        protected override void FinishFile(string fileName) {
            /* Ajoute le fichier dans TFS */
            base.FinishFile(fileName);

            if (string.IsNullOrEmpty(_csprojFileName)) {
                return;
            }

            /* Chemin relatif au csproj */
            string localFileName = ProjectFileUtils.GetProjectRelativeFileName(fileName, _csprojFileName);

            /* Met à jour le fichier csproj. */
            ProjectUpdater
                .Create(TfsManager.Client)
                .AddItem(_csprojFileName, new ProjectItem { ItemPath = localFileName, BuildAction = BuildActions.Compile });
        }
    }
}
