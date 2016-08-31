using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Classe de test.
    /// </summary>
    [DataContract]
    [Table("BEAN")]
    public class SimpleBean {
        /// <summary>
        /// Identifiant.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Column("BEA_ID")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Domain("IDENTIFIANT")]
        public int? Id {
            get;
            set;
        }

        /// <summary>
        /// Libellé.
        /// </summary>
        [DataMember]
        [Column("BEA_DESCRIPTION")]
        [Domain("LIBELLE_COURT")]
        public string Description {
            get;
            set;
        }
    }
}
