using Kinetix.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kinetix.Workflow.model
{

    /// <summary>
    /// This class defines the Activity Definition.
    /// </summary>
    /// 
    [Table("WF_ACTIVITY_DEFINITION")]
    public partial class WfActivityDefinition
    {
        /// <summary>
        /// Constructeur.
        /// </summary>
        public WfActivityDefinition()
        {
            this.OnCreated();
        }

        /// <summary>
        /// Constructeur par recopie.
        /// </summary>
        /// <param name="bean">Source.</param>
        public WfActivityDefinition(WfActivityDefinition bean)
        {
            if (bean == null)
            {
                throw new ArgumentNullException(nameof(bean));
            }

            this.WfwdId = bean.WfwdId;
            this.Name = bean.Name;
            this.WfmdCode = bean.WfmdCode;
            this.WfadId = bean.WfadId;
            this.Level = bean.Level;

            this.OnCreated(bean);
        }

        #region Meta données

        /// <summary>
        /// Type énuméré présentant les noms des colonnes en base.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "A corriger")]
        public enum Cols
        {
            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfadId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFAD_ID,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété Name.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            NAME,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété Level.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            LEVEL,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfmdCode.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFMD_CODE,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfwdId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFWD_ID,

        }

        #endregion

        /// <summary>
        /// Primary key for WfActivityDefinition 
        /// </summary>
        [Column("WFAD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public int? WfadId
        {
            get;
            set;
        }

        /// <summary>
        /// Logical name for this definition.
        /// </summary>
        [Column("NAME")]
        [Domain("DO_X_WORKFLOW_LABEL")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Level position of this activity in the default transition path for the workflowDefinition.
        /// This field should not be used by specific code.
        /// </summary>
        [Column("LEVEL")]
        [Domain("DO_X_WORKFLOW_LEVEL")]
        public int? Level
        {
            get;
            set;
        }

        /// <summary>
        /// Multiplicity definition for this activity.
        /// Multiplicity can be :
        /// - Single : Only one user validation is required to go to the next step.
        /// - Multiple : All the users validations are required to go to the next step.
        /// </summary>
        [Column("WFMD_CODE")]
        [Domain("DO_X_WORKFLOW_CODE")]
        public string WfmdCode
        {
            get;
            set;
        }

        /// <summary>
        /// Foreign key to the WorkflowDefinition
        /// </summary>
        [Column("WFWD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int WfwdId
        {
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
        partial void OnCreated(WfActivityDefinition bean);
    }
}
