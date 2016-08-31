using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Bean sans clef primaire.
    /// </summary>
    [DataContract]
    [Table("BEAN")]
    public class BeanLibelle {
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
        /// Libellé.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Column("BEA_LIBELLE")]
        public string Libelle {
            get;
            set;
        }
    }
}
