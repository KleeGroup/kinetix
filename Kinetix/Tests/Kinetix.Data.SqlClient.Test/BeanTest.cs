using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Kinetix.Data.SqlClient.Test {

    /// <summary>
    /// Bean sans clef primaire.
    /// </summary>
    [DataContract]
    [Table("BEAN")]
    [DefaultProperty("Name")]
    public class BeanTest {

        /// <summary>
        /// Type énuméré présentant les noms de colonne en base.
        /// </summary>
        public enum Cols {
            /// <summary>
            /// Nom de la colonne en base pour la propriété Id.
            /// </summary>
            BEA_ID,

            /// <summary>
            /// Nom de la colonne en base pour la propriété Name.
            /// </summary>
            BEA_NAME,
        }

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
        /// Autre propriété sans attribut.
        /// </summary>
        public string OtherAttribut {
            get;
            set;
        }
    }
}