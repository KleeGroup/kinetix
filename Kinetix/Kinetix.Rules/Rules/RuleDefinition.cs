using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kinetix.ComponentModel;


namespace Kinetix.Rules {

    /// <summary>
    /// This class defines the Definition of a rule.
    /// </summary>
    /// 
    [Table("RULE_DEFINITION")]
    public partial class RuleDefinition {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public RuleDefinition() {
            this.OnCreated();
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="id">Id de la règle.</param>
        /// <param name="creationDate">Date de création de la règle.</param>
        /// <param name="itemId">Id de l'Item.</param>
        /// <param name="label">Libelle de la règle.</param>

        public RuleDefinition(int? id, DateTime? creationDate, int? itemId, string label) {

            this.Id = id;
            this.CreationDate = creationDate;
            this.ItemId = itemId;
            this.Label = label;

            this.OnCreated();
        }

        /// <summary>
        /// Constructeur par recopie.
        /// </summary>
        /// <param name="bean">Source.</param>
        public RuleDefinition(RuleDefinition bean) {
            if (bean == null) {
                throw new ArgumentNullException(nameof(bean));
            }

            this.Id = bean.Id;
            this.CreationDate = bean.CreationDate;
            this.ItemId = bean.ItemId;
            this.Label = bean.Label;

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

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            LABEL,
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

        [Column("LABEL")]
        [Domain("DO_X_RULES_LABEL")]
        public string Label {
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
        partial void OnCreated(RuleDefinition bean);
    }
}


