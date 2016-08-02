using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Kinetix.ComponentModel;

namespace Kinetix.Broker {

    /// <summary>
    /// Critère de recherche par champs.
    /// Les champs de l'objet recherché sont filtrés par la valeur associée dans la map.
    /// </summary>
    public class FilterCriteria {

        /// <summary>
        /// Liste des critères de recherche.
        /// </summary>
        private readonly IList<FilterCriteriaParam> _listFilter = new List<FilterCriteriaParam>();

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        public FilterCriteria() {
        }

        /// <summary>
        /// Constructeur à partir d'une liste de critères.
        /// </summary>
        /// <param name="criteria">Critère.</param>
        public FilterCriteria(object criteria) {
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            this.AddAllCriteria(criteria, null);
        }

        /// <summary>
        /// Constructeur à partir d'un objet via SqlStore.
        /// </summary>
        /// <param name="param">Nom du champ en base.</param>
        /// <param name="exprType">Type d'expression.</param>
        /// <param name="value">Valeur du champ.</param>
        internal FilterCriteria(string param, Expression exprType, object value) {
            AddCriteria(param, exprType, value);
        }

        /// <summary>
        /// Retourne la liste des paramètres du critère.
        /// </summary>
        public ReadOnlyCollection<FilterCriteriaParam> Parameters {
            get {
                return new ReadOnlyCollection<FilterCriteriaParam>(_listFilter);
            }
        }

        /// <summary>
        /// Ajoute au critère une intervalle de recherche pour les datess.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <param name="min">Valeur inférieure.</param>
        /// <param name="max">Valeur supérieure.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria Between(Enum enumCol, DateTime min, DateTime max) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            AddCriteria(enumCol.ToString(), Expression.Between, new DateTime[] { min, max });
            return this;
        }

