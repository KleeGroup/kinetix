using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;
using log4net;

namespace Kinetix.Broker {

    /// <summary>
    /// Store de base pour le stockage en base de données.
    /// </summary>
    /// <typeparam name="T">Type du store.</typeparam>
    public abstract class SqlStore<T> : IStore<T>
        where T : class, new() {

        /// <summary>
        /// Préfixe générique d'un service de sélection.
        /// </summary>
        private const string ServiceSelect = "SV_SELECT";

        /// <summary>
        /// Préfixe générique d'un service d'insertion.
        /// </summary>
        private const string ServiceInsert = "SV_INSERT";

        /// <summary>
        /// Préfixe générique d'un service de mise à jour.
        /// </summary>
        private const string ServiceUpdate = "SV_UPDATE";

        /// <summary>
        /// Préfixe générique d'un service de suppression.
        /// </summary>
        private const string ServiceDelete = "SV_DELETE";

        /// <summary>
        /// Dictionnaire de règles.
        /// </summary>
        private readonly Dictionary<string, IStoreRule> _rules = new Dictionary<string, IStoreRule>();

        /// <summary>
        /// Définition de l'objet T.
        /// </summary>
        private readonly BeanDefinition _definition;

        /// <summary>
        /// Nom de la source de données.
        /// </summary>
        private readonly string _dataSourceName;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="dataSourceName">Nom de la chaine de base de données.</param>
        protected SqlStore(string dataSourceName) {
            ILog log = LogManager.GetLogger("Sql");
            try {
                if (dataSourceName == null) {
                    throw new ArgumentNullException("dataSourceName");
                }

                // Charge la définition de l'objet donné T.
                _definition = BeanDescriptor.GetDefinition(typeof(T), true);

                object[] attrs = typeof(T).GetCustomAttributes(typeof(TableAttribute), true);
                if (attrs == null || attrs.Length == 0) {
                    throw new NotSupportedException(typeof(T).FullName + " has no TableAttribute. Check type persistence.");
                }

                if (string.IsNullOrEmpty(_definition.ContractName)) {
                    throw new NotSupportedException(typeof(T) + " has no ContractName defined. Check type persistence.");
                }

                if (_definition.PrimaryKey == null) {
                    throw new NotSupportedException(typeof(T) + " has no primary key defined.");
                }

                _dataSourceName = dataSourceName;
            } catch (Exception e) {
                if (log.IsErrorEnabled) {
                    log.Error("Echec d'instanciation du store.", e);
                }

                throw new BrokerException("Broker<" + typeof(T).FullName + "> " + e.Message, e);
            }
        }

        /// <summary>
        /// Retourne la liste des régles appliquées par le store.
        /// </summary>
        public ICollection<IStoreRule> Rules {
            get {
                return _rules.Values;
            }
        }

        /// <summary>
        /// Current user logging statement.
        /// </summary>
        protected string CurrentUserStatementLog {
            get;
            set;
        }

        /// <summary>
        /// Source de données du store.
        /// </summary>
        protected string DataSourceName {
            get {
                return _dataSourceName;
            }
        }

        /// <summary>
        /// Retourne la définition.
        /// </summary>
        protected BeanDefinition Definition {
            get {
                return _definition;
            }
        }

        /// <summary>
        /// Lancemement d'une exception si la requête retourne un nombre de lignes supérieur au maximum spécifié.
        /// </summary>
        protected virtual bool ThrowExceptionOnRowOverflow {
            get {
                return true;
            }
        }

        /// <summary>
        /// Préfixe utilisé par le store pour faire référence à une variable.
        /// </summary>
        protected abstract string VariablePrefix {
            get;
        }

        /// <summary>
        /// Charactere de conacténation.
        /// </summary>
        protected abstract string ConcatCharacter {
            get;
        }

        /// <summary>
        /// Ajoute une règle de gestion au store.
        /// </summary>
        /// <param name="rule">Règle de gestion.</param>
        public void AddRule(IStoreRule rule) {
            if (rule == null) {
                throw new ArgumentNullException("rule");
            }

            _rules.Add(rule.FieldName, rule);
        }

        /// <summary>
        /// Checks if the object is used by another object in the application.
        /// </summary>
        /// <param name="primaryKey">Id of the object.</param>
        /// <param name="tablesToIgnore">A collection of tables to ignore when looking for tables that depend on the object.</param>
        /// <returns>True if the object is used by another object.</returns>
        public virtual bool IsUsed(object primaryKey, ICollection<string> tablesToIgnore = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if at least one of the objects is used by another object in the application.
        /// </summary>
        /// <param name="primaryKeys">Ids of the objects.</param>
        /// <param name="tablesToIgnore">A collection of tables to ignore when looking for tables that depend on the object.</param>
        /// <returns>True if one of the objects is used by another object.</returns>
        public virtual bool AreUsed(ICollection<int> primaryKeys, ICollection<string> tablesToIgnore = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Charge un bean à partir de sa clef primaire.
        /// </summary>
        /// <param name="destination">Le bean à charger.</param>
        /// <param name="primaryKey">Valeur de la clef primaire.</param>
        /// <returns>Bean.</returns>
        public T Load(T destination, object primaryKey) {
            if (primaryKey == null) {
                throw new ArgumentNullException("primaryKey");
            }

            _definition.PrimaryKey.CheckValueType(primaryKey);

            string commandName = ServiceSelect + "_" + _definition.ContractName;

            // On charge l'objet à partir d'un seul critère
            // correspondant à sa clé primaire
            FilterCriteria criteria = new FilterCriteria(_definition.PrimaryKey.MemberName, Expression.Equals, primaryKey);

            IReadCommand cmd = this.GetCommand(commandName, _definition.ContractName, criteria, BrokerManager.NoLimit, null);
            return CollectionBuilder<T>.ParseCommandForSingleObject(destination, cmd);
        }

        /// <summary>
        /// Charge toutes les données pour un type.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection.</returns>
        public ICollection<T> LoadAll(ICollection<T> collection, QueryParameter queryParameter) {
            string commandName = ServiceSelect + "_ALL_" + _definition.ContractName;

            return this.InternalLoadAll(collection, commandName, queryParameter, new FilterCriteria());
        }

        /// <summary>
        /// Récupération d'une liste d'objets d'un certain type correspondant à un critère donnée.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="criteria">Liste des critères de correpondance.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection.</returns>
        public ICollection<T> LoadAllByCriteria(ICollection<T> collection, FilterCriteria criteria, QueryParameter queryParameter) {

            // Les critères ne doivent pas être vides
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            string commandName = ServiceSelect + "_ALL_LIKE_" + _definition.ContractName;

            return InternalLoadAll(collection, commandName, queryParameter, criteria);
        }

        /// <summary>
        /// Récupération d'un objet à partir de critère de recherche.
        /// </summary>
        /// <param name="destination">Bean à charger.</param>
        /// <param name="criteria">Le critère de recherche.</param>
        /// <returns>Objet.</returns>
        public T LoadByCriteria(T destination, FilterCriteria criteria) {
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            string commandName = ServiceSelect + "_LIKE_" + _definition.ContractName;
            IReadCommand cmd = this.GetCommand(commandName, _definition.ContractName, criteria, BrokerManager.NoLimit, null);
            return CollectionBuilder<T>.ParseCommandForSingleObject(destination, cmd);
        }

        /// <summary>
        /// Dépose un bean dans le store.
        /// </summary>
        /// <param name="bean">Bean à enregistrer.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Clef primaire.</returns>
        /// <exception cref="BrokerException">Retourne une erreur en cas de mise à jour erronée.</exception>
        public object Put(T bean, ColumnSelector columnSelector = null) {
            if (bean == null) {
                throw new ArgumentNullException("bean");
            }

            object value = _definition.PrimaryKey.GetValue(bean);

            if (value == null && _definition.PrimaryKey.PropertyType == typeof(Guid?)) {
                value = Guid.NewGuid();
            }

            using (IDataReader reader = ExecutePutReader(bean, _definition, _definition.PrimaryKey, value, columnSelector)) {
                if (reader.RecordsAffected == 0) {
                    throw new BrokerException("Zero record affected");
                }

                if (reader.RecordsAffected > 1) {
                    throw new BrokerException("Too many records affected");
                }

                // Dans le cas d'un update, il n'y a plus de select
                // qui compte le nombre de lignes mises à jour, donc
                // on retourne directement l'identifiant.
                if (value != null) {
                    return value;
                }

                reader.Read();
                return reader.GetValue(0);
            }
        }

        /// <summary>
        /// Dépose les beans dans le store.
        /// </summary>
        /// <param name="collection">Beans à enregistrer.</param>
        /// <returns>Beans enregistrés.</returns>
        public ICollection<T> PutAll(ICollection<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            if (collection.Count == 0) {
                return collection;
            }

            string commandName = ServiceInsert + "_" + _definition.ContractName;
            return this.InsertAll(commandName, collection, _definition);
        }

        /// <summary>
        /// Supprime un bean du store.
        /// </summary>
        /// <param name="primaryKey">Clef primaire du bean à supprimer.</param>
        public void Remove(object primaryKey) {
            if (primaryKey == null) {
                throw new ArgumentNullException("primaryKey");
            }

            _definition.PrimaryKey.CheckValueType(primaryKey);
            string commandName = ServiceDelete + "_" + _definition.ContractName;

            // On charge l'objet à partir d'un seul critère
            // correspondant à sa clé primaire
            FilterCriteria criteria = new FilterCriteria(_definition.PrimaryKey.MemberName, Expression.Equals, primaryKey);

            int rowsAffected = this.DeleteAllByCriteria(commandName, _definition.ContractName, criteria);
            if (rowsAffected == 0) {
                throw new BrokerException("Zero row deleted");
            }

            if (rowsAffected > 1) {
                throw new BrokerException("Too many rows deleted");
            }
        }

        /// <summary>
        /// Supprime tous les objets correspondant aux critères.
        /// </summary>
        /// <param name="criteria">Critères de suppression.</param>
        public void RemoveAllByCriteria(FilterCriteria criteria) {
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            string commandName = ServiceDelete + "_ALL_LIKE_" + _definition.ContractName;
            this.DeleteAllByCriteria(commandName, _definition.ContractName, criteria);
        }

        /// <summary>
        /// Crée la commande.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="tableName">Nom de la table.</param>
        /// <param name="criteria">Liste des critères de recherche.</param>
        /// <param name="maxRows">Nombre maximum d'enregistrements (BrokerManager.NoLimit = pas de limite).</param>
        /// <param name="queryParameter">Paramètre de tri des résultats et de limit des résultats.</param>
        /// <returns>IReadCommand contenant la commande.</returns>
        protected abstract IReadCommand GetCommand(string commandName, string tableName, FilterCriteria criteria, int maxRows, QueryParameter queryParameter);

        /// <summary>
        /// Insère un nouvel enregistrement.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à insérer.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Bean inséré.</returns>
        protected abstract IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector);

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
        protected abstract IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector, object primaryKeyValue);

        /// <summary>
        /// Dépose les beans dans le store.
        /// </summary>
        /// <param name="commandName">Nom du service.</param>
        /// <param name="collection">Beans à enregistrer.</param>
        /// <param name="beanDefinition">Définition.</param>
        /// <returns>Beans enregistrés.</returns>
        protected abstract ICollection<T> InsertAll(string commandName, ICollection<T> collection, BeanDefinition beanDefinition);

        /// <summary>
        /// Met à jour un bean.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à mettre à jour.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Bean mise à jour.</returns>
        protected abstract IDataReader Update(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, object primaryKeyValue, ColumnSelector columnSelector);

        /// <summary>
        /// Supprime tous les objets correspondant aux critères.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="tableName">Nom de la table.</param>
        /// <param name="criteria">Critères de suppression.</param>
        /// <returns>Retourne le nombre de lignes supprimées.</returns>
        protected virtual int DeleteAllByCriteria(string commandName, string tableName, FilterCriteria criteria) {
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            SqlServerCommand command = CreateSqlCommand(commandName, CommandType.Text);
            command.CommandTimeout = 0;
            StringBuilder commandText = new StringBuilder(CurrentUserStatementLog);
            commandText.Append("delete from ");
            commandText.Append(tableName);
            if (criteria.Parameters.Any()) {
                PrepareFilterCriteria(criteria, command, commandText);
            }

            command.CommandText = commandText.ToString();
            return command.ExecuteNonQuery();
        }

        /// <summary>
        /// Retourne le critère sur une colonne.
        /// </summary>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>Colonne à tester.</returns>
        protected virtual string GetColumnCriteriaByColumnName(string columnName) {
            return columnName;
        }

        /// <summary>
        /// Crée une nouvelle commande à partir d'une requête.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="commandType">Type de la commande.</param>
        /// <returns>Une nouvelle instance d'une classe héritant de AbstractSqlCommand.</returns>
        protected abstract SqlServerCommand CreateSqlCommand(string commandName, CommandType commandType);

        /// <summary>
        /// Retourne la règle associée à un champ.
        /// </summary>
        /// <param name="fieldName">Nom du champ.</param>
        /// <returns>Null si aucune règle n'est définie.</returns>
        protected IStoreRule GetStoreRule(string fieldName) {
            IStoreRule rule;
            return _rules.TryGetValue(fieldName, out rule) ? rule : null;
        }

        /// <summary>
        /// Retourne le nombre maximal de lignes que renvoie la requête.
        /// </summary>
        /// <param name="maxRows">Nombre maximal de lignes attendues.</param>
        /// <returns>Nombre maximal de lignes renvoyées par la requête.</returns>
        protected int GetMaxRowCount(int maxRows) {
            if (maxRows == BrokerManager.NoLimit) {
                return BrokerManager.NoLimit;
            }

            return ThrowExceptionOnRowOverflow ? maxRows + 1 : maxRows;
        }

        /// <summary>
        /// Ajoute les paramètres de la requête select (noms des colonnes, clauses from, where et order by).
        /// </summary>
        /// <param name="commandText">Requête SQL à laquelle seront ajoutés les paramètres.</param>
        /// <param name="tableName">Nom de la table.</param>
        /// <param name="criteria">Critère de recherche.</param>
        /// <param name="sortOrder">Ordre de tri.</param>
        /// <param name="command">Commande d'appel à la base de données.</param>
        protected void AppendSelectParameters(StringBuilder commandText, string tableName, FilterCriteria criteria, string sortOrder, SqlServerCommand command) {
            if (commandText == null) {
                throw new ArgumentNullException("commandText");
            }

            BeanPropertyDescriptorCollection properties = BeanDescriptor.GetDefinition(typeof(T)).Properties;
            bool hasColumn = false;
            foreach (BeanPropertyDescriptor property in properties) {
                if (string.IsNullOrEmpty(property.MemberName)) {
                    continue;
                }

                if (property.PropertyType == typeof(byte[])) {
                    continue;
                }

                if (hasColumn) {
                    commandText.Append(", ");
                }

                commandText.Append(property.MemberName);
                hasColumn = true;
            }

            commandText.Append(" from ").Append(tableName);

            PrepareFilterCriteria(criteria, command, commandText);

            // Ajout du Order By si non-nul
            if (!string.IsNullOrEmpty(sortOrder)) {
                commandText.Append(" order by ");
                commandText.Append(sortOrder);
            }
        }

        /// <summary>
        /// Prépare la chaîne SQL et les paramètres de commandes pour appliquer un FilterCriteria.
        /// </summary>
        /// <param name="filter">Critères de filtrage.</param>
        /// <param name="command">Commande.</param>
        /// <param name="commandText">Texte de la commande.</param>
        protected void PrepareFilterCriteria(FilterCriteria filter, SqlServerCommand command, StringBuilder commandText) {
            if (filter == null) {
                throw new ArgumentNullException("filter");
            }

            if (command == null) {
                throw new ArgumentNullException("command");
            }

            if (commandText == null) {
                throw new ArgumentNullException("commandText");
            }

            int pos = 0;
            Dictionary<string, int> mapParameters = new Dictionary<string, int>();
            foreach (FilterCriteriaParam criteriaParam in filter.Parameters) {
                commandText.Append(pos == 0 ? " where " : " and ");
                commandText.Append(GetColumnCriteriaByColumnName(criteriaParam.ColumnName));

                string parameterName = null;
                if (!mapParameters.ContainsKey(criteriaParam.ColumnName)) {
                    parameterName = criteriaParam.ColumnName;
                    mapParameters.Add(criteriaParam.ColumnName, 1);
                } else {
                    mapParameters[criteriaParam.ColumnName] = mapParameters[criteriaParam.ColumnName] + 1;
                    parameterName = criteriaParam.ColumnName + mapParameters[criteriaParam.ColumnName].ToString(CultureInfo.InvariantCulture);
                }

                if (criteriaParam.Expression == Expression.Between) {
                    DateTime[] dateValues = (DateTime[])criteriaParam.Value;
                    command.Parameters.AddWithValue(parameterName + "T1", dateValues[0]);
                    command.Parameters.AddWithValue(parameterName + "T2", dateValues[0]);
                } else {
                    command.Parameters.AddWithValue(parameterName, criteriaParam.Value);
                }

                commandText.Append(GetSqlString(parameterName, criteriaParam));
                ++pos;
            }
        }

        /// <summary>
        /// Ajoute les paramètres d'insertion.
        /// </summary>
        /// <param name="bean">Bean à insérér.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="parameters">Paramètres de la commande SQL.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        protected void AddInsertParameters(T bean, BeanDefinition beanDefinition, SqlServerParameterCollection parameters, ColumnSelector columnSelector) {
            if (beanDefinition == null) {
                throw new ArgumentNullException("beanDefinition");
            }

            foreach (BeanPropertyDescriptor property in beanDefinition.Properties) {
                if (property.IsPrimaryKey || property.MemberName == null || (columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName))) {
                    continue;
                }

                object value = property.GetValue(bean);
                if (value != null) {
                    ExtendedValue extValue = value as ExtendedValue;
                    if (extValue != null) {
                        value = extValue.Value;
                    }
                }

                IStoreRule rule = this.GetStoreRule(property.PropertyName);
                ValueRule valueRule = null;
                if (rule != null) {
                    valueRule = rule.GetInsertValue(value);
                }

                if (valueRule != null) {
                    switch (valueRule.Action) {
                        case ActionRule.DoNothing:
                            continue;
                        case ActionRule.Update:
                            value = valueRule.Value;
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }

                // Ajout du paramètre en entrée de la commande.
                SqlServerParameter parameter = AddParameter(parameters, property, value);
                if (property.PrimitiveType == typeof(byte[])) {
                    parameter.DbType = DbType.Binary;
                }
            }
        }

