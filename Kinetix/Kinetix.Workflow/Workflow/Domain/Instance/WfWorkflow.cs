using Kinetix.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Kinetix.Workflow.instance
{

    /// <summary>
    /// This class defines the Workflow Instance.
    /// </summary>
    /// 
    [Table("WF_WORKFLOW")]
    public partial class WfWorkflow
    {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public WfWorkflow()
        {
            this.OnCreated();
        }

        /// <summary>
        /// Constructeur par recopie.
        /// </summary>
        /// <param name="bean">Source.</param>
        public WfWorkflow(WfWorkflow bean)
        {
            if (bean == null)
            {
                throw new ArgumentNullException(nameof(bean));
            }

            this.WfwId = bean.WfwId;
            this.CreationDate = bean.CreationDate;
            this.ItemId = bean.ItemId;
            this.Username = bean.Username;
            this.UserLogic = bean.UserLogic;

            this.WfwdId = bean.WfwdId;
            this.WfsCode = bean.WfsCode;
            this.WfaId2 = bean.WfaId2;

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
            /// Nom de la colonne en base associée à la propriété WfwId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFW_ID,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété CreationDate.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            CREATION_DATE,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété ItemId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            ITEM_ID,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété Username.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            USERNAME,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété UserLogic.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            USER_LOGIC,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfwdId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFWD_ID,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfsCode.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFS_CODE,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfaId2.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFA_CURRENT,
        }

        #endregion

        /// <summary>
        /// Primary key for the WfWorkflow
        /// </summary>
        [Column("WFW_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public int? WfwId
        {
            get;
            set;
        }

        /// <summary>
        /// Creation date of the workflow instance
        /// </summary>
        [Column("CREATION_DATE")]
        [Domain("DO_X_WORKFLOW_DATE")]
        public DateTime? CreationDate
        {
            get;
            set;
        }

        /// <summary>
        /// Weak foreign key linked to the business object on which the rules should be applyed.
        /// Ex of business object : DemandeAchat
        /// </summary>
        [Column("ITEM_ID")]
        [Domain("DO_X_WORKFLOW_WEAK_ID")]
        public int? ItemId
        {
            get;
            set;
        }

        /// <summary>
        /// Username of the user who has created this workflow instance.
        /// </summary>
        [Column("USERNAME")]
        [Domain("DO_X_WORKFLOW_USER")]
        public string Username
        {
            get;
            set;
        }

        /// <summary>
        /// Flag to know if this workflow has been created by a human user or by a business rules or a job
        /// True if created manually
        /// False if created automatically
        /// </summary>
        [Column("USER_LOGIC")]
        [Domain("DO_X_WORKFLOW_FLAG")]
        [Required]
        public bool UserLogic
        {
            get;
            set;
        }

        /// <summary>
        /// Foreign key linked to the workflow definition
        /// </summary>
        [Column("WFWD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Required]
        public int? WfwdId
        {
            get;
            set;
        }

        /// <summary>
        /// Status code for the workflow instance status : the workflow can be :
        /// Created, Started, paused or Stopped
        /// </summary>
        [Column("WFS_CODE")]
        [Domain("DO_X_WORKFLOW_CODE")]
        [Required]
        public string WfsCode
        {
            get;
            set;
        }

        /// <summary>
        /// Foreign key to the current activity.
        /// 
        /// Use this field to know the current activity of this instance. 
        /// This field will be updated by the engine when the user go to the next activity, or when a 
        /// recalculation determine that this activity is the current.
        /// This is one of the most important field of the workflow package.
        /// </summary>
        [Column("WFA_ID_2")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int? WfaId2
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
        partial void OnCreated(WfWorkflow bean);


    }
}
