using System;
using System.IO;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Kinetix.Tfs.Tools.Client {

    /// <summary>
    /// Client pour se connecter à TFS.
    /// ATTENTION : très lent si Visual Studio est allumé en même temps et branché sur le workspace de travail.
    /// </summary>
    public class TfsClient : IDisposable {

        private readonly string _collectionUrl;
        private readonly TfsTeamProjectCollection _projectCollection;
        private readonly VersionControlServer _vcs;
        private readonly Workspace _ws;

        /// <summary>
        /// Créé une nouvelle instance de TfsClient.
        /// </summary>
        /// <param name="collectionUrl">URL de la collection TFS.</param>
        /// <param name="workspace">Dossier du workspace local.</param>
        private TfsClient(string collectionUrl, string workspace) {
            _collectionUrl = collectionUrl;
            _projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(_collectionUrl));
            _vcs = (VersionControlServer)_projectCollection.GetService(typeof(VersionControlServer));
            _ws = _vcs.GetWorkspace(workspace);
        }

        /// <summary>
        /// Créé un client TFS.
        /// </summary>
        /// <param name="collectionUrl">URL de la collection TFS.</param>
        /// <param name="workspace">Dossier du workspace local.</param>
        /// <returns>Client TFS.</returns>
        public static TfsClient Connect(string collectionUrl, string workspace) {
            return new TfsClient(collectionUrl, workspace);
        }

        /// <summary>
        /// Ajoute un fichier dans les pending changes du workspace local.
        /// </summary>
        /// <param name="fileName">Chemin du fichier.</param>
        public void Add(string fileName) {
            var fi = new FileInfo(fileName);
            Console.WriteLine("Pend Add " + fi.Name + "...");
            _ws.PendAdd(fileName);
        }

        /// <summary>
        /// Met un fichier en édition dans les pending changes du workspace local.
        /// </summary>
        /// <param name="fileName">Chemin du fichier.</param>
        public void CheckOut(string fileName) {
            var fi = new FileInfo(fileName);
            Console.WriteLine("Pend Edit " + fi.Name + "...");
            _ws.PendEdit(fileName);
        }

        /// <summary>
        /// Dispose le projet.
        /// </summary>
        public void Dispose() {
            _projectCollection.Dispose();
        }
    }
}