        /// <summary>
        /// Ajoute les paramètres à une commande de mise à jour.
        /// </summary>
        /// <param name="bean">Bean à mettre à jour.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="parameters">Paramètres de la commande SQL.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        protected void AddUpdateParameters(T bean, BeanDefinition beanDefinition, SqlServerParameterCollection parameters, ColumnSelector columnSelector) {
            if (beanDefinition == null) {
                throw new ArgumentNullException("beanDefinition");
            }

            if (parameters == null) {
                throw new ArgumentNullException("parameters");
            }

            foreach (BeanPropertyDescriptor property in beanDefinition.Properties) {
                if (property.MemberName == null || (columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName) && !property.IsPrimaryKey)) {
                    continue;
                }

                object value = property.GetValue(bean);
                if (value != null) {
                    ExtendedValue extValue = value as ExtendedValue;
                    if (extValue != null) {
                        value = extValue.Value;
                    }
                }

                if (property.IsPrimaryKey) {
                    AddPrimaryKeyParameter(parameters, property.MemberName, value);
                }

                IStoreRule rule = this.GetStoreRule(property.PropertyName);
                ValueRule valueRule = null;
                if (rule != null) {
                    valueRule = rule.GetUpdateValue(value);
                    if (valueRule != null && valueRule.Action == ActionRule.DoNothing) {
                        continue;
                    }
                }

