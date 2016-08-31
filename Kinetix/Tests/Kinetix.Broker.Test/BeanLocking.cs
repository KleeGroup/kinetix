using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Kinetix.ComponentModel;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Bean sans clef primaire.
    /// </summary>
    [DataContract]
    [Table("BEAN")]
    [DefaultProperty("Name")]
    public class BeanLocking : IOptimisticLocking {
        /// <summary>
        /// Identifiant.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Column("BEA_ID")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id {
            get;
            set;
        }

        /// <summary>
        /// Nom.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Column("BEA_NAME")]
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Numéro de version.
        /// </summary>
        public int? NumeroVersion {
            get;
            set;
        }

        /// <summary>
        /// Date de création.
        /// </summary>
        public DateTime? DateCreation {
            get;
            set;
        }

        /// <summary>
        /// Date de modification.
        /// </summary>
        public DateTime? DateModif {
            get;
            set;
        }

        /// <summary>
        /// Utilisateur créateur.
        /// </summary>
        public int? UtilisateurIdCreation {
            get;
            set;
        }

        /// <summary>
        /// Utilisateur modificateur.
        /// </summary>
        public int? UtilisateurIdModificateur {
            get;
            set;
        }
    }
}
