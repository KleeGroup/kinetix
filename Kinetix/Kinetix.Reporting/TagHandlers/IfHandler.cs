using System;
using System.Collections.Generic;
using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Kinetix.Reporting.TagHandlers {

    /// <summary>
    /// Tag permettant de réaliser un If.
    /// </summary>
    internal class IfHandler : AbstractTagHandler {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="currentPart">OpenXmlPart courant.</param>
        /// <param name="currentElement">Element courant.</param>
        /// <param name="currentBean">Source de données du bloc conditionnel.</param>
        /// <param name="documentId">Id document en cours.</param>
        /// <param name="isXmlData">Si la source en xml.</param>
        public IfHandler(OpenXmlPart currentPart, CustomXmlElement currentElement, object currentBean, Guid documentId, bool isXmlData)
            : base(currentPart, currentElement, currentBean, documentId, isXmlData) {
            this.Condition = this["condition"];
            this.Expression = this["expression"];
            this.OrCondition = this["orcondition"];
            this.OrExpression = this["orexpression"];
        }

        /// <summary>
        /// Condition.
        /// </summary>
        public string Condition {
            get;
            private set;
        }

        /// <summary>
        /// Expression.
        /// </summary>
        public string Expression {
            get;
            private set;
        }

        /// <summary>
        /// Condition.
        /// </summary>
        public string OrCondition {
            get;
            private set;
        }

        /// <summary>
        /// Expression.
        /// </summary>
        public string OrExpression {
            get;
            private set;
        }

        /// <summary>
        /// Retourne si le tag If est uniquement associé à une condition.
        /// </summary>
        private bool IsConditionOnly {
            get {
                return string.IsNullOrEmpty(this.Expression) && string.IsNullOrEmpty(this.OrCondition) && string.IsNullOrEmpty(this.OrExpression);
            }
        }

        /// <summary>
        /// Retourne si le tag If est associé en plus de sa condition à une expression.
        /// </summary>
        private bool IsExpression {
            get {
                return !string.IsNullOrEmpty(this.Expression) && string.IsNullOrEmpty(this.OrCondition) && string.IsNullOrEmpty(this.OrExpression);
            }
        }

        /// <summary>
        /// Retourne si le tag est associé en plus de sa condition à une condition ou.
        /// </summary>
        private bool IsOrCondition {
            get {
                return string.IsNullOrEmpty(this.Expression) && !string.IsNullOrEmpty(this.OrCondition) && string.IsNullOrEmpty(this.OrExpression);
            }
        }

        /// <summary>
        /// Retourne si le tag est associé en plus de sa condition a une expression et une condition ou.
        /// </summary>
        private bool IsExpressionAndOrCondition {
            get {
                return !string.IsNullOrEmpty(this.Expression) && !string.IsNullOrEmpty(this.OrCondition) && string.IsNullOrEmpty(this.OrExpression);
            }
        }

        /// <summary>
        /// Retourne si le tag est associé à une expression, une condition ou et une expression ou.
        /// </summary>
        private bool IsExpressionAndOrExpressionAndOrCondition {
            get {
                return !string.IsNullOrEmpty(this.Expression) && !string.IsNullOrEmpty(this.OrCondition) && !string.IsNullOrEmpty(this.OrExpression);
            }
        }

        /// <summary>
        /// Retourne si le tag est associé à une expression ou et une condition ou.
        /// </summary>
        private bool IsOrExpressionAndOrCondition {
            get {
                return string.IsNullOrEmpty(this.Expression) && !string.IsNullOrEmpty(this.OrCondition) && !string.IsNullOrEmpty(this.OrExpression);
            }
        }

        /// <summary>
        /// Prise en charge du tag.
        /// </summary>
        /// <returns>Le contenu en OpenXML.</returns>
        protected override IEnumerable<OpenXmlElement> ProcessTag() {
            if (!Evaluate()) {
                return null;
            }

            return this.PrepareClone();
        }

        /// <summary>
        /// Evaluate expression.
        /// </summary>
        /// <param name="tagName">Tag name.</param>
        /// <param name="condition">Current condition.</param>
        /// <param name="expression">Current expression.</param>
        /// <param name="propertyValue">Property value.</param>
        /// <returns>Result of the evaluation. </returns>
        private static bool EvalExpression(string tagName, string condition, string expression, object propertyValue) {
            bool conditionValue = false;
            switch (expression) {
                case "(@ != null)":
                    if (propertyValue != null) {
                        if (propertyValue.GetType() == typeof(string)) {
                            conditionValue = !string.IsNullOrEmpty(Convert.ToString(propertyValue, CultureInfo.InvariantCulture));
                        } else {
                            conditionValue = true;
                        }
                    } else {
                        conditionValue = false;
                    }

                    break;
                case "(@ == null)":
                    if (propertyValue != null) {
                        if (propertyValue.GetType() == typeof(string)) {
                            conditionValue = string.IsNullOrEmpty(Convert.ToString(propertyValue, CultureInfo.InvariantCulture));
                        } else {
                            conditionValue = false;
                        }
                    } else {
                        conditionValue = true;
                    }

                    break;
                case "(!@)":
                    conditionValue = !Convert.ToBoolean(propertyValue, CultureInfo.InvariantCulture);
                    break;
                default:
                    throw new KeyNotFoundException("The tag " + tagName + " has no valide attribute named " + condition + " Expression " + expression);
            }

            return conditionValue;
        }

        /// <summary>
        /// Evalue la condition.
        /// </summary>
        /// <returns><code>True</code> si valide, <code>False </code> sinon.</returns>
        private bool Evaluate() {
            bool conditionValue = false;
            bool orconditionValue = false;
            object propertyValue = GetPropertyValue(this.DataSource, this.Condition, this.IsXmlData);
            object orpropertyValue = null;

            if (IsConditionOnly) {
                conditionValue = Convert.ToBoolean(propertyValue, CultureInfo.InvariantCulture);
            } else if (IsExpression) {
                conditionValue = EvalExpression(this.TagName, this.Condition, this.Expression, propertyValue);
            } else if (IsOrCondition) {
                conditionValue = Convert.ToBoolean(propertyValue, CultureInfo.InvariantCulture);
                orpropertyValue = GetPropertyValue(this.DataSource, this.OrCondition, this.IsXmlData);
                orconditionValue = Convert.ToBoolean(orpropertyValue, CultureInfo.InvariantCulture);
            } else if (IsExpressionAndOrCondition) {
                conditionValue = EvalExpression(this.TagName, this.Condition, this.Expression, propertyValue);
                orpropertyValue = GetPropertyValue(this.DataSource, this.OrCondition, this.IsXmlData);
                orconditionValue = Convert.ToBoolean(orpropertyValue, CultureInfo.InvariantCulture);
            } else if (IsExpressionAndOrExpressionAndOrCondition) {
                conditionValue = EvalExpression(this.TagName, this.Condition, this.Expression, propertyValue);
                orpropertyValue = GetPropertyValue(this.DataSource, this.OrCondition, this.IsXmlData);
                orconditionValue = EvalExpression(this.TagName, this.OrCondition, this.OrExpression, orpropertyValue);
            } else if (IsOrExpressionAndOrCondition) {
                conditionValue = Convert.ToBoolean(propertyValue, CultureInfo.InvariantCulture);
                orpropertyValue = GetPropertyValue(this.DataSource, this.OrCondition, this.IsXmlData);
                orconditionValue = EvalExpression(this.TagName, this.OrCondition, this.OrExpression, orpropertyValue);
            } else {
                // Cas OrExpression sans OrCondition.
                throw new KeyNotFoundException("The tag " + this.TagName + " has no valide attribute named " + this.Condition + " OrCondition " + this.OrExpression);
            }

            conditionValue = conditionValue || orconditionValue;
            return conditionValue;
        }
    }
}
