using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Kinetix.Data.SqlClient.Test.DataContract {

    /// <summary>
    /// Bean incluant tous les types de données.
    /// </summary>
    [DataContract(Name = "BEAN")]
    [Table("BEAN")]
    public class Bean {

        /// <summary>
        /// Constante de test.
        /// </summary>
        public const string CINQ = "5";

        /// <summary>
        /// Type énuméré présentant le noms des colonnes en base.
        /// </summary>
        public enum Cols {

            /// <summary>
            /// Nom de colonne pour la propriété Pk.
            /// </summary>
            BEA_PK,

            /// <summary>
            /// Nom de colonne pour la propriété DataLong.
            /// </summary>
            BEA_LONG,

            /// <summary>
            /// Nom de colonne pour la propriété DataShort.
            /// </summary>
            BEA_SHORT,

            /// <summary>
            /// Nom de colonne pour la propriété DataGuid.
            /// </summary>
            BEA_GUID,

            /// <summary>
            /// Nom de colonne pour la propriété DataFloat.
            /// </summary>
            BEA_FLOAT,

            /// <summary>
            /// Nom de colonne pour la propriété DataDouble.
            /// </summary>
            BEA_DOUBLE,

            /// <summary>
            /// Nom de colonne pour la propriété DataDecimal.
            /// </summary>
            BEA_DECIMAL,

            /// <summary>
            /// Nom de colonne pour la propriété DataDatetime.
            /// </summary>
            BEA_DATETIME,

            /// <summary>
            /// Nom de colonne pour la propriété DataChars.
            /// </summary>
            BEA_CHARS,

            /// <summary>
            /// Nom de colonne pour la propriété DataChar.
            /// </summary>
            BEA_CHAR,

            /// <summary>
            /// Nom de colonne pour la propriété DataBytes.
            /// </summary>
            BEA_BYTES,

            /// <summary>
            /// Nom de colonne pour la propriété DataByte.
            /// </summary>
            BEA_BYTE,

            /// <summary>
            /// Nom de colonne pour la propriété DataBool.
            /// </summary>
            BEA_BOOL,

            /// <summary>
            /// Nom de colonne pour la propriété DataInt.
            /// </summary>
            BEA_INT,

            /// <summary>
            /// Nom de colonne pour la propriété DataString.
            /// </summary>
            BEA_STRING,
        }

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

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_LONG")]
        public long? DataLong {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_SHORT")]
        public short? DataShort {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_GUID")]
        public Guid? DataGuid {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_FLOAT")]
        public float? DataFloat {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_DOUBLE")]
        public double? DataDouble {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_DECIMAL")]
        public decimal? DataDecimal {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_DATETIME")]
        public DateTime? DataDateTime {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_CHARS")]
        public char[] DataChars {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_CHAR")]
        public char? DataChar {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_BYTES")]
        public byte[] DataBytes {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_BYTE")]
        public byte? DataByte {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_BOOL")]
        public bool? DataBool {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_INT")]
        public int? DataInt {
            get;
            set;
        }

        /// <summary>
        /// Data.
        /// </summary>
        [DataMember]
        [Column("BEA_STRING")]
        public string DataString {
            get;
            set;
        }

        /// <summary>
        /// Autre propriété.
        /// </summary>
        public string Other {
            get;
            set;
        }
    }
}
