using System.Collections.Generic;
using System.Configuration;
using System.Security.Principal;
using System.Threading;
using System.Transactions;
using Fmk.ComponentModel;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using TearDownAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute;
using System.Security.Claims;
using Kinetix.ComponentModel;
#endif

namespace Kinetix.Data.SqlClient.Test {
    /// <summary>
    /// Test unitaire de CommandParser.
    /// </summary>
    [TestFixture]
    public class CommandParserTest {
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
            SqlServerManager.Instance.RegisterConstDataTypes(this.GetType().Assembly);

            GenericPrincipal custom = new GenericPrincipal(new TestIdentity("test"), null);
            Thread.CurrentPrincipal = custom;
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
        /// Identité de test.
        /// </summary>
        private class TestIdentity : GenericIdentity
        {

            /// <summary>
            /// Crée une nouvelle instance.
            /// </summary>
            /// <param name="name">Nom.</param>
            public TestIdentity(string name)
                : base(name)
            {
                this.AddClaim(new Claim(UserIdClaim.ClaimType, "123456789"));
            }

            /// <summary>
            /// Identifiant base de données.
            /// </summary>
            public int Id
            {
                get
                {
                    return 123456789;
                }
            }

            /// <summary>
            /// Nom complet.
            /// </summary>
            public string FullName
            {
                get
                {
                    return "CommandParserTest";
                }
            }

            /// <summary>
            /// Code de la langue.
            /// </summary>
            public string LangueCode
            {
                get
                {
                    return "FR";
                }
            }
        }

        /// <summary>
        /// Test de parsing de CurrentUserId.
        /// </summary>
        [Test]
        public void TestParseCurrentUserId() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                TestDbProviderFactory.DefinedNextResult(new List<Bean>());

                SqlServerCommand command = new SqlServerCommand("test", SqlResource.ResourceManager, "SqlTestUserId");
                command.ExecuteReader();

                Assert.IsTrue(command.CommandText.Contains(":CURRENT_USER_ID IS NOT NULL"));

                command.Dispose();
            }
        }

        /// <summary>
        /// Test de parsing d'une constante.
        /// </summary>
        [Test]
        public void TestParseConstant() {
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                TestDbProviderFactory.DefinedNextResult(new List<Bean>());

                SqlServerCommand command = new SqlServerCommand("test", SqlResource.ResourceManager, "SqlTestConst");
                command.ExecuteReader();

                Assert.IsTrue(command.CommandText.Contains("'5' = 5"));

                command.Dispose();
            }
        }
       
    }
}
