namespace Kinetix.ClassGenerator.Tfs {

    /// <summary>
    /// Writer pour l'écriture d'un fichier javascript.
    /// Le fichier est mis en check out / add dans TFS.
    /// </summary>
    internal class TfsJsFileWriter : TfsFileWriter {

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="fileName">Nom du fichier à écrire.</param>
        public TfsJsFileWriter(string fileName)
            : base(fileName) {
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
    }
}
