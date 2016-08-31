using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Kinetix.Data.SqlClient.Test {
    /// <summary>
    /// Bean incluant tous les types de données.
    /// </summary>
    [DataContract]
    [Table("BEAN")]
    public class PkBean {
        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_PK")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Pk {
            get;
            set;
        }
    }
}
