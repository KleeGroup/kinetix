using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;

namespace Kinetix.Broker {
    /// <summary>
    /// Store Sql Server spécifique pour les tables de référence (gestion de la traduction).
    /// </summary>
    /// <typeparam name="T">Type du store.</typeparam>
    public class ReferenceSqlServerStore<T> : SqlServerStore<T>
        where T : class, new() {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="dataSourceName">Nom de la source de données.</param>
        public ReferenceSqlServerStore(string dataSourceName)
            : base(dataSourceName) {
        }

        /// <summary>
        /// Execute une commande et retourne un reader.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="tableName">Nom de la table.</param>
        /// <param name="criteria">Critère de recherche.</param>
        /// <param name="maxRows">Nombre maximum d'enregistrements (BrokerManager.NoLimit = pas de limite).</param>
        /// <param name="queryParameter">Paramètre de la requête.</param>
        /// <returns>DataReader contenant le résultat de la commande.</returns>
        protected override IReadCommand GetCommand(string commandName, string tableName, FilterCriteria criteria, int maxRows, QueryParameter queryParameter) {
            SqlServerCommand command = new SqlServerCommand(this.DataSourceName, commandName, CommandType.Text);

            StringBuilder commandText = new StringBuilder("select ");
            if (maxRows != BrokerManager.NoLimit) {
                commandText.Append("top(@top) ");
                command.Parameters.AddWithValue("top", maxRows);
            }

            string order = null;
            if (queryParameter != null && !string.IsNullOrEmpty(queryParameter.SortCondition)) {
                order = queryParameter.SortCondition;
            }

            BeanDefinition definition = BeanDescriptor.GetDefinition(typeof(T));
            BeanPropertyDescriptorCollection properties = definition.Properties;

            bool hasColumn = false;
            bool checkLanguage = false;

            // Rank sur les langues trouvées.
            var propTranslatableList = properties.Where(x => x.IsTranslatable);
            if (propTranslatableList.Any()) {

                checkLanguage = true;
                commandText.Append(" * from (select *,");

                IEnumerable<string> partitionList = new List<string>() { "temp." + definition.PrimaryKey.MemberName };
                ICollection<string> orderList = new List<string>();

                foreach (BeanPropertyDescriptor property in propTranslatableList) {
                    orderList.Add("rg" + property.PropertyName);
                }

                commandText.Append(GetRankOverPartitionSqlString(partitionList, orderList, "r"));
                commandText.Append(" from (Select ");
            }

            hasColumn = false;
            foreach (BeanPropertyDescriptor property in properties) {
                if (!IsPropertyToRetrieve(property)) {
                    continue;
                }

                if (hasColumn) {
                    commandText.Append(", ");
                }

                if (property.IsTranslatable) {
                    PrepareTranslationColumn(commandText, definition, property);
                } else {
                    commandText.Append("tab.").Append(property.MemberName);
                }

                hasColumn = true;
            }

            commandText.Append(" from ").Append(tableName).Append(" tab ");

            IDictionary<string, FilterCriteriaParam> criteriaMap = new Dictionary<string, FilterCriteriaParam>();
            foreach (FilterCriteriaParam param in criteria.Parameters) {
                if (!criteriaMap.ContainsKey(param.ColumnName)) {
                    criteriaMap.Add(param.ColumnName, param);
                }
            }

            IDictionary<string, string> propertyNameMap = new Dictionary<string, string>();
            foreach (BeanPropertyDescriptor property in properties) {
                if (!IsPropertyToRetrieve(property)) {
                    continue;
                }

                if (property.IsTranslatable) {
                    if (!propertyNameMap.ContainsKey(property.MemberName)) {
                        propertyNameMap.Add(property.MemberName, property.PropertyName);
                    }

                    string refTableName = definition.BeanType.FullName + "_." + property.PropertyName;
                    commandText.Append(" left join TRADUCTION_REFERENCE").Append(" ref").Append(property.PropertyName);
                    commandText.Append(" on ref").Append(property.PropertyName).Append(".").Append("TDR_TABLE").Append(" = '").Append(refTableName);
                    commandText.Append("' and ref").Append(property.PropertyName).Append(".").Append("TDR_CODE").Append(" = ").Append("tab.").Append(definition.PrimaryKey.MemberName);
                }
            }

            PrepareFilterCriteria(criteria, command, commandText);

            if (checkLanguage) {
                commandText.Append(" ) temp ) t where r = 1");
            }

            // Ajout du Order By si non-nul
            if (order != null) {
                commandText.Append(" order by ");
                commandText.Append(order);
            }

            // Set de la requête
            command.CommandText = commandText.ToString();

            return command;
        }

        /// <summary>
        /// Retourne le critère sur une colonne pour un bean de référence.
        /// </summary>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>Colonne à tester.</returns>
        protected override string GetColumnCriteriaByColumnName(string columnName) {
            StringBuilder commandText = new StringBuilder();
            BeanDefinition definition = BeanDescriptor.GetDefinition(typeof(T));
            BeanPropertyDescriptor property = definition.Properties.Where(x => x.MemberName == columnName).FirstOrDefault();
            if (property != null && property.IsTranslatable) {
                commandText.Append("isnull(ref").Append(property.PropertyName).Append(".").Append("TDR_VALEUR, tab.");
                commandText.Append(columnName).Append(")");
            } else {
                commandText.Append(columnName);
            }

            return commandText.ToString();
        }

        /// <summary>
        /// Retourne si la propriété doit être sélectionnée ou non.
        /// </summary>
        /// <param name="property">Propriété à vérifier.</param>
        /// <returns>Vrai si la propriété doit être sélectionnée, faux sinon.</returns>
        private static bool IsPropertyToRetrieve(BeanPropertyDescriptor property) {
            if (string.IsNullOrEmpty(property.MemberName)) {
                return false;
            }

            if (property.PropertyType == typeof(byte[])) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Prépare la chaîne SQL pour une condition case / when.
        /// </summary>
        /// <param name="columnTest">Nom de la colonne à tester.</param>
        /// <param name="valuesMap">Dictionnaire des valeurs test / valeurs à retourner.</param>
        /// <param name="valueElse">Valeur du else.</param>
        /// <param name="alias">Alias de la valeur.</param>
        /// <returns>Chaîne SQL pour la condition case / when.</returns>
        /// <remarks>Il y a autant de close when que de clés dans le dictionnaire valuesMap.</remarks>
        private static string GetCaseWhen(string columnTest, IDictionary<string, string> valuesMap, string valueElse, string alias) {
            StringBuilder commandText = new StringBuilder("case");
            foreach (string valueTest in valuesMap.Keys) {
                commandText.Append(" when ");
                commandText.Append(columnTest);
                commandText.Append(" = ").Append(valueTest);
                commandText.Append(" then ").Append(valuesMap[valueTest]);
            }

            if (!string.IsNullOrEmpty(valueElse)) {
                commandText.Append(" else ").Append(valueElse);
            }

            commandText.Append(" end ");
            if (!string.IsNullOrEmpty(alias)) {
                commandText.Append(" as ").Append(alias);
            }

            return commandText.ToString();
        }

        /// <summary>
        /// Prépare la chaîne SQL pour une close isnull.
        /// </summary>
        /// <param name="value1">Valeur 1.</param>
        /// <param name="value2">Valeur 2.</param>
        /// <param name="alias">Alias.</param>
        /// <returns>Chaîne SQL pour une close isnull.</returns>
        private static string GetIsNullSqlString(string value1, string value2, string alias) {
            StringBuilder commandText = new StringBuilder("isnull( ");
            commandText.Append(value1).Append(", ").Append(value2).Append(") ");
            if (!string.IsNullOrEmpty(alias)) {
                commandText.Append(" as ").Append(alias);
            }

            return commandText.ToString();
        }

        /// <summary>
        /// Prépare la chaîne SQL pour une close rank over partition.
        /// </summary>
        /// <param name="partitionByList">Liste des critères de partition.</param>
        /// <param name="orderByList">Liste des critères d'ordonnancement.</param>
        /// <param name="alias">Alias.</param>
        /// <returns>Chaîne SLQ pour la close rank over partition.</returns>
        private static string GetRankOverPartitionSqlString(IEnumerable<string> partitionByList, IEnumerable<string> orderByList, string alias) {
            StringBuilder commandText = new StringBuilder("rank() over (partition by ");
            bool hasColumn = false;
            foreach (string item in partitionByList) {
                if (hasColumn) {
                    commandText.Append(", ");
                }

                commandText.Append(item);
                hasColumn = true;
            }

            commandText.Append(" order by ");
            hasColumn = false;
            foreach (string item in orderByList) {
                if (hasColumn) {
                    commandText.Append(", ");
                }

                commandText.Append(item);
                hasColumn = true;
            }

            commandText.Append(") ");
            if (!string.IsNullOrEmpty(alias)) {
                commandText.Append(" as ").Append(alias);
            }

            return commandText.ToString();
        }

        /// <summary>
        /// Prépare les colonnes à charger dans le cas de colonnes traduites, pour une propriété.
        /// </summary>
        /// <param name="commandText">Texte de commande.</param>
        /// <param name="definition">Définition de l'objet à charger.</param>
        /// <param name="property">Propriété à charger.</param>
        private void PrepareTranslationColumn(StringBuilder commandText, BeanDefinition definition, BeanPropertyDescriptor property) {
            string refName = "ref" + property.PropertyName;
            string langue = refName + "." + "LAN_CODE";
            string valeur = refName + "." + "TDR_VALEUR";
            string cultureUI = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpperInvariant();

            IDictionary<string, string> valuesMap = new Dictionary<string, string>();
            valuesMap.Add("'" + cultureUI + "'", valeur);
            string caseWhenCondition = GetCaseWhen(langue, valuesMap, "'[' + " + langue + " + '] ' + " + valeur, null);

            commandText.Append(GetIsNullSqlString(caseWhenCondition, "tab." + property.MemberName, property.MemberName));

            commandText.Append(", ");
            valuesMap.Clear();
            valuesMap.Add("'" + cultureUI + "'", "1");
            if (!valuesMap.ContainsKey("'EN'")) {
                valuesMap.Add("'EN'", "2");
            }

            IEnumerable<string> partitionList = new List<string>() { "tab." + definition.PrimaryKey.MemberName };
            IEnumerable<string> orderList = new List<string>() { langue };
            caseWhenCondition = GetCaseWhen(langue, valuesMap, GetRankOverPartitionSqlString(partitionList, orderList, null) + " + 2", "rg" + property.PropertyName);
            commandText.Append(caseWhenCondition);
        }
    }
}
