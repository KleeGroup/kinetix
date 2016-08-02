using System;
using System.Data.Linq;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Structure d'un fichier téléchargé.
    /// </summary>
    public sealed class DownloadedFile {
        /// <summary>
        /// Obtient ou définit la valeur de Fichier.
        /// </summary>
        public byte[] Fichier {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la valeur de FileName.
        /// </summary>
        public string FileName {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la valeur de DateCreation.
        /// </summary>
        public DateTime DateCreation {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la valeur de ContentType.
        /// </summary>
        public string ContentType {
            get;
            set;
        }

        /// <summary>
        /// Taille du fichier (en octets).
        /// </summary>
        public int Size {
            get;
            set;
        }

        /// <summary>
        /// Guid du fichier.
        /// </summary>
        public Guid Guid {
            get;
            set;
        }

        /// <summary>
        /// Etat du fichier.
        /// </summary>
        public ChangeAction State {
            get;
            set;
        }

        /// <summary>
        /// Indique si un fichier existe initialement.
        /// </summary>
        public bool IsFileExists {
            get;
            set;
        }
    }
}
