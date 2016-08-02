namespace Kinetix.ClassGenerator.NVortex {
    /// <summary>
    /// Définit un message pour NVortex.
    /// </summary>
    public sealed class NVortexMessage {

        /// <summary>
        /// Code d'erreur.
        /// </summary>
        public string Code {
            get;
            set;
        }

        /// <summary>
        /// Nom du fichier associé.
        /// </summary>
        public string FileName {
            get;
            set;
        }

        /// <summary>
        /// Catégorie du message.
        /// </summary>
        public Category Category {
            get;
            set;
        }

        /// <summary>
        /// Description associée au message.
        /// </summary>
        public string Description {
            get;
            set;
        }

        /// <summary>
        /// Indique si le message est un message d'erreur.
        /// </summary>
        public bool IsError {
            get;
            set;
        }
    }
}
