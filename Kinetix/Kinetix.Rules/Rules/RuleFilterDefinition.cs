using Kinetix.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kinetix.Rules
{
    /// <summary>
    /// This class defines the Definition of a rule filter.
    /// </summary>
    /// 
    [Table("RULE_FILTER_DEFINITION")]
    public partial class RuleFilterDefinition
    {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public RuleFilterDefinition()
        {
            this.OnCreated();
        }

        /// <summary>
        /// Constructeur champs par champs.
        /// </summary>
        /// <param name="id">Id du filtre.</param>
        /// <param name="field">Champ.</param>
        /// <param name="operateur">Operateur de la condition.</param>
        /// <param name="expression">Expression/Valeur.</param>
        /// <param name="rudId">Id du selecteur associé.</param>
        public RuleFilterDefinition(int? id, string field, string operateur, string expression, int? selId)
        {
            this.Id = id;
            this.Field = field;
            this.Operator = operateur;
            this.Expression = expression;
            this.SelId = selId;

            this.OnCreated();
        }

        /// <summary>
        /// Constructeur par recopie.
        /// </summary>
        /// <param name="bean">Source.</param>
        public RuleFilterDefinition(RuleFilterDefinition bean)
        {
            if (bean == null)
            {
                throw new ArgumentNullException(nameof(bean));
            }

            this.Id = bean.Id;
            this.Field = bean.Field;
            this.Operator = bean.Operator;
            this.Expression = bean.Expression;
            this.SelId = bean.SelId;

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
            ID,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété Field.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            FIELD,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété Operator.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            OPERATOR,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété Expression.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            EXPRESSION,

            /// <summary>
            /// Nom de la colonne en base associée à la propriété SelId.
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Correspondance schéma persistence")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Correspondance schéma persistence")]
            SEL_ID,
        }

        #endregion

        /// <summary>
        /// Primary key for RuleFilterDefinition
        /// </summary>
        [Column("ID")]
        [Domain("DO_X_RULES_ID")]
        [Key]
        public int? Id
        {
            get;
            set;
        }

        /// <summary>
        /// Field of the business object on which the rule should be applied.
        /// </summary>
        [Column("FIELD")]
        [Domain("DO_X_RULES_FIELD")]
        public string Field
        {
            get;
            set;
        }

        /// <summary>
        /// Operator to apply to execute the selector.
        /// </summary>
        [Column("OPERATOR")]
        [Domain("DO_X_RULES_OPERATOR")]
        public string Operator
        {
            get;
            set;
        }

        /// <summary>
        /// Value expression that should be compared to the field whith the defined operator.
        /// Should be a constant
        /// </summary>
        [Column("EXPRESSION")]
        [Domain("DO_X_RULES_EXPRESSION")]
        public string Expression
        {
            get;
            set;
        }

        /// <summary>
        /// Foreign key to the SelectorDefinition
        /// </summary>
        [Column("SEL_ID")]
        [Domain("DO_X_RULES_ID")]
        public int? SelId
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
        partial void OnCreated(RuleFilterDefinition bean);
    }
}
