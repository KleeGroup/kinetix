using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;
using Kinetix.Security;

namespace Kinetix.Broker {

    /// <summary>
    /// Store SqlServer.
    /// </summary>
    /// <typeparam name="T">Type du store.</typeparam>
    public class SqlServerStore<T> : SqlStore<T>
        where T : class, new() {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="dataSourceName">Nom de la chaine de base de données.</param>
        public SqlServerStore(string dataSourceName)
            : base(dataSourceName) {
            string loggingStatement = "declare " + VariablePrefix + "UserId varbinary(128) = cast('{0}' as varbinary(128))" +
               ";WITH CHANGE_TRACKING_CONTEXT (" + VariablePrefix + "UserId) ";

            int? userId = StandardUser.UserId;
            if (userId.HasValue) {
                CurrentUserStatementLog = string.Format(CultureInfo.InvariantCulture, loggingStatement, userId.Value);
            } else {
                CurrentUserStatementLog = string.Format(CultureInfo.InvariantCulture, loggingStatement, 0);
            }
        }

        /// <summary>
        /// Préfixe utilisé par le store pour faire référence à une variable.
        /// </summary>
        protected override string VariablePrefix {
            get {
                return "@";
            }
        }

        /// <summary>
        /// Charactere de conacténation.
        /// </summary>
        protected override string ConcatCharacter {
            get {
                return " + ";
            }
        }

        /// <summary>
        /// Insère un nouvel enregistrement.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à insérér.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Reader retournant les données du bean inséré.</returns>
        protected override IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector) {
            string sql = this.BuildInsertQuery(beanDefinition, true, columnSelector);
            SqlServerCommand command = new SqlServerCommand(this.DataSourceName, commandName, sql);
            command.CommandTimeout = 0;
            this.AddInsertParameters(bean, beanDefinition, command.Parameters, columnSelector);
            return command.ExecuteReader();
        }

