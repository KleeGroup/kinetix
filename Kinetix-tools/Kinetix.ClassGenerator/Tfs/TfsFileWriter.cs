using System;
using System.IO;

namespace Kinetix.ClassGenerator.Tfs {

    /// <summary>
    /// Writer pour l'écriture de fichier avec entête TFS.
    /// L'écriture est réalisé à la fermeture du stream. Le fichier
    /// n'est écrit que si il est différent du fichier actuel (à l'entête près).
    /// Le programme Checkout automatiquement les fichiers uniquement s'ils sont différents (à l'entete près).
    /// </summary>
    internal abstract class TfsFileWriter : AbstractFileWriter {

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="fileName">Nom du fichier à écrire.</param>
        protected TfsFileWriter(string fileName)
            : base(fileName) {
        }

        /// <summary>
        /// Retourne le numéro de la ligne qui contient la version.
        /// </summary>
        protected override int VersionLine {
            get {
                return 3;
            }
        }

        /// <summary>
        /// Prépare le fichier pour une écriture.
        /// </summary>
        /// <param name="fileName">Nom du fichier.</param>
        protected override void PrepareFile(string fileName) {
            if (File.Exists(fileName)) {
                try {
                    TfsManager.CheckOut(fileName);
                } catch (Exception e) {
                    throw new NotSupportedException("Erreur lors du checkout TFS du fichier " + fileName + " : ", e);
                }
            }
        }

        /// <summary>
        /// Appelé après la création d'un nouveau fichier.
        /// </summary>
        /// <param name="fileName">Nom du fichier.</param>
        protected override void FinishFile(string fileName) {
            /* Ajoute le fichier dans TFS */
            TfsManager.Client.Add(fileName);
        }

        /// <summary>
        /// Ecrit le header du fichier.
        /// </summary>
        /// <param name="sw">Writer.</param>
        /// <param name="currentVersion">Numéro de version courant.</param>
        protected override void WriterHeader(StreamWriter sw, string currentVersion) {
        }
    }
}
