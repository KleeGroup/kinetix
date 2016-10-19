using Kinetix.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
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

        public abstract void TestInitialize();


        public virtual void Register()
        {
            
        }
    }

}
