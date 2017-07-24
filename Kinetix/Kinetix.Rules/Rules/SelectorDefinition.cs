using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kinetix.ComponentModel;


namespace Kinetix.Rules {

    [Table("SELECTOR_DEFINITION")]
    public partial class SelectorDefinition {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public SelectorDefinition() {
            this.OnCreated();
        }

        /// <summary>
        /// Constructeur champs par champs.
        /// </summary>
        /// <param name="id">Id du selector.</param>
        /// <param name="creationDate">Date de création.</param>
        /// <param name="itemId">Id de l'item.</param>
        public SelectorDefinition(int? id, DateTime? creationDate, int? itemId, string groupId) {
            this.Id = id;
            this.ItemId = itemId;
            this.GroupId = groupId;
            this.CreationDate = creationDate;
            this.OnCreated();
        }

        /// <summary>
        /// Constructeur par recopie.
        /// </summary>
        /// <param name="bean">Source.</param>
        public SelectorDefinition(SelectorDefinition bean) {
            if (bean == null) {
                throw new ArgumentNullException(nameof(bean));
            }

            this.Id = bean.Id;
            this.CreationDate = bean.CreationDate;
            this.ItemId = bean.ItemId;
            this.GroupId = bean.GroupId;

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
            /// Nom de la colonne en base associée à la propriété GroupId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            GROUP_ID,
        }

        #endregion


        /// <summary>
        /// Primary Key for SelectorDefinition.
        /// </summary>
        [Column("ID")]
        [Domain("DO_X_RULES_ID")]
        [Key]
        public int? Id {
            get;
            set;
        }

        /// <summary>
        /// Creation Date for the Selector Definition
        /// </summary>
        [Column("CREATION_DATE")]
        [Domain("DO_X_RULES_DATE")]
        public DateTime? CreationDate {
            get;
            set;
        }

        /// <summary>
        /// Weak Foreign key to which item the rule should be applied.
        /// In the workflow context, this field should be linked to the WfActivityDefinition (and not the business object).
        /// </summary>
        [Column("ITEM_ID")]
        [Domain("DO_X_RULES_WEAK_ID")]
        public int? ItemId {
            get;
            set;
        }

        /// <summary>
        /// Weak Foreign key that define the group of users that should be selected.
        /// In the workflow context, this group id should be linked to the group of users that should be allowed to validate the activity designated by the ItemId Field.
        /// </summary>
        [Column("GROUP_ID")]
        [Domain("DO_X_RULES_GROUP_ID")]
        public string GroupId {
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
        partial void OnCreated(SelectorDefinition bean);
    }
}
