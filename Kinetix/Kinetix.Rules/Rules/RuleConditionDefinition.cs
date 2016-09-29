using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kinetix.ComponentModel;

namespace Kinetix.Rules {
    /// <summary>
    /// This class defines the Definition of rule condition.
    /// </summary>
    /// 
    [Table("RULE_CONDITION_DEFINITION")]
    public partial class RuleConditionDefinition {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public RuleConditionDefinition() {
            this.OnCreated();
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="id">Id de la condition.</param>
        /// <param name="field">Champ.</param>
        /// <param name="operateur">Operateur de la condition.</param>
        /// <param name="expression">Expression/Valeur.</param>
        /// <param name="rudId">Id de la rule associée.</param>
        public RuleConditionDefinition(int? id, string field, string operateur, string expression, int? rudId) {
            this.Id = id;
            this.Field = field;
            this.Operator = operateur;
            this.Expression = expression;
            this.RudId = rudId;

            this.OnCreated();
        }

        /// <summary>
        /// Constructeur par recopie.
        /// </summary>
        /// <param name="bean">Source.</param>
        public RuleConditionDefinition(RuleConditionDefinition bean) {
            if (bean == null) {
                throw new ArgumentNullException(nameof(bean));
            }

            this.Id = bean.Id;
            this.Field = bean.Field;
            this.Operator = bean.Operator;
            this.Expression = bean.Expression;
            this.RudId = bean.RudId;

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
            FIELD,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            OPERATOR,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            EXPRESSION,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété .
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            RUD_ID,
        }

        #endregion

        [Column("ID")]
        [Domain("DO_X_RULES_ID")]
        [Key]
        public int? Id {
            get;
            set;
        }

        [Column("FIELD")]
        [Domain("DO_X_RULES_FIELD")]
        public string Field {
            get;
            set;
        }

        [Column("OPERATOR")]
        [Domain("DO_X_RULES_OPERATOR")]
        public string Operator {
            get;
            set;
        }

        [Column("EXPRESSION")]
        [Domain("DO_X_RULES_EXPRESSION")]
        public string Expression {
            get;
            set;
        }

        [Column("RUD_ID")]
        [Domain("DO_X_RULES_ID")]
        public int? RudId {
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
        partial void OnCreated(RuleConditionDefinition bean);
    }
}

