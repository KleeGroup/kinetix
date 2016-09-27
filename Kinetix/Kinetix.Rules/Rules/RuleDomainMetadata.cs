using System;
using System.ComponentModel.DataAnnotations;
using Kinetix.ComponentModel;
using Kinetix.ComponentModel.DataAnnotations;


namespace Kinetix.Rules {
    /// <summary>
    /// Definit les domaines utilises par les rulse.
    /// Au démarrage de l'application, le type de cette classe doit être enregistré dans le DomainManager.
    /// </summary>
    [DomainMetadataType]
    public class RuleDomainMetadata {
        /// <summary>
        /// Domaine DO_X_RULES_ID.
        /// </summary>
        [Domain("DO_X_RULES_ID", ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "ErrorFormatEntier")]
        public int? Id { get; set; }

        /// <summary>
        /// Domaine DO_X_RULES_CODE.
        /// </summary>
        [Domain("DO_X_RULES_CODE")]
        [StringLength(3)]
        public string Code { get; set; }

        /// <summary>
        /// Domaine DO_X_RULES_DATE.
        /// Format JJ/MM/AAAA.
        /// </summary>
        [Domain("DO_X_RULES_DATE")]
        [Date(0)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Domaine DO_X_RULES_WEAK_ID.
        /// </summary>
        [Domain("DO_X_RULES_WEAK_ID")]
        public int? WeakId { get; set; }

        /// <summary>
        /// Domaine DO_X_RULES_GROUP_ID.
        /// </summary>
        [StringLength(100)]
        [Domain("DO_X_RULES_GROUP_ID")]
        public string GroupId { get; set; }
        
        /// <summary>
        /// Domaine DO_X_RULES_LABEL.
        /// </summary>
        [StringLength(100)]
        [Domain("DO_X_RULES_LABEL")]
        public string Label { get; set; }

        /// <summary>
        /// Domaine DO_X_RULES_FIELD.
        /// </summary>
        [StringLength(50)]
        [Domain("DO_X_RULES_FIELD")]
        public string Field { get; set; }

        /// <summary>
        /// Domaine DO_X_RULES_OPERATOR.
        /// </summary>
        [StringLength(3)]
        [Domain("DO_X_RULES_OPERATOR")]
        public string Operator { get; set; }

        /// <summary>
        /// Domaine DO_X_RULES_EXPRESSION.
        /// </summary>
        [StringLength(100)]
        [Domain("DO_X_RULES_EXPRESSION")]
        public string Expression { get; set; }

    }
}
