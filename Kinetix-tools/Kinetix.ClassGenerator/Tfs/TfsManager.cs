using Kinetix.Tfs.Tools.Client;

namespace Kinetix.ClassGenerator.Tfs {

    /// <summary>
    /// Singleton publiant une instance du client TFS.
    /// </summary>
    public static class TfsManager {

        /// <summary>
        /// Client TFS.
        /// </summary>
        public static TfsClient Client {
            get;
            set;
        }

        /// <summary>
        /// Met un fichier en édition dans les pending changes du workspace local.
        /// </summary>
        /// <param name="fileName">Chemin du fichier.</param>
        public static void CheckOut(string fileName) {
            Client.CheckOut(fileName);
        }
    }
}
