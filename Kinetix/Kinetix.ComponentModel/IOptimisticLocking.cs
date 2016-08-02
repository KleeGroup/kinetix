using System;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Interface de toutes les classes supportant le lock optimiste.
    /// </summary>
    public interface IOptimisticLocking {
        /// <summary>
        /// Obtient ou définit la valeur de NumeroVersion.
        /// Numéro de version technique.
        /// </summary>
        int? NumeroVersion {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la valeur de DateCreation.
        /// Date de création de l'oeuvre.
        /// </summary>
        DateTime? DateCreation {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la valeur de DateModif.
        /// Date de dernière modification de l'oeuvre.
        /// </summary>
        DateTime? DateModif {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la valeur de UtilisateurIdCreateur.
        /// Utilisateur ayant effectué la création.
        /// </summary>
        int? UtilisateurIdCreation {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la valeur de UtilisateurIdModificateur.
        /// Utilisateur ayant effectué la dernière modification.
        /// </summary>
        int? UtilisateurIdModificateur {
            get;
            set;
        }
    }
}
