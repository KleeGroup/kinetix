using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Transactions;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
    using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
    using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
#endif

namespace Kinetix.Data.SqlClient.Test {
    /// <summary>
    /// Test les collections de paramèters Sql.
    /// </summary>
    [TestFixture]
    public class SqlServerParameterCollectionTest {

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
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                Assert.IsNotNull(command.Parameters);
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestAdd() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.IsTrue(((IList)coll).Contains(param));
            }
        }

        /// <summary>
        /// Test de l'ajout d'un bean null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestAddBeanPropertiesNull() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                command.Parameters.AddBeanProperties(null);
            }
        }

        /// <summary>
        /// Test de l'ajout d'un bean.
        /// </summary>
        [Test]
        public void TestAddBeanProperties() {
            PkBean item = new PkBean() { Pk = 1000 };
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                ICollection<SqlServerParameter> cols = command.Parameters.AddBeanProperties(item);
                Assert.IsTrue(command.Parameters.Contains("@BEA_PK"));
                Assert.AreEqual(command.Parameters["@BEA_PK"].Value, item.Pk);
            }
        }

        /// <summary>
        /// Test de l'ajout d'un bean contenant des propriétés non primitives.
        /// </summary>
        /// <todo who="OGY|FCN" type="IGNORE">Test désactivé, a revoir, le test est il encore pertinant.</todo>
        [Test]
        [Ignore()]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestAddBeanPropertiesNonPrimitiveProperty() {
            InheritBean item = new InheritBean();
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                ICollection<SqlServerParameter> cols = command.Parameters.AddBeanProperties(item);
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestAddFullTextWithValue() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                coll.AddFullTextWithValue("Param1", "le petit chat");
                Assert.AreEqual("(FORMSOF(INFLECTIONAL, \"petit\") OR FORMSOF(THESAURUS, \"petit\")) AND (FORMSOF(INFLECTIONAL, \"chat\") OR FORMSOF(THESAURUS, \"chat\"))", coll["@Param1"].Value);
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestICollectionAdd() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                ((ICollection<SqlServerParameter>)coll).Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.IsTrue(((IList)coll).Contains(param));
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestIListAdd() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                ((IList)coll).Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.IsTrue(((IList)coll).Contains(param));
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestItem() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(param, coll[0]);
                Assert.AreEqual(param, coll["Param1"]);
                Assert.AreEqual(param, ((IList)coll)[0]);
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestSetItem() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(param, coll[0]);
                Assert.AreEqual(param, coll["Param1"]);
                Assert.AreEqual(param, ((IList)coll)[0]);

                param = command.CreateParameter();
                param.ParameterName = "Param2";
                coll[0] = param;

                Assert.AreEqual(1, coll.Count);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.AreEqual(param, coll[0]);
                Assert.AreEqual(param, coll["Param2"]);
                Assert.AreEqual(param, ((IList)coll)[0]);
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestIListSetItem() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(param, coll[0]);
                Assert.AreEqual(param, coll["Param1"]);
                Assert.AreEqual(param, ((IList)coll)[0]);

                param = command.CreateParameter();
                param.ParameterName = "Param2";
                ((IList)coll)[0] = param;

                Assert.AreEqual(1, coll.Count);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.AreEqual(param, coll[0]);
                Assert.AreEqual(param, coll["Param2"]);
                Assert.AreEqual(param, ((IList)coll)[0]);
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestIDataParameterCollectionSetItem() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(param, coll[0]);
                Assert.AreEqual(param, coll["Param1"]);
                Assert.AreEqual(param, ((IDataParameterCollection)coll)["Param1"]);

                param = command.CreateParameter();
                param.ParameterName = "Param2";
                ((IDataParameterCollection)coll)["Param1"] = param;

                Assert.AreEqual(1, coll.Count);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.AreEqual(param, coll[0]);
                Assert.AreEqual(param, coll["Param2"]);
                Assert.AreEqual(param, ((IList)coll)[0]);
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestSetItemByName() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(param, coll[0]);
                Assert.AreEqual(param, coll["Param1"]);
                Assert.AreEqual(param, ((IList)coll)[0]);

                param = command.CreateParameter();
                param.ParameterName = "Param2";
                coll["Param1"] = param;

                Assert.AreEqual(1, coll.Count);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.AreEqual(param, coll[0]);
                Assert.AreEqual(param, coll["Param2"]);
                Assert.AreEqual(param, ((IList)coll)[0]);
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestIndexOf() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(0, coll.IndexOf(param));
                Assert.AreEqual(0, coll.IndexOf("Param1"));
                Assert.AreEqual(0, ((IList)coll).IndexOf(param));
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestClear() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(1, coll.Count);
                coll.Clear();
                Assert.IsFalse(coll.Contains(param));
                Assert.IsFalse(coll.Contains("Param1"));
                Assert.AreEqual(0, coll.Count);
            }
        }

        /// <summary>
        /// Test l'insert.
        /// </summary>
        [Test]
        public void TestInsert() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(1, coll.Count);

                SqlServerParameter param2 = command.CreateParameter();
                param2.ParameterName = "Param2";
                coll.Insert(0, param2);
                Assert.IsTrue(coll.Contains(param2));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.AreEqual(2, coll.Count);

                Assert.AreEqual(0, coll.IndexOf(param2));
                Assert.AreEqual(1, coll.IndexOf(param));
            }
        }

        /// <summary>
        /// Test l'insert.
        /// </summary>
        [Test]
        public void TestIListInsert() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(1, coll.Count);

                SqlServerParameter param2 = command.CreateParameter();
                param2.ParameterName = "Param2";
                ((IList)coll).Insert(0, param2);
                Assert.IsTrue(coll.Contains(param2));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.AreEqual(2, coll.Count);

                Assert.AreEqual(0, coll.IndexOf(param2));
                Assert.AreEqual(1, coll.IndexOf(param));
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestRemove() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(1, coll.Count);

                SqlServerParameter param2 = command.CreateParameter();
                param2.ParameterName = "Param2";
                coll.Add(param2);
                Assert.IsTrue(coll.Contains(param2));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.AreEqual(2, coll.Count);

                coll.Remove(param);
                Assert.IsTrue(coll.Contains(param2));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.IsFalse(coll.Contains(param));
                Assert.IsFalse(coll.Contains("Param1"));
                Assert.AreEqual(1, coll.Count);
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestRemoveAt() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(1, coll.Count);
                Assert.AreEqual(0, coll.IndexOf(param));

                SqlServerParameter param2 = command.CreateParameter();
                param2.ParameterName = "Param2";
                coll.Add(param2);
                Assert.IsTrue(coll.Contains(param2));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.AreEqual(2, coll.Count);

                Assert.AreEqual(0, coll.IndexOf(param));
                Assert.AreEqual(1, coll.IndexOf(param2));

                coll.RemoveAt(0);
                Assert.AreEqual(1, coll.Count);
                Assert.IsTrue(coll.Contains(param2));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.IsFalse(coll.Contains(param));
                Assert.IsFalse(coll.Contains("Param1"));
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestRemoveAtByName() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(1, coll.Count);

                SqlServerParameter param2 = command.CreateParameter();
                param2.ParameterName = "Param2";
                coll.Add(param2);
                Assert.IsTrue(coll.Contains(param2));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.AreEqual(2, coll.Count);

                coll.RemoveAt("Param1");
                Assert.IsTrue(coll.Contains(param2));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.IsFalse(coll.Contains(param));
                Assert.IsFalse(coll.Contains("Param1"));
                Assert.AreEqual(1, coll.Count);
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestIListRemove() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.AreEqual(1, coll.Count);

                SqlServerParameter param2 = command.CreateParameter();
                param2.ParameterName = "Param2";
                coll.Add(param2);
                Assert.IsTrue(coll.Contains(param2));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.AreEqual(2, coll.Count);

                ((IList)coll).Remove(param);
                Assert.IsTrue(coll.Contains(param2));
                Assert.IsTrue(coll.Contains("Param2"));
                Assert.IsFalse(coll.Contains(param));
                Assert.IsFalse(coll.Contains("Param1"));
                Assert.AreEqual(1, coll.Count);
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestIListIsSynchronized() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                IList coll = command.Parameters;
                Assert.IsFalse(coll.IsSynchronized);
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestIListSyncRoot() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                IList coll = command.Parameters;
                Assert.IsNull(coll.SyncRoot);
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestICollectionIsSynchronized() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                ICollection coll = command.Parameters;
                Assert.IsFalse(coll.IsSynchronized);
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestICollectionSyncRoot() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                ICollection coll = command.Parameters;
                Assert.IsNull(coll.SyncRoot);
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestICollectionIsReadonly() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                ICollection<SqlServerParameter> coll = command.Parameters;
                Assert.IsFalse(coll.IsReadOnly);
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestIListIsReadonly() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                IList coll = command.Parameters;
                Assert.IsFalse(coll.IsReadOnly);
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestIListIsFixedSize() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                IList coll = command.Parameters;
                Assert.IsFalse(coll.IsFixedSize);
            }
        }

        /// <summary>
        /// Test la suppression.
        /// </summary>
        [Test]
        public void TestIListGetEnumerator() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                IList coll = command.Parameters;
                Assert.IsNotNull(coll.GetEnumerator());
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestCopyTo() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.IsTrue(((IList)coll).Contains(param));

                SqlServerParameter[] array = new SqlServerParameter[1];
                coll.CopyTo(array, 0);
                Assert.AreEqual(param, array[0]);
            }
        }

        /// <summary>
        /// Test l'ajout.
        /// </summary>
        [Test]
        public void TestICollectionCopyTo() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameterCollection coll = command.Parameters;
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                coll.Add(param);
                Assert.IsTrue(coll.Contains(param));
                Assert.IsTrue(coll.Contains("Param1"));
                Assert.IsTrue(((IList)coll).Contains(param));

                object[] array = new object[1];
                ((ICollection)coll).CopyTo(array, 0);
                Assert.AreEqual(param, array[0]);
            }
        }
    }
}
