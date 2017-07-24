using Kinetix.ComponentModel;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Kinetix.Workflow
{
    /// <summary>
    /// This class defines the Activity.
    /// </summary>
    /// 
    [Table("WF_ACTIVITY")]
    public class WfActivity
    {
        /// <summary>
        /// Primary key for WfActivity
        /// </summary>
        [Column("WFA_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public int? WfaId
        {
            get;
            set;
        }

        /// <summary>
        /// The creation date of this activity
        /// </summary>
        [Column("CREATION_DATE")]
        [Domain("DO_X_WORKFLOW_DATE")]
        public DateTime? CreationDate
        {
            get;
            set;
        }

        /// <summary>
        /// The foreign key to the workflow instance
        /// </summary>
        [Column("WFW_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int WfwId
        {
            get;
            set;
        }

        /// <summary>
        /// The foreign key to the activity definition
        /// </summary>
        [Column("WFAD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int WfadId
        {
            get;
            set;
        }


        /// <summary>
        /// Flag IsAuto. 
        /// Internal flag computed to know if this activity was auto in the last validation 
        /// or in the last recalculation. 
        /// Due to the lazy nature of the recalculation, this flag will be correctly computed 
        /// only if there is/was a decision linked to it.
        /// For the previous reason, you should not use this flag in the specific code.
        /// </summary>
        [Column("IS_AUTO")]
        [Domain("DO_X_WORKFLOW_FLAG")]
        public bool IsAuto
        {
            get;
            set;
        }


        /// <summary>
        /// Flag IsValid.
        /// Internal flag computed to know if this activity has been validated at least one time in the past.
        /// This flag is used to remember the validation of this activity when this activity was manual, then auto, then manual again.
        /// For the previous reason, you should not use this flag in the specific code.
        /// </summary>
        [Column("IS_VALID")]
        [Domain("DO_X_WORKFLOW_FLAG")]
        public bool IsValid
        {
            get;
            set;
        }

        /// <summary>
        /// Generated Key used to store the primary key for mass insertion of activities during the recalculation.
        /// </summary>
        [Column("INSERT_KEY")]
        [Domain("DO_X_WORKFLOW_ID")]
        [DataMember]
        [GeneratedCode("InsertKey", "")]
        public int? InsertKey { get; set; }

        /// <summary>
        /// Type énuméré présentant les noms des colonnes en base.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "A corriger")]
        public enum Cols
        {
            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfaId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFA_ID,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété CreationDate.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            CREATION_DATE,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfwId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFW_ID,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfadId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFAD_ID,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété IsAuto.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            IS_AUTO,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété IsValid.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            IS_VALID,
        }

        
    }

}
