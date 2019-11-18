using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Kinetix.ComponentModel;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Bean liste de référence avec traduction.
    /// </summary>
    [Reference]
    [Table("BEAN")]
    public class ReferenceBean {
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
        /// Libellé.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Translatable]
        [Column("BEA_LIBELLE")]
        public string Libelle {
            get;
            set;
        }
    }
}
