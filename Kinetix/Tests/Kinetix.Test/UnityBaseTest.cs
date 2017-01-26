using Kinetix.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Transactions;

namespace Kinetix.Test
{
    /// <summary>
    /// Defines methods needed in tests.
    /// </summary>
    public abstract class UnityBaseTest
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() => {
            return new UnityContainer();
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        protected static TransactionScope _scope;
        //private TestSecurityContext _context;

        /// <summary>
        /// Test cleanup.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                //_context.Dispose();
            }
            catch
            {
                // RAS.
            }

            try
            {
                if (Transaction.Current != null)
                {
                    Transaction.Current.Rollback();
                }

                _scope.Dispose();
                _scope = null;
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Test initialize.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.Register();

            /* Initialise la configuration. */
            ConfigManager.Init();

            _scope = new TransactionScope();
        }


        public virtual void Register()
        {
            
        }
    }

}
