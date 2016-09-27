using Kinetix.ComponentModel;
using Kinetix.ComponentModel.DataAnnotations;
using Kinetix.ComponentModel.Formatters;
using System;
using System.ComponentModel.DataAnnotations;


namespace Kinetix.Workflow
{
    /// <summary>
    /// Definit les domaines utilises par le workflow.
    /// Au démarrage de l'application, le type de cette classe doit être enregistré dans le DomainManager.
    /// </summary>
    [DomainMetadataType]
    public class WfDomainMetadata
    {
        /// <summary>
        /// Domaine DO_X_WORKFLOW_ID.
        /// </summary>
        [Domain("DO_X_WORKFLOW_ID", ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "ErrorFormatEntier")]
        public int? Id { get; set; }

        /// <summary>
        /// Domaine DO_X_WORKFLOW_CODE.
        /// </summary>
        [Domain("DO_X_WORKFLOW_CODE")]
        [StringLength(3)]
        public string Code { get; set; }

        /// <summary>
        /// Domaine DO_X_WORKFLOW_DATE.
        /// Format JJ/MM/AAAA.
        /// </summary>
        [Domain("DO_X_WORKFLOW_DATE")]
        [Date(0)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Domaine DO_X_WORKFLOW_WEAK_ID.
        /// </summary>
        [Domain("DO_X_WORKFLOW_WEAK_ID")]
        public int? WeakId { get; set; }

        /// <summary>
        /// Domaine DO_X_WORKFLOW_USER.
        /// </summary>
        [Domain("DO_X_WORKFLOW_USER")]
        [StringLength(50)]
        public string User { get; set; }

        /// <summary>
        /// Domaine DO_X_WORKFLOW_CHOICE.
        /// </summary>
        [Domain("DO_X_WORKFLOW_CHOICE")]
        public int? Choice { get; set; }

        /// <summary>
        /// Domaine DO_X_WORKFLOW_LABEL.
        /// </summary>
        [StringLength(100)]
        [Domain("DO_X_WORKFLOW_LABEL")]
        public string Label { get; set; }

        /// <summary>
        /// Domaine DO_X_WORKFLOW_COMMENTS.
        /// </summary>
        [StringLength(100)]
        [Domain("DO_X_WORKFLOW_COMMENTS")]
        public string Comments { get; set; }

        /// <summary>
        /// Domaine DO_X_WORKFLOW_FLAG.
        /// </summary>
        [Domain("DO_X_WORKFLOW_FLAG")]
        [CustomTypeConverter(typeof(FormatterBooleen))]
        public bool? Booleen { get; set; }

        /// <summary>
        /// Domaine DO_X_WORKFLOW_FLAG.
        /// </summary>
        [Domain("DO_X_WORKFLOW_LEVEL")]
        public int? Level { get; set; }

    }
}