        /// <summary>
        /// Insère un nouvel enregistrement.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à insérer.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <param name="primaryKeyValue">Valeur de la clef primaire.</param>
        /// <returns>Bean inséré.</returns>
        protected override IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector, object primaryKeyValue) {

            if (primaryKey == null) {
                throw new ArgumentNullException("primaryKey");
            }

            string sql = this.BuildInsertQuery(beanDefinition, true, columnSelector);
            SqlServerCommand command = new SqlServerCommand(this.DataSourceName, commandName, sql);
            command.CommandTimeout = 0;
            command.Parameters.AddWithValue(primaryKey.MemberName, primaryKeyValue);
            this.AddInsertParameters(bean, beanDefinition, command.Parameters, columnSelector);
            return command.ExecuteReader();
        }

        /// <summary>
        /// Dépose les beans dans le store.
        /// </summary>
        /// <param name="commandName">Nom du service.</param>
        /// <param name="collection">Beans à enregistrer.</param>
        /// <param name="beanDefinition">Définition.</param>
        /// <returns>Beans enregistrés.</returns>
        [SuppressMessage("Klee.FxCop", "EX0009:NoTransactionSuppressRule", Justification = "Création du type à la volée en dehors de tout contexte transactionnel.")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Compléxité liée à l'optimisation mémoire.")]
        protected override ICollection<T> InsertAll(string commandName, ICollection<T> collection, BeanDefinition beanDefinition) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            if (beanDefinition == null) {
                throw new ArgumentNullException("beanDefinition");
            }

            SqlServerParameterBeanCollection<T> collectionStore = new SqlServerParameterBeanCollection<T>(collection, true);
            return collectionStore.ExecuteInsert(commandName, this.DataSourceName);
        }

        /// <summary>
        /// Met à jour un enregistrement.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à mettre à jour.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Reader retournant les données du bean mise à jour.</returns>
        protected override IDataReader Update(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, object primaryKeyValue, ColumnSelector columnSelector) {
            string sql = this.BuildUpdateQuery(bean, beanDefinition, primaryKey, columnSelector);
            SqlServerCommand command = new SqlServerCommand(this.DataSourceName, commandName, sql);
            command.CommandTimeout = 0;
            this.AddUpdateParameters(bean, beanDefinition, command.Parameters, columnSelector);
            return command.ExecuteReader();
        }

        /// <summary>
        /// Execute une commande et retourne un reader.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="tableName">Nom de la table.</param>
        /// <param name="criteria">Critère de recherche.</param>
        /// <param name="maxRows">Nombre maximum d'enregistrements (BrokerManager.NoLimit = pas de limite).</param>
        /// <param name="queryParameter">Paramètre de tri des résultats et de limit des résultats.</param>
        /// <returns>DataReader contenant le résultat de la commande.</returns>
        protected override IReadCommand GetCommand(string commandName, string tableName, FilterCriteria criteria, int maxRows, QueryParameter queryParameter) {
            SqlServerCommand command = new SqlServerCommand(this.DataSourceName, commandName, CommandType.Text);
            command.QueryParameters = queryParameter;

            StringBuilder commandText = new StringBuilder("select ");
            if (maxRows != BrokerManager.NoLimit) {
                commandText.Append("top(@top) ");
                command.Parameters.AddWithValue("top", maxRows);
            }

            string order = null;
            if (queryParameter != null && !string.IsNullOrEmpty(queryParameter.SortCondition)) {
                order = queryParameter.SortCondition;
            }

            // Todo : brancher le tri.
            AppendSelectParameters(commandText, tableName, criteria, order, command);

            // Set de la requête
            command.CommandText = commandText.ToString();

            return command;
        }

        /// <summary>
        /// Crée une nouvelle commande à partir d'une requête.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="commandType">Type de la commande.</param>
        /// <returns>Une nouvelle instance de SqlServerCommand.</returns>
        protected override SqlServerCommand CreateSqlCommand(string commandName, CommandType commandType) {
            return new SqlServerCommand(this.DataSourceName, commandName, commandType);
        }

        /// <summary>
        /// Ajout du paramètre en entrée de la commande envoyée à SQL Server.
        /// </summary>
        /// <param name="parameters">Collection des paramètres dans laquelle ajouter le nouveau paramètre.</param>
        /// <param name="primaryKeyName">Nom de la clé primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clé primaire.</param>
        protected override void AddPrimaryKeyParameter(SqlServerParameterCollection parameters, string primaryKeyName, object primaryKeyValue) {
            if (parameters == null) {
                throw new ArgumentNullException("parameters");
            }

            parameters.AddWithValue("PK_" + primaryKeyName, primaryKeyValue);
        }

        /// <summary>
        /// Ajoute un paramètre à une collection avec sa valeur.
        /// </summary>
        /// <param name="parameters">Collection de paramètres dans laquelle le nouveau paramètre est créé.</param>
        /// <param name="property">Propriété correspondant au paramètre.</param>
        /// <param name="value">Valeur du paramètre.</param>
        /// <returns>Paramètre ajouté.</returns>
        protected override SqlServerParameter AddParameter(SqlServerParameterCollection parameters, BeanPropertyDescriptor property, object value) {
            if (parameters == null) {
                throw new ArgumentNullException("parameters");
            }

            if (property == null) {
                throw new ArgumentNullException("property");
            }

            return parameters.AddWithValue(property.MemberName, value);
        }

        /// <summary>
        /// Crée la requête SQL d'insertion.
        /// </summary>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="dbGeneratedPK">True si la clef est générée par la base.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Requête SQL.</returns>
        private string BuildInsertQuery(BeanDefinition beanDefinition, bool dbGeneratedPK, ColumnSelector columnSelector) {
            StringBuilder sbInsert = new StringBuilder(CurrentUserStatementLog);
            sbInsert.Append("insert into ");
            sbInsert.Append(beanDefinition.ContractName).Append("(");
            StringBuilder sbValues = new StringBuilder(") values (");
            int count = 0;
            foreach (BeanPropertyDescriptor property in beanDefinition.Properties) {
                if (dbGeneratedPK && (property.IsPrimaryKey || property.MemberName == null ||
                    (columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName)))) {
                    continue;
                }

                IStoreRule rule = this.GetStoreRule(property.PropertyName);
                if (rule != null) {
                    ValueRule valueRule = rule.GetInsertValue(null);
                    if (valueRule != null && valueRule.Action == ActionRule.DoNothing) {
                        continue;
                    }
                }

                if (count > 0) {
                    sbInsert.Append(", ");
                    sbValues.Append(", ");
                }

                sbInsert.Append(property.MemberName);

                sbValues.Append(VariablePrefix);
                sbValues.Append(property.MemberName);
                count++;
            }

            sbInsert.Append(sbValues.ToString()).Append(")\n");
            if (dbGeneratedPK) {
                sbInsert.Append("select cast(SCOPE_IDENTITY() as int)");
            }

            return sbInsert.ToString();
        }
    }
}