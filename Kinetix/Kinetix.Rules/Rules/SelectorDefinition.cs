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
        public SelectorDefinition(int? id, int? itemId, string groupId) {
            this.Id = id;
            this.ItemId = itemId;
            this.GroupId = groupId;

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

            this.OnCreated(bean);
        }

        #region Meta données

        /// <summary>
        /// Type énuméré présentant les noms des colonnes en base.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "A corriger")]
        public enum Cols {
            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            ID,

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
            ITEM_ID,
        }

        #endregion

        [Column("ID")]
        [Domain("DO_X_RULES_ID")]
        [Key]
        public int? Id {
            get;
            set;
        }

        [Column("CREATION_DATE")]
        [Domain("DO_X_RULES_DATE")]
        public DateTime? CreationDate {
            get;
            set;
        }

        [Column("ITEM_ID")]
        [Domain("DO_X_RULES_WEAK_ID")]
        public int? ItemId {
            get;
            set;
        }

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
