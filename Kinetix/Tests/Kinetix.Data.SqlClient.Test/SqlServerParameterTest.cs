using System;
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
    /// Classe de tests des paramètres SqlServer.
    /// </summary>
    [TestFixture]
    public class SqlServerParameterTest {

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
                Assert.IsNotNull(command.CreateParameter());
            }
        }

        /// <summary>
        /// Test le type de données.
        /// </summary>
        [Test]
        public void TestDbType() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameter param = command.CreateParameter();
                param.DbType = DbType.String;
                Assert.AreEqual(DbType.String, param.DbType);
            }
        }

        /// <summary>
        /// Test la direction.
        /// </summary>
        [Test]
        public void TestDirection() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameter param = command.CreateParameter();
                param.Direction = ParameterDirection.Output;
                Assert.AreEqual(ParameterDirection.Output, param.Direction);
            }
        }

        /// <summary>
        /// Test le nom.
        /// </summary>
        [Test]
        public void TestParameterName() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameter param = command.CreateParameter();
                param.ParameterName = "Param1";
                Assert.AreEqual("Param1", param.ParameterName);
            }
        }

        /// <summary>
        /// Test la precision.
        /// </summary>
        [Test]
        public void TestPrecision() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameter param = command.CreateParameter();
                param.DbType = DbType.Decimal;
                param.Precision = 5;
                Assert.AreEqual(0, param.Precision);
            }
        }

        /// <summary>
        /// Test l'échelle.
        /// </summary>
        [Test]
        public void TestScale() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameter param = command.CreateParameter();
                param.Scale = 3;
                Assert.AreEqual(0, param.Scale);
            }
        }

        /// <summary>
        /// Test la taille.
        /// </summary>
        [Test]
        public void TestSize() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameter param = command.CreateParameter();
                param.Size = 3;
                Assert.AreEqual(3, param.Size);
            }
        }

        /// <summary>
        /// Test la colonne source.
        /// </summary>
        [Test]
        public void TestSourceColumn() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                IDataParameter param = command.CreateParameter();
                param.SourceColumn = "Column1";
                Assert.AreEqual("Column1", param.SourceColumn);
            }
        }

        /// <summary>
        /// Test la version de colonne.
        /// </summary>
        [Test]
        public void TestSourceVersion() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                IDataParameter param = command.CreateParameter();
                param.SourceVersion = DataRowVersion.Proposed;
                Assert.AreEqual(DataRowVersion.Proposed, param.SourceVersion);
            }
        }

        /// <summary>
        /// Test la nullabilité.
        /// </summary>
        [Test]
        public void TestIsNull() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                IDataParameter param = command.CreateParameter();
                Assert.IsTrue(param.IsNullable);
            }
        }

        /// <summary>
        /// Test la valeur.
        /// </summary>
        [Test]
        public void TestValue() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameter param = command.CreateParameter();
                param.Value = "32";
                Assert.AreEqual("32", param.Value);
            }
        }

        /// <summary>
        /// Test la valeur.
        /// </summary>
        [Test]
        public void TestValueNull() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameter param = command.CreateParameter();
                param.Value = null;
                Assert.IsNull(param.Value);
            }
        }

        /// <summary>
        /// Test la valeur.
        /// </summary>
        [Test]
        public void TestValueDbNull() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                SqlServerCommand command = new SqlServerCommand("test", "procTest");
                SqlServerParameter param = command.CreateParameter();
                param.Value = DBNull.Value;
                Assert.IsNull(param.Value);
            }
        }
    }
}
