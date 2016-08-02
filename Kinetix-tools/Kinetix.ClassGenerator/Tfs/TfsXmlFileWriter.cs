namespace Kinetix.ClassGenerator.Tfs {

    /// <summary>
    /// Writer pour l'écriture de fichier avec entête TFS.
    /// Spécifique pour les fichiers XML (usage du token commentaire SQL).
    /// </summary>
    internal class TfsXmlFileWriter : TfsFileWriter {

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="fileName">Nom du fichier à écrire.</param>
        public TfsXmlFileWriter(string fileName)
            : base(fileName) {
        }

        /// <summary>
        /// Désactive l'entête car un fichier XML de configuration peut être modifié manuellement.
        /// </summary>
        protected override bool EnableHeader {
            get {
                return false;
            }
        }

        /// <summary>
        /// Non utilisé.
        /// </summary>
        /// <returns>Non utilisé.</returns>
        protected override string StartCommentToken {
            get {
                return null;
            }
        }
    }
}
