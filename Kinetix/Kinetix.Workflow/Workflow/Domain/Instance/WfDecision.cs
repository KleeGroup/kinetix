using Kinetix.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Workflow.instance
{
    /// <summary>
    /// This class defines the Decision.
    /// </summary>
    /// 
    [Table("WF_DECISION")]
    public partial class WfDecision
    {
        /// <summary>
        /// Constructeur.
        /// </summary>
        public WfDecision()
        {
            this.OnCreated();
        }

        /// <summary>
        /// Constructeur par recopie.
        /// </summary>
        /// <param name="bean">Source.</param>
        public WfDecision(WfDecision bean)
        {
            if (bean == null)
            {
                throw new ArgumentNullException(nameof(bean));
            }

            this.Id = bean.Id;
            this.Choice = bean.Choice;
            this.Comments = bean.Comments;
            this.DecisionDate = bean.DecisionDate;

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
            /// Nom de la colonne en base associée à la propriété Id.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFE_ID,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété Username.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            USERNAME,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété Choice.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            CHOICE,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété DecisionDate.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            DECISION_DATE,


            /// <summary>
            /// Nom de la colonne en base associée à la propriété Comments.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            COMMENTS,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfaId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFA_ID,

        }

        #endregion

        /// <summary>
        /// Primary key for WfDecision
        /// </summary>
        [Column("WFE_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public int? Id
        {
            get;
            set;
        }

        /// <summary>
        /// Username of the user who has validated the decision. 
        /// If the decision is auto, a specific user will be used.
        /// </summary>
        [Column("USERNAME")]
        [Domain("DO_X_WORKFLOW_USER")]
        public string Username
        {
            get;
            set;
        }

        /// <summary>
        /// Functionnal choice decision used to validate this decision.
        /// Ex: 1:OK, 2:OK with reserves, 3: KO
        /// </summary>
        [Column("CHOICE")]
        [Domain("DO_X_WORKFLOW_CHOICE")]
        public int? Choice
        {
            get;
            set;
        }

        /// <summary>
        /// Date of the decision.
        /// </summary>
        [Column("DECISION_DATE")]
        [Domain("DO_X_WORKFLOW_DATE")]
        public DateTime? DecisionDate
        {
            get;
            set;
        }

        /// <summary>
        /// User comments linked to the decision.
        /// </summary>
        [Column("COMMENTS")]
        [Domain("DO_X_WORKFLOW_COMMENTS")]
        public string Comments
        {
            get;
            set;
        }

        /// <summary>
        /// Foreign key linked to the activity
        /// </summary>
        [Column("WFA_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int WfaId
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
        partial void OnCreated(WfDecision bean);

    }
}
