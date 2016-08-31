using System;
using System.Collections.Generic;
using System.Data;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Store de test du SqlStore.
    /// </summary>
    /// <typeparam name="T">Type du store.</typeparam>
    public class SqlTestStore<T> : SqlStore<T> where T : class, new() {
        /// <summary>
        /// Préfixe utilisé par le store pour faire référence à une variable
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
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="name">Nom de la base de données utilisée.</param>
        public SqlTestStore(string name)
            : base(name) {
        }

        /// <summary>
        /// Retourne le nom de la source de données du store.
        /// </summary>
        public string Name {
            get {
                return this.DataSourceName;
            }
        }

        /// <summary>
        /// Retourne l'ordre de tri de la requête.
        /// </summary>
        public string SortOrder {
            get;
            private set;
        }

        /// <summary>
        /// Retourne une régle associée au store.
        /// </summary>
        /// <param name="fieldName">Nom du champ.</param>
        /// <returns>Régle si elle existe.</returns>
        public IStoreRule GetRule(string fieldName) {
            return this.GetStoreRule(fieldName);
        }

        /// <summary>
        /// Prépare une commande.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="tableName">Nom de la table.</param>
        /// <param name="criteria">Critères de recherche.</param>
        /// <param name="sortOrder">Ordre de tri.</param>
        /// <param name="maxRows">Nombre maximum d'enregistrements (BrokerManager.NoLimit = pas de limite).</param>
        /// <param name="limit">Nombre de lignes à ramener.</param>
        /// <param name="offset">Offset de sélection des lignes.</param>
        /// <returns>Résultat de la requête.</returns>
        protected override IReadCommand GetCommand(string commandName, string tableName, FilterCriteria criteria, int maxRows, QueryParameter queryParameter) {
            string sortOrder = string.Empty;
            foreach (string sort in queryParameter.SortedFields) {
                sortOrder += sort + ',';
            }
            sortOrder.Substring(0, sortOrder.Length);
            this.SortOrder = sortOrder;

            if ("SV_SELECT_BEAN".Equals(commandName)) {
                Assert.AreEqual("BEAN", tableName);
                Assert.IsNull(sortOrder);
                BeanDefinition definition = BeanDescriptor.GetDefinition(typeof(Bean));
                FilterCriteria filter = (FilterCriteria)criteria;
                FilterCriteriaParam pkParam = null;
                foreach (FilterCriteriaParam criteriaParam in filter.Parameters) {
                    if (criteriaParam.ColumnName == definition.PrimaryKey.MemberName) {
                        pkParam = criteriaParam;
                    }
                }
                Assert.IsNotNull(pkParam);
                Assert.IsNotNull(pkParam.Value);

                return new TestDbCommand(new TestDataReader((int)pkParam.Value));
            } else if ("SV_SELECT_ALL_BEAN".Equals(commandName)) {
                Assert.AreEqual("BEAN", tableName);
                Assert.AreEqual("BEA_NAME", sortOrder);

                if (maxRows > 10) {
                    return new TestDbCommand(new TestDataReader(maxRows));
                } else {
                    return new TestDbCommand(new TestDataReader(maxRows - 1));
                }
            } else if ("SV_SELECT_ALL_LIKE_BEAN".Equals(commandName)) {
                return new TestDbCommand(new TestDataReader(10));
            } else if ("SV_SELECT_LIKE_BEAN".Equals(commandName)) {
                return new TestDbCommand(new TestDataReader(1));
            } else {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Insère un nouvel enregistrement.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à insérer.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <returns>Bean inséré.</returns>
        protected override IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector) {
            BeanDefinition definition = BeanDescriptor.GetDefinition(typeof(Bean));
            Assert.AreEqual(definition, beanDefinition);
            Assert.AreEqual(definition.PrimaryKey, primaryKey);
            Assert.IsNull(primaryKey.GetValue(bean));
            return new TestDataReader(1, 1);
        }

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
        protected override IDataReader Update(string commandName, T bean, BeanDefinition beanDefinition, BeanPropertyDescriptor primaryKey, object primaryKeyValue, ColumnSelector columnSelector) {
            BeanDefinition definition = BeanDescriptor.GetDefinition(typeof(Bean));
            Assert.AreEqual(definition, beanDefinition);
            Assert.AreEqual(definition.PrimaryKey, primaryKey);
            Assert.IsNotNull(primaryKey.GetValue(bean));
            Assert.IsNotNull(primaryKeyValue);
            Assert.AreEqual(primaryKeyValue, primaryKey.GetValue(bean));
            int id = (int)primaryKeyValue;
            int rowAffected = 1;
            if (id == 6) {
                rowAffected = 0;
            } else if (id == 7) {
                rowAffected = 2;
            }
            return new TestDataReader(id, rowAffected);
        }

        /// <summary>
        /// Supprime tous les objets correspondant aux critères.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="tableName">Nom de la table.</param>
        /// <param name="criteria">Critères de suppression.</param>
        /// <returns>Nombre de lignes supprimées.</returns>
        protected override int DeleteAllByCriteria(string commandName, string tableName, FilterCriteria criteria) {
            FilterCriteria filter = (FilterCriteria)criteria;
            foreach (FilterCriteriaParam parameter in filter.Parameters) {
                if (parameter.ColumnName == "BEA_ID") {
                    int id = (int)parameter.Value;
                    if (id == 10) {
                        return 0;
                    } else if (id == 20) {
                        return 2;
                    }
                }
            }
            return 1;
        }

        /// <summary>
        /// Crée une nouvelle commande à partir d'une requête.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="commandType">Type de la commande.</param>
        protected override AbstractSqlCommand CreateSqlCommand(string commandName, CommandType commandType) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Insère un nouvel enregistrement.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="bean">Bean à insérer.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <param name="primaryKey">Définition de la clef primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clef primaire.</param>
        /// <returns>Bean inséré.</returns>
        protected override IDataReader Insert(string commandName, T bean, BeanDefinition beanDefinition,
                BeanPropertyDescriptor primaryKey, ColumnSelector columnSelector, object primaryKeyValue) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Insère tous les éléments.
        /// </summary>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="collection">Collection des éléments à insérer.</param>
        /// <param name="beanDefinition">Définition du bean.</param>
        /// <returns>Liste des éléments insérés.</returns>
        protected override ICollection<T> InsertAll(string commandName, ICollection<T> collection, BeanDefinition beanDefinition) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Ajout du paramètre en entrée de la commande envoyée à SQL Server.
        /// </summary>
        /// <param name="parameters">Collection des paramètres dans laquelle ajouter le nouveau paramètre.</param>
        /// <param name="primaryKeyName">Nom de la clé primaire.</param>
        /// <param name="primaryKeyValue">Valeur de la clé primaire.</param>
        protected override void AddPrimaryKeyParameter(SqlServerParameterCollection parameters, string primaryKeyName, object primaryKeyValue) {
        }

        /// <summary>
        /// Ajoute un paramètre à une collection avec sa valeur.
        /// </summary>
        /// <param name="parameters">Collection de paramètres dans laquelle le nouveau paramètre est créé.</param>
        /// <param name="property">Propriété correspondant au paramètre.</param>
        /// <param name="value">Valeur du paramètre.</param>
        /// <returns>Paramètre ajouté.</returns>
        protected override SqlServerParameter AddParameter(SqlServerParameterCollection parameters, BeanPropertyDescriptor property, object value) {
            return parameters.AddWithValue(property.MemberName, value);
        }
    }
}