                if (valueRule == null) {
                    // Ajout du paramètre en entrée de la commande envoyée à SQL Server.
                    SqlServerParameter parameter = AddParameter(parameters, property, value);
                    if (property.PrimitiveType == typeof(byte[])) {
                        parameter.DbType = DbType.Binary;
                    }
                } else {
                    parameters.AddWithValue(property.MemberName, valueRule.Value);
                }

                // Contrainte de la valeur à mettre à jour
                if (rule != null) {
                    valueRule = rule.GetWhereClause(value);
                    if (valueRule != null && valueRule.Action != ActionRule.DoNothing) {
                        parameters.AddWithValue("RU_" + property.MemberName, valueRule.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Ajout du paramètre en entrée de la commande envoyée à SQL Server.
        /// </summary>
        /// <param name="parameters">Collection des paramètres dans laquelle ajouter le nouveau paramètre.</param>
        /// <param name="primaryKeyName">Nom de la clé primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clé primaire.</param>
        protected abstract void AddPrimaryKeyParameter(SqlServerParameterCollection parameters, string primaryKeyName, object primaryKeyValue);

        /// <summary>
        /// Crée la requête SQL de mise à jour d'un bean.
        /// </summary>
        /// <param name="bean">Bean à mettre à jour.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Requête SQL.</returns>
        protected string BuildUpdateQuery(T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector) {
            if (beanDefinition == null) {
                throw new ArgumentNullException("beanDefinition");
            }

            if (primaryKey == null) {
                throw new ArgumentNullException("primaryKey");
            }

            StringBuilder sbUpdate = new StringBuilder(CurrentUserStatementLog);
            sbUpdate.Append("update ");

            StringBuilder sbUpdateSet = new StringBuilder(beanDefinition.ContractName);
            sbUpdateSet.Append(" set");

            StringBuilder sbUpdateWhere = new StringBuilder(" where ");
            sbUpdateWhere.Append(primaryKey.MemberName).Append(" = ").Append(VariablePrefix).Append(primaryKey.MemberName);

            // Construction des champs de l'update SET et du WHERE
            int count = 0;
            foreach (BeanPropertyDescriptor property in beanDefinition.Properties) {

                // Si la propriété est une clé primaire ou n'est pas défini,
                // on passe à la propriété suivante.
                if (property.MemberName == null || property.IsPrimaryKey || property.IsReadOnly ||
                        (columnSelector != null && !columnSelector.ColumnList.Contains(property.MemberName))) {
                    continue;
                }

                // Dans le cas où la règle appliquée au champ n'a pas pour action
                // DoNothing, le champ est ajouté dans la liste pour être mis à jour.
                IStoreRule rule = this.GetStoreRule(property.PropertyName);
                ValueRule valueRule = null;
                if (rule != null) {
                    object value = property.GetValue(bean);
                    if (value != null) {
                        ExtendedValue extValue = value as ExtendedValue;
                        if (extValue != null) {
                            value = extValue.Value;
                        }
                    }

                    valueRule = rule.GetUpdateValue(value);
                    if (valueRule != null && ActionRule.DoNothing.Equals(valueRule.Action)) {
                        continue;
                    }
                }

                BuildUpdateSet(sbUpdateSet, count, property, valueRule);

                // Contrainte de la valeur à mettre à jour
                BuildUpdateWhere(bean, sbUpdateWhere, property, rule);

                count++;
            }

            sbUpdate.Append(sbUpdateSet).Append(sbUpdateWhere);
            return sbUpdate.ToString();
        }

        /// <summary>
        /// Crée la chaine lié au set.
        /// </summary>
        /// <param name="sbUpdateSet">Clause Set.</param>
        /// <param name="count">Index courant.</param>
        /// <param name="property">Propriété courante.</param>
        /// <param name="valueRule">Régle à appliquer.</param>
        protected void BuildUpdateSet(StringBuilder sbUpdateSet, int count, BeanPropertyDescriptor property, ValueRule valueRule) {
            if (sbUpdateSet == null) {
                throw new ArgumentNullException("sbUpdateSet");
            }

            if (property == null) {
                throw new ArgumentNullException("property");
            }

            if (count > 0) {
                sbUpdateSet.Append(",");
            }

            sbUpdateSet.Append(" ").Append(property.MemberName).Append(" = ");

            // Insertion de la valeur à mettre à jour
            if (valueRule == null) {
                // Si aucune règle n'est associée au champ
                sbUpdateSet.Append(VariablePrefix).Append(property.MemberName);
            } else {
                switch (valueRule.Action) {
                    case ActionRule.Update:
                        sbUpdateSet.Append(VariablePrefix).Append(property.MemberName);
                        break;
                    case ActionRule.IncrementalUpdate:
                        sbUpdateSet.Append(property.MemberName).Append(" + ").Append(VariablePrefix).Append(property.MemberName);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Crée la chaîne liée à la clause Where.
        /// </summary>
        /// <param name="bean">Bean à mettre à jour.</param>
        /// <param name="sbUpdateWhere">Clause Where.</param>
        /// <param name="property">Propriété courante.</param>
        /// <param name="rule">Règle à appliquer.</param>
        protected void BuildUpdateWhere(T bean, StringBuilder sbUpdateWhere, BeanPropertyDescriptor property, IStoreRule rule) {
            if (sbUpdateWhere == null) {
                throw new ArgumentNullException("sbUpdateWhere");
            }

            if (property == null) {
                throw new ArgumentNullException("property");
            }

            if (rule == null) {
                return;
            }

            object value = property.GetValue(bean);
            if (value != null) {
                ExtendedValue extValue = value as ExtendedValue;
                if (extValue != null) {
                    value = extValue.Value;
                }
            }

            ValueRule valueRule = rule.GetWhereClause(value);
            if (valueRule == null) {
                return;
            }

            switch (valueRule.Action) {
                case ActionRule.Check:
                    sbUpdateWhere.Append(" and ").Append(property.MemberName).Append(" = ");
                    sbUpdateWhere.Append(VariablePrefix).Append("RU_").Append(
                        property.MemberName);
                    break;
                case ActionRule.DoNothing:
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Ajoute un paramètre à une collection avec sa valeur.
        /// </summary>
        /// <param name="parameters">Collection de paramètres dans laquelle le nouveau paramètre est créé.</param>
        /// <param name="property">Propriété correspondant au paramètre.</param>
        /// <param name="value">Valeur du paramètre.</param>
        /// <returns>Paramètre ajouté.</returns>
        protected abstract SqlServerParameter AddParameter(SqlServerParameterCollection parameters, BeanPropertyDescriptor property, object value);

        /// <summary>
        /// Retourne la traduction SQL du paramètre de filtrage considéré.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <param name="criteriaParam">Le parametre considéré.</param>
        /// <returns>L'expression traduite en SQL.</returns>
        private string GetSqlString(string parameterName, FilterCriteriaParam criteriaParam) {
            switch (criteriaParam.Expression) {
                case Expression.Between:
                    return " BETWEEN " + VariablePrefix + parameterName + "T1" + " AND " + VariablePrefix + parameterName + "T2";
                case Expression.Contains:
                    return " LIKE '%' + " + VariablePrefix + parameterName + " " + ConcatCharacter + " '%'";
                case Expression.EndsWith:
                    return " LIKE '%' + " + VariablePrefix + parameterName;
                case Expression.Equals:
                    return " = " + VariablePrefix + parameterName;
                case Expression.GreaterOrEquals:
                    return " >= " + VariablePrefix + parameterName;
                case Expression.LowerOrEquals:
                    return " <= " + VariablePrefix + parameterName;
                case Expression.Greater:
                    return " > " + VariablePrefix + parameterName;
                case Expression.IsNotNull:
                    return " IS NOT NULL";
                case Expression.IsNull:
                    return " IS NULL";
                case Expression.Lower:
                    return " < " + VariablePrefix + parameterName;
                case Expression.NotStartsWith:
                    return " NOT LIKE " + VariablePrefix + parameterName + " " + ConcatCharacter + "'%'";
                case Expression.StartsWith:
                    return " LIKE " + VariablePrefix + parameterName + " " + ConcatCharacter + " '%'";
                case Expression.NotEquals:
                    return " != " + VariablePrefix + parameterName;
                default:
                    throw new NotSupportedException("Type d'expression de filtre non supportée : " + criteriaParam.Expression.ToString());
            }
        }

        /// <summary>
        /// Récupération d'une liste d'objets d'un certain type correspondant à un critère donnée.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <param name="criteria">Map de critères auquelle la recherche doit correpondre.</param>
        /// <returns>Collection.</returns>
        private ICollection<T> InternalLoadAll(ICollection<T> collection, string commandName, QueryParameter queryParameter, FilterCriteria criteria) {
            int maxRows = BrokerManager.NoLimit;
            if (queryParameter != null) {
                // Définition du tri à partir de la requete.
                queryParameter.RemapSortColumn(typeof(T));

                // Definition du maxRows
                maxRows = GetMaxRowCount(queryParameter.MaxRows);
            }

            IReadCommand cmd = this.GetCommand(commandName, _definition.ContractName, criteria, maxRows, queryParameter);
            ICollection<T> coll = CollectionBuilder<T>.ParseCommand(collection, cmd);

            long collCount = coll.Count;
            if (queryParameter != null && (queryParameter.Offset > 0 || queryParameter.Limit > 0)) {
                collCount = QueryContext.InlineCount.Value;
            }

            if (maxRows > BrokerManager.NoLimit && collCount > maxRows) {
                throw new BrokerException("Store return too many rows.");
            }

            return coll;
        }

        /// <summary>
        /// Obtient un reader du résultat d'enregistrement.
        /// </summary>
        /// <param name="bean">Bean à saugevarder.</param>
        /// <param name="beanDefinition">Definition.</param>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clef primaire.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>DataReader contenant le bean sauvegardé.</returns>
        private IDataReader ExecutePutReader(T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, object primaryKeyValue, ColumnSelector columnSelector) {
            if (primaryKey.GetValue(bean) != null) {
                string commandName = ServiceUpdate + "_" + beanDefinition.ContractName;
                return this.Update(commandName, bean, beanDefinition, primaryKey, primaryKeyValue, columnSelector);
            }

            if (primaryKeyValue == null) {
                string commandName = ServiceInsert + "_" + beanDefinition.ContractName;
                return this.Insert(commandName, bean, beanDefinition, primaryKey, columnSelector);
            } else {
                string commandName = ServiceInsert + "_" + beanDefinition.ContractName;
                return this.Insert(commandName, bean, beanDefinition, primaryKey, columnSelector, primaryKeyValue);
            }
        }
    }
}
