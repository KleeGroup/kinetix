using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kinetix.ComponentModel;

namespace Kinetix.Audit {

    /// <summary>
    /// This class defines the Auditing Trace for an Object.
    /// </summary>
    /// 
    [Table("AUDIT_TRACE")]
    public partial class AuditTrace {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public AuditTrace() {
            this.OnCreated();
        }

        public AuditTrace(int? id, string category, string username, DateTime? businessDate, DateTime? executionDate, int? item, string message, string context) {
            this.Id = id;
            this.Category = category;
            this.Username = username;
            this.BusinessDate = businessDate;
            this.ExecutionDate = executionDate;
            this.Item = item;
            this.Message = message;
            this.Context = context;
        }

        #region Meta données

        /// <summary>
        /// Type énuméré présentant les noms des colonnes en base.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "A corriger")]
        public enum Cols {
            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            AUD_ID,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            AUD_CATEGORY,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            AUD_USERNAME,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            AUD_BUSINESS_DATE,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            AUD_EXECUTION_DATE,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            AUD_ITEM,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            AUD_MESSAGE,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            AUD_CONTEXT
        }

        #endregion


        [Column("AUD_ID")]
        [Domain("DO_X_AUDIT_ID")]
        [Key]
        public int? Id {
            get;
            set;
        }

        [Column("AUD_CATEGORY")]
        [Domain("DO_X_AUDIT_CATEGORY")]
        public string Category {
            get;
            set;
        }

        [Column("AUD_USERNAME")]
        [Domain("DO_X_AUDIT_USER")]
        public string Username {
            get;
            set;
        }

        [Column("AUD_BUSINESS_DATE")]
        [Domain("DO_X_AUDIT_DATE")]
        public DateTime? BusinessDate {
            get;
            set;
        }

        [Column("AUD_EXECUTION_DATE")]
        [Domain("DO_X_AUDIT_DATE")]
        public DateTime? ExecutionDate {
            get;
            set;
        }

        [Column("AUD_ITEM")]
        [Domain("DO_X_AUDIT_ITEM")]
        public int? Item {
            get;
            set;
        }

        [Column("AUD_MESSAGE")]
        [Domain("DO_X_AUDIT_MESSAGE")]
        public string Message {
            get;
            set;
        }

        [Column("AUD_CONTEXT")]
        [Domain("DO_X_AUDIT_CONTEXT")]
        public string Context {
            get;
            set;
        }

        /// <summary>
        /// Methode d'extensibilité possible pour les constructeurs.
        /// </summary>
        partial void OnCreated();

        /// <summary>
        /// Methode d'extensibilité possible pour les constructeurs par recopie.
        /// </summary>
        /// <param name="bean">Source.</param>
        partial void OnCreated(AuditTrace bean);
    }
}