        /// <summary>
        /// Ajoute au critère un filtre LIKE.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <param name="value">Valeur contenue.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria Contains(Enum enumCol, string value) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentNullException("value");
            }

            AddCriteria(enumCol.ToString(), Expression.Contains, value);
            return this;
        }

        /// <summary>
        /// Ajoute un critère vérifiant que la colonne se termine par la chaine de caractères.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <param name="value">Valeur testée.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria EndsWith(Enum enumCol, string value) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentNullException("value");
            }

            AddCriteria(enumCol.ToString(), Expression.EndsWith, value);
            return this;
        }

        /// <summary>
        /// Ajoute un critère d'égalité.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <param name="value">Valeur testée.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria Equals(Enum enumCol, object value) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            if (value == null) {
                throw new ArgumentNullException("value", "Utiliser la méthode IsNull si vous souhaitez tester la nullité.");
            }

            AddCriteria(enumCol.ToString(), Expression.Equals, value);
            return this;
        }

        /// <summary>
        /// Ajoute un critère de supériorité.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <param name="value">Valeur testée.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria Greater(Enum enumCol, object value) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            if (value == null) {
                throw new ArgumentNullException("value");
            }

            AddCriteria(enumCol.ToString(), Expression.Greater, value);
            return this;
        }

        /// <summary>
        /// Ajoute un critère de supériorité ou d'égalité.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <param name="value">Valeur testée.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria GreaterOrEquals(Enum enumCol, object value) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            if (value == null) {
                throw new ArgumentNullException("value");
            }

            AddCriteria(enumCol.ToString(), Expression.GreaterOrEquals, value);
            return this;
        }

        /// <summary>
        /// Ajoute un critère de nullité.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria IsNull(Enum enumCol) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            AddCriteria(enumCol.ToString(), Expression.IsNull, null);
            return this;
        }

        /// <summary>
        /// Ajoute un critère de non nullité.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria IsNotNull(Enum enumCol) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            AddCriteria(enumCol.ToString(), Expression.IsNotNull, null);
            return this;
        }

        /// <summary>
        /// Ajoute un critère d'infériorité.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <param name="value">Valeur testée.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria Lower(Enum enumCol, object value) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            if (value == null) {
                throw new ArgumentNullException("value");
            }

            AddCriteria(enumCol.ToString(), Expression.Lower, value);
            return this;
        }

        /// <summary>
        /// Ajoute un critère d'infériorité ou d'égalité.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <param name="value">Valeur testée.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria LowerOrEquals(Enum enumCol, object value) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            if (value == null) {
                throw new ArgumentNullException("value");
            }

            AddCriteria(enumCol.ToString(), Expression.LowerOrEquals, value);
            return this;
        }

        /// <summary>
        /// Ajoute un critère d'inégalité.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <param name="value">Valeur testée.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria NotEquals(Enum enumCol, object value) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            if (value == null) {
                throw new ArgumentNullException("value");
            }

            AddCriteria(enumCol.ToString(), Expression.NotEquals, value);
            return this;
        }

        /// <summary>
        /// Ajoute un critère vérifiant que la colonne ne commence pas par la chaîne de caractères.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <param name="value">Valeur testée.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria NotStartsWith(Enum enumCol, object value) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            if (value == null) {
                throw new ArgumentNullException("value");
            }

            AddCriteria(enumCol.ToString(), Expression.NotStartsWith, value);
            return this;
        }

        /// <summary>
        /// Ajoute un critère vérifiant que la colonne commence par la chaîne de caractères.
        /// </summary>
        /// <param name="enumCol">Colonne cible.</param>
        /// <param name="value">Valeur testée.</param>
        /// <returns>Le critère.</returns>
        public FilterCriteria StartsWith(Enum enumCol, object value) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            if (value == null) {
                throw new ArgumentNullException("value");
            }

            AddCriteria(enumCol.ToString(), Expression.StartsWith, value);
            return this;
        }

        /// <summary>
        /// Ajoute un critère dans le filtre.
        /// </summary>
        /// <param name="param">Nom du paramètre (champ dans la table).</param>
        /// <param name="exprType">Expression utilisée.</param>
        /// <param name="value">Valeur du critère.</param>
        internal void AddCriteria(string param, Expression exprType, object value) {
            if (string.IsNullOrEmpty(param)) {
                throw new ArgumentNullException("param");
            }

            if (value == null && exprType != Expression.IsNotNull && exprType != Expression.IsNull) {
                throw new ArgumentNullException("value");
            }

            if (exprType == Expression.Between) {
                DateTime[] dateValues = value as DateTime[];
                if (dateValues == null || dateValues.Length != 2) {
                    throw new NotSupportedException("Expression.Between only supports DateTime[2] values.");
                }
            }

            _listFilter.Add(new FilterCriteriaParam(param, exprType, value));
        }

        /// <summary>
        /// Constructeur à partir d'une liste de critères.
        /// </summary>
        /// <param name="criteria">Critère.</param>
        /// <param name="expressionIndex">Index des expressions à appliquer.</param>
        private void AddAllCriteria(object criteria, IDictionary<string, Expression> expressionIndex) {
            BeanDefinition definition = BeanDescriptor.GetDefinition(criteria);
            foreach (BeanPropertyDescriptor property in definition.Properties) {
                if (property.MemberName == null) {
                    continue;
                }

                object value = property.GetValue(criteria);
                if (value == null) {
                    continue;
                }

                ExtendedValue extValue = value as ExtendedValue;
                if (extValue != null) {
                    if (extValue.Value == null) {
                        continue;
                    }

                    value = extValue.Value;
                }

                string s = value as string;
                if (s != null && s.Length == 0) {
                    continue;
                }

                Expression expr = Expression.Equals;
                if (expressionIndex != null && expressionIndex.ContainsKey(property.MemberName)) {
                    expr = expressionIndex[property.MemberName];
                }

                this.AddCriteria(property.MemberName, expr, value);
            }
        }
    }
}
