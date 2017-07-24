using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kinetix.ComponentModel;


namespace Kinetix.Workflow.model {
    /// <summary>
    /// This class defines the Transition Definition.
    /// </summary>
    /// 
    [Table("WF_TRANSITION_DEFINITION")]
    public partial class WfTransitionDefinition {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public WfTransitionDefinition() {
            this.OnCreated();
        }

        /// <summary>
        /// Constructeur par recopie.
        /// </summary>
        /// <param name="bean">Source.</param>
        public WfTransitionDefinition(WfTransitionDefinition bean) {
            if (bean == null) {
                throw new ArgumentNullException(nameof(bean));
            }

            this.Id = bean.Id;
            this.Name = bean.Name;
            this.WfadIdFrom = bean.WfadIdFrom;
            this.WfadIdTo = bean.WfadIdTo;
            this.WfwdId = bean.WfwdId;
            this.OnCreated(bean);
        }

        #region Meta données

        /// <summary>
        /// Type énuméré présentant les noms des colonnes en base.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "A corriger")]
        public enum Cols {
            /// <summary>
            /// Nom de la colonne en base associée à la propriété Id.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            ID,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété Name.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            NAME,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfadIdFrom.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFAD_ID_FROM,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfadIdTo.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFAD_ID_TO,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété WfwdId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFWD_ID,

        }

        #endregion

        /// <summary>
        /// Primary key for WfTransitionDefinition
        /// </summary>
        [Column("WFTD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public int? Id {
            get;
            set;
        }

        /// <summary>
        /// Name for the transition.
        /// All transition must be named. A path with transition named 'Default' 
        /// must be used to define the main workflow path.
        /// </summary>
        [Column("NAME")]
        [Domain("DO_X_WORKFLOW_LABEL")]
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Foreign key to the WfActivity.
        /// This field define the Activity origin for this "oriented" transition
        /// </summary>
        [Column("WFAD_ID_FROM")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int WfadIdFrom {
            get;
            set;
        }

        /// <summary>
        /// Foreign key to the WfActivity.
        /// This field define the Activity end for this "oriented" transition
        /// </summary>
        [Column("WFAD_ID_TO")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int WfadIdTo {
            get;
            set;
        }

        /// <summary>
        /// Foreign key to the WfWorkflowDefinition.
        /// </summary>
        [Column("WFWD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int WfwdId {
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
        partial void OnCreated(WfTransitionDefinition bean);

    }
}
