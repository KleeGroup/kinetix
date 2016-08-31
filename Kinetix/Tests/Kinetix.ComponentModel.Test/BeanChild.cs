using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Classe de test.
    /// </summary>
    [DataContract]
    [Table("BEAN_CHILD")]
    [DefaultProperty("Libelle")]
    public class BeanChild : IOptimisticLocking {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public BeanChild() {
            this.RoleList = new List<string>();
        }

        /// <summary>
        /// Identifiant.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Column("BEA_ID")]
        [Key]
        [Domain("IDENTIFIANT")]
        public int? Id {
            get;
            set;
        }

        /// <summary>
        /// Identifiant.
        /// </summary>
        [DataMember(IsRequired = false)]
        [Column("OTH_ID")]
        [Domain("IDENTIFIANT")]
        [ReferencedType(typeof(Kinetix.ComponentModel.Test.Contract.Bean))]
        public int? OtherId {
            get;
            set;
        }

        /// <summary>
        /// Pourcentage.
        /// </summary>
        [DataMember(IsRequired = false)]
        [Column("BEA_PERCENT")]
        [Domain("POURCENTAGE")]
        public decimal? Pourcentage {
            get;
            set;
        }

        /// <summary>
        /// Libellé.
        /// </summary>
        [DataMember]
        [Column("BEA_LIBELLE")]
        [Domain("LIBELLE_COURT")]
        public string Libelle {
            get;
            set;
        }

        /// <summary>
        /// Libellé.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Column("BEA_LIBELLE_NOT_NULL")]
        [Domain("LIBELLE_COURT")]
        public string LibelleNotNull {
            get;
            set;
        }

        /// <summary>
        /// Date avec formateur.
        /// </summary>
        [DataMember]
        [Column("BEA_DATE")]
        [Domain("DATE")]
        public DateTime? Date {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la date de création.
        /// </summary>
        [DataMember(IsRequired = true)]
        [Column("BEA_DATE_MODIFICATION")]
        [Domain("DATE")]
        public DateTime? DateModif {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la date de création.
        /// </summary>
        public DateTime? DateCreation {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un int non signé.
        /// </summary>
        public uint? UintValue {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un long.
        /// </summary>
        public long? LongValue {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un long non signé.
        /// </summary>
        public ulong? UlongValue {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un short.
        /// </summary>
        public short? ShortValue {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un short non signé.
        /// </summary>
        public ushort? UshortValue {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un byte.
        /// </summary>
        public byte? ByteValue {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un byte signé.
        /// </summary>
        public sbyte? SbyteValue {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un bool.
        /// </summary>
        public bool? BoolValue {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un float.
        /// </summary>
        public float? FloatValue {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un double.
        /// </summary>
        public double? DoubleValue {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit un decimal.
        /// </summary>
        public decimal? DecimalValue {
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

        /// <summary>
        /// Liste de rôles.
        /// </summary>
        public ICollection<string> RoleList {
            get;
            set;
        }
    }
}
