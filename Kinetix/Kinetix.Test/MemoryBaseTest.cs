using Kinetix.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Kinetix.Test
{
    public class MemoryBaseTest : UnityBaseTest
    {

        /// <summary>
        /// Test initialize.
        /// </summary>
        [TestInitialize]
        public override void TestInitialize()
        {
            this.Register();

            /* Initialise la configuration. */
            ConfigManager.Init();

            _scope = new TransactionScope();
        }

    }
}
