using System;
using System.ComponentModel.DataAnnotations;
using Kinetix.ComponentModel;


namespace Kinetix.Audit {

    /// <summary>
    /// Definit les domaines utilises par le module d'audit.
    /// Au démarrage de l'application, le type de cette classe doit être enregistré dans le DomainManager.
    /// </summary>
    [DomainMetadataType]
    public class AuditDomainMetadata {

        /// <summary>
        /// Domaine DO_X_AUDIT_ID.
        /// </summary>
        [Domain("DO_X_AUDIT_ID", ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "ErrorFormatEntier")]
        public int? Id { get; set; }

        /// <summary>
        /// Domaine DO_X_AUDIT_CATEGORY.
        /// </summary>
        [Domain("DO_X_AUDIT_CATEGORY")]
        [StringLength(20)]
        public string Category { get; set; }

        /// <summary>
        /// Domaine DO_X_AUDIT_USER.
        /// </summary>
        [Domain("DO_X_AUDIT_USER")]
        [StringLength(100)]
        public string Username { get; set; }

        /// <summary>
        /// Domaine DO_X_AUDIT_DATE.
        /// </summary>
        [Domain("DO_X_AUDIT_DATE")]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Domaine DO_X_AUDIT_ITEM.
        /// </summary>
        [Domain("DO_X_AUDIT_ITEM")]
        public int? Item { get; set; }

        /// <summary>
        /// Domaine DO_X_AUDIT_MESSAGE.
        /// </summary>
        [Domain("DO_X_AUDIT_MESSAGE")]
        [StringLength(250)]
        public string Message { get; set; }

        /// <summary>
        /// Domaine DO_X_AUDIT_CONTEXT.
        /// </summary>
        [Domain("DO_X_AUDIT_CONTEXT")]
        [StringLength(-1)]
        public string Context { get; set; }
    }
}