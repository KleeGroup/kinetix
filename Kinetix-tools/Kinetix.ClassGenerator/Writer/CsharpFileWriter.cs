using Kinetix.ClassGenerator.MsBuild;

namespace Kinetix.ClassGenerator.Writer {

    /// <summary>
    /// Writer pour l'écriture d'un fichier CSharp.
    /// Il est ajouté au fichier csproj fourni.
    /// </summary>
    internal class CsharpFileWriter : FileWriter {

        private readonly string _csprojFileName;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="fileName">Nom du fichier à écrire.</param>
        /// <param name="csprojFileName">Nom du fichier csproj.</param>
        public CsharpFileWriter(string fileName, string csprojFileName = null)
            : base(fileName) {
            _csprojFileName = csprojFileName;
        }

        /// <summary>
        /// Appelé après la création d'un nouveau fichier.
        /// </summary>
        /// <param name="fileName">Nom du fichier.</param>
        protected override void FinishFile(string fileName) {
            /* Ajoute le fichier dans TFS */
            base.FinishFile(fileName);

            if (string.IsNullOrEmpty(_csprojFileName) || GeneratorParameters.IsNewCsproj) {
                return;
            }

            /* Chemin relatif au csproj */
            string localFileName = ProjectFileUtils.GetProjectRelativeFileName(fileName, _csprojFileName);

            /* Met à jour le fichier csproj. */
            new ProjectUpdater()
                .AddItem(_csprojFileName, new ProjectItem { ItemPath = localFileName, BuildAction = BuildActions.Compile });
        }
    }
}
