using System;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Interface des objets exportables en mode delta.
    /// </summary>
    public interface IDeltaExportable {
        /// <summary>
        /// Obtient ou définit la date de dernière modification de l'objet.
        /// </summary>
        DateTime? DateModification {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la date de export.
        /// </summary>
        DateTime? DateEnvoi {
            get;
            set;
        }
    }
}
