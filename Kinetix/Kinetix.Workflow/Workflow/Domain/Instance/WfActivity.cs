using Kinetix.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Kinetix.Workflow
{
    /// <summary>
    /// This class defines the Activity.
    /// </summary>
    /// 
    [Table("WF_ACTIVITY")]
    public class WfActivity
    {
        [Column("WFA_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        [Key]
        public int? WfaId
        {
            get;
            set;
        }

        [Column("CREATION_DATE")]
        [Domain("DO_X_WORKFLOW_DATE")]
        public DateTime? CreationDate
        {
            get;
            set;
        }

        [Column("WFW_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int WfwId
        {
            get;
            set;
        }

        [Column("WFAD_ID")]
        [Domain("DO_X_WORKFLOW_ID")]
        public int WfadId
        {
            get;
            set;
        }


        /// <summary>
        /// Type énuméré présentant les noms des colonnes en base.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "A corriger")]
        public enum Cols
        {
            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFA_ID,
            
            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            CREATION_DATE,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFW_ID,
            
            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            WFAD_ID,

        }

    }

}
