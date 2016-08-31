using System;
using System.Collections.Generic;
using System.Configuration;
using System.Transactions;
using Kinetix.Broker;
using Kinetix.Data.SqlClient;
using Kinetix.Data.SqlClient.Test;
#if NUnit
    using NUnit.Framework; 
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Classe de test du store Sql Server.
    /// </summary>
    [TestFixture]
    public class SqlServerStoreTest {
        /// <summary>
        /// Initialise l'environnement pour les tests.
        /// </summary>
        [SetUp]
        public void Setup() {
            System.Configuration.Configuration config = ConfigurationManager.OpenMachineConfiguration();
            ConfigurationSection providerSection = config.GetSection("DbProviderFactories");
            config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("test", "test", "Kinetix.Test.DbProvider"));
            config.Save();
            ConfigurationManager.RefreshSection("connectionStrings");
            SqlServerManager.Instance.RegisterProviderFactory("Kinetix.Test.DbProvider", new TestDbProviderFactory());
        }

        /// <summary>
        /// Nettoye l'environnement de test.
        /// </summary>
        [TearDown]
        public void TearDown() {
            System.Configuration.Configuration config = ConfigurationManager.OpenMachineConfiguration();
            config.ConnectionStrings.ConnectionStrings.Remove("test");
            config.Save();
            ConfigurationManager.RefreshSection("connectionStrings");
        }

        /// <summary>
        /// Test le constructeur.
        /// </summary>
        [Test]
        public void TestConstructeur() {
            SqlServerStore<Bean> store = new SqlServerStore<Bean>("test");
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestInsert() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                Assert.AreEqual(1, store.Put(new Kinetix.Data.SqlClient.Test.Bean()));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("insert into BEAN(BEA_LONG, BEA_SHORT, BEA_GUID, BEA_FLOAT, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTES, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING) values (@BEA_LONG, @BEA_SHORT, @BEA_GUID, @BEA_FLOAT, @BEA_DOUBLE, @BEA_DECIMAL, @BEA_DATETIME, @BEA_CHARS, @BEA_CHAR, @BEA_BYTES, @BEA_BYTE, @BEA_BOOL, @BEA_INT, @BEA_STRING)\nselect cast(SCOPE_IDENTITY() as int)"));
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestInsertWithDoNothing() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.InsertValue = new ValueRule(null, ActionRule.DoNothing);
                store.AddRule(rule);
                Assert.AreEqual(1, store.Put(new Kinetix.Data.SqlClient.Test.Bean()));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("insert into BEAN(BEA_LONG, BEA_SHORT, BEA_GUID, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTES, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING) values (@BEA_LONG, @BEA_SHORT, @BEA_GUID, @BEA_DOUBLE, @BEA_DECIMAL, @BEA_DATETIME, @BEA_CHARS, @BEA_CHAR, @BEA_BYTES, @BEA_BYTE, @BEA_BOOL, @BEA_INT, @BEA_STRING)\nselect cast(SCOPE_IDENTITY() as int)"));
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestInsertWithUpdate() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.InsertValue = new ValueRule(3, ActionRule.Update);
                store.AddRule(rule);
                Assert.AreEqual(1, store.Put(new Kinetix.Data.SqlClient.Test.Bean()));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("insert into BEAN(BEA_LONG, BEA_SHORT, BEA_GUID, BEA_FLOAT, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTES, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING) values (@BEA_LONG, @BEA_SHORT, @BEA_GUID, @BEA_FLOAT, @BEA_DOUBLE, @BEA_DECIMAL, @BEA_DATETIME, @BEA_CHARS, @BEA_CHAR, @BEA_BYTES, @BEA_BYTE, @BEA_BOOL, @BEA_INT, @BEA_STRING)\nselect cast(SCOPE_IDENTITY() as int)"));
                Assert.AreEqual(3, command.Parameters["@BEA_FLOAT"].Value);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestInsertWithCheck() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.InsertValue = new ValueRule(3, ActionRule.Check);
                store.AddRule(rule);
                Assert.AreEqual(1, store.Put(new Kinetix.Data.SqlClient.Test.Bean()));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("insert into BEAN(BEA_LONG, BEA_SHORT, BEA_GUID, BEA_FLOAT, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTES, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING) values (@BEA_LONG, @BEA_SHORT, @BEA_GUID, @BEA_FLOAT, @BEA_DOUBLE, @BEA_DECIMAL, @BEA_DATETIME, @BEA_CHARS, @BEA_CHAR, @BEA_BYTES, @BEA_BYTE, @BEA_BOOL, @BEA_INT, @BEA_STRING)\nselect cast(SCOPE_IDENTITY() as int)", command.CommandText);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestInsertWithIncrementalUpdate() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<Kinetix.Data.SqlClient.Test.PkBean> list = new List<Kinetix.Data.SqlClient.Test.PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.InsertValue = new ValueRule(3, ActionRule.IncrementalUpdate);
                store.AddRule(rule);
                Assert.AreEqual(1, store.Put(new Kinetix.Data.SqlClient.Test.Bean()));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("insert into BEAN(BEA_LONG, BEA_SHORT, BEA_GUID, BEA_FLOAT, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTES, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING) values (@BEA_LONG, @BEA_SHORT, @BEA_GUID, @BEA_FLOAT, @BEA_DOUBLE, @BEA_DECIMAL, @BEA_DATETIME, @BEA_CHARS, @BEA_CHAR, @BEA_BYTES, @BEA_BYTE, @BEA_BOOL, @BEA_INT, @BEA_STRING)\nselect cast(SCOPE_IDENTITY() as int)", command.CommandText);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestInsertWithValueRuleNull() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                store.AddRule(rule);
                Assert.AreEqual(1, store.Put(new Kinetix.Data.SqlClient.Test.Bean()));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("insert into BEAN(BEA_LONG, BEA_SHORT, BEA_GUID, BEA_FLOAT, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTES, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING) values (@BEA_LONG, @BEA_SHORT, @BEA_GUID, @BEA_FLOAT, @BEA_DOUBLE, @BEA_DECIMAL, @BEA_DATETIME, @BEA_CHARS, @BEA_CHAR, @BEA_BYTES, @BEA_BYTE, @BEA_BOOL, @BEA_INT, @BEA_STRING)\nselect cast(SCOPE_IDENTITY() as int)"));
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestUpdate() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                Kinetix.Data.SqlClient.Test.Bean bean = new Kinetix.Data.SqlClient.Test.Bean();
                bean.Pk = 1;
                Assert.AreEqual(1, store.Put(bean));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("update BEAN set BEA_LONG = @BEA_LONG, BEA_SHORT = @BEA_SHORT, BEA_GUID = @BEA_GUID, BEA_FLOAT = @BEA_FLOAT, BEA_DOUBLE = @BEA_DOUBLE, BEA_DECIMAL = @BEA_DECIMAL, BEA_DATETIME = @BEA_DATETIME, BEA_CHARS = @BEA_CHARS, BEA_CHAR = @BEA_CHAR, BEA_BYTES = @BEA_BYTES, BEA_BYTE = @BEA_BYTE, BEA_BOOL = @BEA_BOOL, BEA_INT = @BEA_INT, BEA_STRING = @BEA_STRING where BEA_PK = @BEA_PK"));
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestUpdateWithValueRuleNull() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                store.AddRule(rule);
                Kinetix.Data.SqlClient.Test.Bean bean = new Kinetix.Data.SqlClient.Test.Bean();
                bean.Pk = 1;
                Assert.AreEqual(1, store.Put(bean));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("update BEAN set BEA_LONG = @BEA_LONG, BEA_SHORT = @BEA_SHORT, BEA_GUID = @BEA_GUID, BEA_FLOAT = @BEA_FLOAT, BEA_DOUBLE = @BEA_DOUBLE, BEA_DECIMAL = @BEA_DECIMAL, BEA_DATETIME = @BEA_DATETIME, BEA_CHARS = @BEA_CHARS, BEA_CHAR = @BEA_CHAR, BEA_BYTES = @BEA_BYTES, BEA_BYTE = @BEA_BYTE, BEA_BOOL = @BEA_BOOL, BEA_INT = @BEA_INT, BEA_STRING = @BEA_STRING where BEA_PK = @BEA_PK"));
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestUpdateWithDoNothing() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.UpdateValue = new ValueRule(null, ActionRule.DoNothing);
                store.AddRule(rule);
                Kinetix.Data.SqlClient.Test.Bean bean = new Kinetix.Data.SqlClient.Test.Bean();
                bean.Pk = 1;
                Assert.AreEqual(1, store.Put(bean));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("update BEAN set BEA_LONG = @BEA_LONG, BEA_SHORT = @BEA_SHORT, BEA_GUID = @BEA_GUID, BEA_DOUBLE = @BEA_DOUBLE, BEA_DECIMAL = @BEA_DECIMAL, BEA_DATETIME = @BEA_DATETIME, BEA_CHARS = @BEA_CHARS, BEA_CHAR = @BEA_CHAR, BEA_BYTES = @BEA_BYTES, BEA_BYTE = @BEA_BYTE, BEA_BOOL = @BEA_BOOL, BEA_INT = @BEA_INT, BEA_STRING = @BEA_STRING where BEA_PK = @BEA_PK"));
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestUpdateWithUpdate() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.UpdateValue = new ValueRule(3, ActionRule.Update);
                store.AddRule(rule);
                Kinetix.Data.SqlClient.Test.Bean bean = new Kinetix.Data.SqlClient.Test.Bean();
                bean.Pk = 1;
                Assert.AreEqual(1, store.Put(bean));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("update BEAN set BEA_LONG = @BEA_LONG, BEA_SHORT = @BEA_SHORT, BEA_GUID = @BEA_GUID, BEA_FLOAT = @BEA_FLOAT, BEA_DOUBLE = @BEA_DOUBLE, BEA_DECIMAL = @BEA_DECIMAL, BEA_DATETIME = @BEA_DATETIME, BEA_CHARS = @BEA_CHARS, BEA_CHAR = @BEA_CHAR, BEA_BYTES = @BEA_BYTES, BEA_BYTE = @BEA_BYTE, BEA_BOOL = @BEA_BOOL, BEA_INT = @BEA_INT, BEA_STRING = @BEA_STRING where BEA_PK = @BEA_PK"));
                Assert.AreEqual(3, command.Parameters["@BEA_FLOAT"].Value);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestUpdateWithIncrementalUpdate() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.UpdateValue = new ValueRule(3, ActionRule.IncrementalUpdate);
                store.AddRule(rule);
                Kinetix.Data.SqlClient.Test.Bean bean = new Kinetix.Data.SqlClient.Test.Bean();
                bean.Pk = 1;
                Assert.AreEqual(1, store.Put(bean));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("update BEAN set BEA_LONG = @BEA_LONG, BEA_SHORT = @BEA_SHORT, BEA_GUID = @BEA_GUID, BEA_FLOAT = BEA_FLOAT + @BEA_FLOAT, BEA_DOUBLE = @BEA_DOUBLE, BEA_DECIMAL = @BEA_DECIMAL, BEA_DATETIME = @BEA_DATETIME, BEA_CHARS = @BEA_CHARS, BEA_CHAR = @BEA_CHAR, BEA_BYTES = @BEA_BYTES, BEA_BYTE = @BEA_BYTE, BEA_BOOL = @BEA_BOOL, BEA_INT = @BEA_INT, BEA_STRING = @BEA_STRING where BEA_PK = @BEA_PK"));
                Assert.AreEqual(3, command.Parameters["@BEA_FLOAT"].Value);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestUpdateWithCheck() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.UpdateValue = new ValueRule(3, ActionRule.Check);
                store.AddRule(rule);
                Kinetix.Data.SqlClient.Test.Bean bean = new Kinetix.Data.SqlClient.Test.Bean();
                bean.Pk = 1;
                Assert.AreEqual(1, store.Put(bean));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("update BEAN set BEA_LONG = @BEA_LONG, BEA_SHORT = @BEA_SHORT, BEA_GUID = @BEA_GUID, BEA_FLOAT = BEA_FLOAT + @BEA_FLOAT, BEA_DOUBLE = @BEA_DOUBLE, BEA_DECIMAL = @BEA_DECIMAL, BEA_DATETIME = @BEA_DATETIME, BEA_CHARS = @BEA_CHARS, BEA_CHAR = @BEA_CHAR, BEA_BYTES = @BEA_BYTES, BEA_BYTE = @BEA_BYTE, BEA_BOOL = @BEA_BOOL, BEA_INT = @BEA_INT, BEA_STRING = @BEA_STRING where BEA_PK = @BEA_PK", command.CommandText);
                Assert.AreEqual(3, command.Parameters["@BEA_FLOAT"].Value);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestUpdateWithWhereValueRuleNull() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                store.AddRule(rule);
                Kinetix.Data.SqlClient.Test.Bean bean = new Kinetix.Data.SqlClient.Test.Bean();
                bean.Pk = 1;
                Assert.AreEqual(1, store.Put(bean));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("update BEAN set BEA_LONG = @BEA_LONG, BEA_SHORT = @BEA_SHORT, BEA_GUID = @BEA_GUID, BEA_FLOAT = @BEA_FLOAT, BEA_DOUBLE = @BEA_DOUBLE, BEA_DECIMAL = @BEA_DECIMAL, BEA_DATETIME = @BEA_DATETIME, BEA_CHARS = @BEA_CHARS, BEA_CHAR = @BEA_CHAR, BEA_BYTES = @BEA_BYTES, BEA_BYTE = @BEA_BYTE, BEA_BOOL = @BEA_BOOL, BEA_INT = @BEA_INT, BEA_STRING = @BEA_STRING where BEA_PK = @BEA_PK"));
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestUpdateWithWhereDoNothing() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.WhereValue = new ValueRule(null, ActionRule.DoNothing);
                store.AddRule(rule);
                Kinetix.Data.SqlClient.Test.Bean bean = new Kinetix.Data.SqlClient.Test.Bean();
                bean.Pk = 1;
                Assert.AreEqual(1, store.Put(bean));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("update BEAN set BEA_LONG = @BEA_LONG, BEA_SHORT = @BEA_SHORT, BEA_GUID = @BEA_GUID, BEA_FLOAT = @BEA_FLOAT, BEA_DOUBLE = @BEA_DOUBLE, BEA_DECIMAL = @BEA_DECIMAL, BEA_DATETIME = @BEA_DATETIME, BEA_CHARS = @BEA_CHARS, BEA_CHAR = @BEA_CHAR, BEA_BYTES = @BEA_BYTES, BEA_BYTE = @BEA_BYTE, BEA_BOOL = @BEA_BOOL, BEA_INT = @BEA_INT, BEA_STRING = @BEA_STRING where BEA_PK = @BEA_PK"));
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestUpdateWithWhereCheck() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.WhereValue = new ValueRule(3, ActionRule.Check);
                store.AddRule(rule);
                Kinetix.Data.SqlClient.Test.Bean bean = new Kinetix.Data.SqlClient.Test.Bean();
                bean.Pk = 1;
                Assert.AreEqual(1, store.Put(bean));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("update BEAN set BEA_LONG = @BEA_LONG, BEA_SHORT = @BEA_SHORT, BEA_GUID = @BEA_GUID, BEA_FLOAT = @BEA_FLOAT, BEA_DOUBLE = @BEA_DOUBLE, BEA_DECIMAL = @BEA_DECIMAL, BEA_DATETIME = @BEA_DATETIME, BEA_CHARS = @BEA_CHARS, BEA_CHAR = @BEA_CHAR, BEA_BYTES = @BEA_BYTES, BEA_BYTE = @BEA_BYTE, BEA_BOOL = @BEA_BOOL, BEA_INT = @BEA_INT, BEA_STRING = @BEA_STRING where BEA_PK = @BEA_PK and BEA_FLOAT = @RU_BEA_FLOAT"));
                Assert.AreEqual(3, command.Parameters["@RU_BEA_FLOAT"].Value);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestUpdateWithWhereUpdate() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.WhereValue = new ValueRule(null, ActionRule.Update);
                store.AddRule(rule);
                Kinetix.Data.SqlClient.Test.Bean bean = new Kinetix.Data.SqlClient.Test.Bean();
                bean.Pk = 1;
                Assert.AreEqual(1, store.Put(bean));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("update BEAN set BEA_LONG = @BEA_LONG, BEA_SHORT = @BEA_SHORT, BEA_GUID = @BEA_GUID, BEA_FLOAT = @BEA_FLOAT, BEA_DOUBLE = @BEA_DOUBLE, BEA_DECIMAL = @BEA_DECIMAL, BEA_DATETIME = @BEA_DATETIME, BEA_CHARS = @BEA_CHARS, BEA_CHAR = @BEA_CHAR, BEA_BYTES = @BEA_BYTES, BEA_BYTE = @BEA_BYTE, BEA_BOOL = @BEA_BOOL, BEA_INT = @BEA_INT, BEA_STRING = @BEA_STRING where BEA_PK = @BEA_PK", command.CommandText);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestUpdateWithWhereIncrementalUpdate() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<PkBean> list = new List<PkBean>();
                PkBean b = new PkBean();
                b.Pk = 1;
                list.Add(b);
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                TestStoreRule rule = new TestStoreRule("DataFloat");
                rule.WhereValue = new ValueRule(null, ActionRule.IncrementalUpdate);
                store.AddRule(rule);
                Kinetix.Data.SqlClient.Test.Bean bean = new Kinetix.Data.SqlClient.Test.Bean();
                bean.Pk = 1;
                Assert.AreEqual(1, store.Put(bean));
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("update BEAN set BEA_LONG = @BEA_LONG, BEA_SHORT = @BEA_SHORT, BEA_GUID = @BEA_GUID, BEA_FLOAT = @BEA_FLOAT, BEA_DOUBLE = @BEA_DOUBLE, BEA_DECIMAL = @BEA_DECIMAL, BEA_DATETIME = @BEA_DATETIME, BEA_CHARS = @BEA_CHARS, BEA_CHAR = @BEA_CHAR, BEA_BYTES = @BEA_BYTES, BEA_BYTE = @BEA_BYTE, BEA_BOOL = @BEA_BOOL, BEA_INT = @BEA_INT, BEA_STRING = @BEA_STRING where BEA_PK = @BEA_PK", command.CommandText);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestDelete() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                store.Remove(1);
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.IsTrue(command.CommandText.Contains("delete from BEAN where BEA_PK = @BEA_PK"));
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestRemoveAllLikeWithoutCriteria() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerStore<Bean> store = new SqlServerStore<Bean>("test");
                store.RemoveAllByCriteria(new FilterCriteria());
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("delete from BEAN where BEA_PK = @BEA_PK", command.CommandText);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestLoadAllLike() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<Kinetix.Data.SqlClient.Test.Bean> list = new List<Kinetix.Data.SqlClient.Test.Bean>();
                for (byte i = 0; i < 10; i++) {
                    Kinetix.Data.SqlClient.Test.Bean b = new Kinetix.Data.SqlClient.Test.Bean();
                    b.Pk = i;
                    b.DataBool = true;
                    b.DataByte = i;
                    b.DataBytes = new byte[i];
                    b.DataChar = (char)(i + 64);
                    b.DataChars = new char[i];
                    b.DataDateTime = DateTime.Now;
                    b.DataDecimal = i;
                    b.DataDouble = i;
                    b.DataFloat = i;
                    b.DataGuid = Guid.NewGuid();
                    b.DataInt = i;
                    b.DataLong = i;
                    b.DataShort = i;
                    b.DataString = i.ToString();
                    list.Add(b);
                }
                list.Add(new Kinetix.Data.SqlClient.Test.Bean());
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                FilterCriteria criteria = new FilterCriteria().Equals(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, 3);
                QueryParameter sort = new QueryParameter(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, SortOrder.Asc);
                store.LoadAllByCriteria(null, criteria, sort);
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("select BEA_PK, BEA_LONG, BEA_SHORT, BEA_GUID, BEA_FLOAT, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING from BEAN where BEA_FLOAT = @BEA_FLOAT order by BEA_FLOAT asc", command.CommandText);
                Assert.AreEqual(3, command.Parameters["@BEA_FLOAT"].Value);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestLoadAllLikeTop() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<Kinetix.Data.SqlClient.Test.Bean> list = new List<Kinetix.Data.SqlClient.Test.Bean>();
                for (byte i = 0; i < 10; i++) {
                    Kinetix.Data.SqlClient.Test.Bean b = new Kinetix.Data.SqlClient.Test.Bean();
                    b.Pk = i;
                    b.DataBool = true;
                    b.DataByte = i;
                    b.DataBytes = new byte[i];
                    b.DataChar = (char)(i + 64);
                    b.DataChars = new char[i];
                    b.DataDateTime = DateTime.Now;
                    b.DataDecimal = i;
                    b.DataDouble = i;
                    b.DataFloat = i;
                    b.DataGuid = Guid.NewGuid();
                    b.DataInt = i;
                    b.DataLong = i;
                    b.DataShort = i;
                    b.DataString = i.ToString();
                    list.Add(b);
                }
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                FilterCriteria criteria = new FilterCriteria().Equals(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, 3);
                QueryParameter sort = new QueryParameter(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, SortOrder.Asc, 10);
                store.LoadAllByCriteria(null, criteria, sort);
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("select top(@top) BEA_PK, BEA_LONG, BEA_SHORT, BEA_GUID, BEA_FLOAT, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING from BEAN where BEA_FLOAT = @BEA_FLOAT order by BEA_FLOAT asc", command.CommandText);
                Assert.AreEqual(10 + 1, command.Parameters["@top"].Value);
                Assert.AreEqual(3, command.Parameters["@BEA_FLOAT"].Value);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestLoadAllLikeAnd() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<Kinetix.Data.SqlClient.Test.Bean> list = new List<Kinetix.Data.SqlClient.Test.Bean>();
                for (byte i = 0; i < 10; i++) {
                    Kinetix.Data.SqlClient.Test.Bean b = new Kinetix.Data.SqlClient.Test.Bean();
                    b.Pk = i;
                    b.DataBool = true;
                    b.DataByte = i;
                    b.DataBytes = new byte[i];
                    b.DataChar = (char)(i + 64);
                    b.DataChars = new char[i];
                    b.DataDateTime = DateTime.Now;
                    b.DataDecimal = i;
                    b.DataDouble = i;
                    b.DataFloat = i;
                    b.DataGuid = Guid.NewGuid();
                    b.DataInt = i;
                    b.DataLong = i;
                    b.DataShort = i;
                    b.DataString = i.ToString();
                    list.Add(b);
                }
                list.Add(new Kinetix.Data.SqlClient.Test.Bean());
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                FilterCriteria criteria = new FilterCriteria()
                    .Equals(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, 3)
                    .Equals(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_DOUBLE, 3);
                QueryParameter sort = new QueryParameter(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, SortOrder.Asc);
                store.LoadAllByCriteria(null, criteria, sort);
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("select BEA_PK, BEA_LONG, BEA_SHORT, BEA_GUID, BEA_FLOAT, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING from BEAN where BEA_FLOAT = @BEA_FLOAT and BEA_DOUBLE = @BEA_DOUBLE order by BEA_FLOAT asc", command.CommandText);
                Assert.AreEqual(3, command.Parameters["@BEA_FLOAT"].Value);
                Assert.AreEqual(3, command.Parameters["@BEA_DOUBLE"].Value);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestLoadAllByCriteriaString() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<Kinetix.Data.SqlClient.Test.Bean> list = new List<Kinetix.Data.SqlClient.Test.Bean>();
                for (byte i = 0; i < 10; i++) {
                    Kinetix.Data.SqlClient.Test.Bean b = new Kinetix.Data.SqlClient.Test.Bean();
                    b.Pk = i;
                    b.DataBool = true;
                    b.DataByte = i;
                    b.DataBytes = new byte[i];
                    b.DataChar = (char)(i + 64);
                    b.DataChars = new char[i];
                    b.DataDateTime = DateTime.Now;
                    b.DataDecimal = i;
                    b.DataDouble = i;
                    b.DataFloat = i;
                    b.DataGuid = Guid.NewGuid();
                    b.DataInt = i;
                    b.DataLong = i;
                    b.DataShort = i;
                    b.DataString = i.ToString();
                    list.Add(b);
                }
                list.Add(new Kinetix.Data.SqlClient.Test.Bean());
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                FilterCriteria criteria = new FilterCriteria()
                    .Equals(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, 3)
                    .StartsWith(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_STRING, "test")
                    .NotStartsWith(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_STRING, "test2");
                QueryParameter sort = new QueryParameter(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, SortOrder.Asc);
                store.LoadAllByCriteria(null, criteria, sort);
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("select BEA_PK, BEA_LONG, BEA_SHORT, BEA_GUID, BEA_FLOAT, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING from BEAN where BEA_FLOAT = @BEA_FLOAT and BEA_STRING LIKE @BEA_STRING + '%' and BEA_STRING NOT LIKE @BEA_STRING2 + '%' order by BEA_FLOAT asc", command.CommandText);
                Assert.AreEqual(3, command.Parameters["@BEA_FLOAT"].Value);
                Assert.AreEqual("test", command.Parameters["@BEA_STRING"].Value);
            }
        }

        /// <summary>
        /// Test LoadAllLike avec un not.
        /// </summary>
        [Test]
        public void TestLoadAllNotLikeString() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<Kinetix.Data.SqlClient.Test.Bean> list = new List<Kinetix.Data.SqlClient.Test.Bean>();
                for (byte i = 0; i < 10; i++) {
                    Kinetix.Data.SqlClient.Test.Bean b = new Kinetix.Data.SqlClient.Test.Bean();
                    b.Pk = i;
                    b.DataBool = true;
                    b.DataByte = i;
                    b.DataBytes = new byte[i];
                    b.DataChar = (char)(i + 64);
                    b.DataChars = new char[i];
                    b.DataDateTime = DateTime.Now;
                    b.DataDecimal = i;
                    b.DataDouble = i;
                    b.DataFloat = i;
                    b.DataGuid = Guid.NewGuid();
                    b.DataInt = i;
                    b.DataLong = i;
                    b.DataShort = i;
                    b.DataString = i.ToString();
                    list.Add(b);
                }
                list.Add(new Kinetix.Data.SqlClient.Test.Bean());
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                FilterCriteria criteria = new FilterCriteria().Equals(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, 3).NotStartsWith(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_STRING, "test2");
                QueryParameter sort = new QueryParameter(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, SortOrder.Asc);
                store.LoadAllByCriteria(null, criteria, sort);
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("select BEA_PK, BEA_LONG, BEA_SHORT, BEA_GUID, BEA_FLOAT, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING from BEAN where BEA_FLOAT = @BEA_FLOAT and BEA_STRING NOT LIKE @BEA_STRING + '%' order by BEA_FLOAT asc", command.CommandText);
                Assert.AreEqual(3, command.Parameters["@BEA_FLOAT"].Value);
                Assert.AreEqual("test2", command.Parameters["@BEA_STRING"].Value);
            }
        }

        /// <summary>
        /// Test l'insertion.
        /// </summary>
        [Test]
        public void TestLoadAllLikeSortDesc() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                List<Kinetix.Data.SqlClient.Test.Bean> list = new List<Kinetix.Data.SqlClient.Test.Bean>();
                for (byte i = 0; i < 10; i++) {
                    Kinetix.Data.SqlClient.Test.Bean b = new Kinetix.Data.SqlClient.Test.Bean();
                    b.Pk = i;
                    b.DataBool = true;
                    b.DataByte = i;
                    b.DataBytes = new byte[i];
                    b.DataChar = (char)(i + 64);
                    b.DataChars = new char[i];
                    b.DataDateTime = DateTime.Now;
                    b.DataDecimal = i;
                    b.DataDouble = i;
                    b.DataFloat = i;
                    b.DataGuid = Guid.NewGuid();
                    b.DataInt = i;
                    b.DataLong = i;
                    b.DataShort = i;
                    b.DataString = i.ToString();
                    list.Add(b);
                }
                list.Add(new Kinetix.Data.SqlClient.Test.Bean());
                TestDbProviderFactory.DefinedNextResult(list);

                SqlServerStore<Kinetix.Data.SqlClient.Test.Bean> store = new SqlServerStore<Kinetix.Data.SqlClient.Test.Bean>("test");
                FilterCriteria criteria = new FilterCriteria().Equals(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, 3);
                QueryParameter sort = new QueryParameter(Kinetix.Data.SqlClient.Test.Bean.Cols.BEA_FLOAT, SortOrder.Desc);
                store.LoadAllByCriteria(null, criteria, sort);
                Kinetix.Data.SqlClient.Test.TestDbCommand command = Kinetix.Data.SqlClient.Test.TestDbCommand.LastCommand;
                Assert.AreEqual("select BEA_PK, BEA_LONG, BEA_SHORT, BEA_GUID, BEA_FLOAT, BEA_DOUBLE, BEA_DECIMAL, BEA_DATETIME, BEA_CHARS, BEA_CHAR, BEA_BYTE, BEA_BOOL, BEA_INT, BEA_STRING from BEAN where BEA_FLOAT = @BEA_FLOAT order by BEA_FLOAT desc", command.CommandText);
                Assert.AreEqual(3, command.Parameters["@BEA_FLOAT"].Value);
            }
        }
    }
}
