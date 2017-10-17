namespace Kinetix.ClassGenerator.Writer {

    /// <summary>
    /// Writer pour l'écriture de fichier.
    /// Spécifique pour les fichiers XML (usage du token commentaire SQL).
    /// </summary>
    internal class XmlFileWriter : FileWriter {

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="fileName">Nom du fichier à écrire.</param>
        public XmlFileWriter(string fileName)
            : base(fileName) {
        }

        /// <summary>
        /// Désactive l'entête car un fichier XML de configuration peut être modifié manuellement.
        /// </summary>
        protected override bool EnableHeader => false;

        /// <summary>
        /// Non utilisé.
        /// </summary>
        /// <returns>Non utilisé.</returns>
        protected override string StartCommentToken => null;
    }
}
