using System;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using Kinetix.Audit.Audit;
using System.Collections.Generic;
using Kinetix.Test;
using Microsoft.Practices.Unity;
using Kinetix.Data.SqlClient;
using Kinetix.Broker;
using Kinetix.ComponentModel;
using System.Configuration;

namespace Kinetix.Audit.Test {
    [TestClass]
    public class AuditTest : UnityBaseTest {

        private readonly string DefaultDataSource = "default";

        // TODO : Change hard coded data-source
        private const string SERVER_NAME = "carla";
        private const string DATABASE_NAME = "DianeTfs";
        private const string USER_NAME = "dianeConnection";
        private const string USER_PWD = "Puorgeelk23";

        private readonly bool SqlServer = true;

        //private IUnityContainer container;

        public override void Register() {
            
            if (SqlServer) {
                // Connection string.
                ConnectionStringSettings conn = new ConnectionStringSettings {
                    Name = DefaultDataSource,
                    ConnectionString = $"Data Source={SERVER_NAME};Initial Catalog={DATABASE_NAME};User ID={USER_NAME};Password={USER_PWD}",
                    ProviderName = "System.Data.SqlClient"
                };

                DomainManager.Instance.RegisterDomainMetadataType(typeof(AuditDomainMetadata));
                SqlServerManager.Instance.RegisterConnectionStringSettings(conn);
                BrokerManager.RegisterDefaultDataSource(DefaultDataSource);
                BrokerManager.Instance.RegisterStore(DefaultDataSource, typeof(SqlServerStore<>));
            }

            var container = GetConfiguredContainer();
            container.RegisterType<Kinetix.Audit.IAuditManager, Kinetix.Audit.AuditManager>();

            if (SqlServer) {
                container.RegisterType<Kinetix.Audit.IAuditTraceStorePlugin, Kinetix.Audit.SqlServerAuditTraceStorePlugin>();
            } else {
                container.RegisterType<Kinetix.Audit.IAuditTraceStorePlugin, Kinetix.Audit.MemoryAuditTraceStorePlugin>();
            }
        }

        [TestMethod]
        public void TestAddAuditTrace() {
            var container = GetConfiguredContainer();
            IAuditManager auditManager = container.Resolve<IAuditManager>();

            AuditTrace auditTrace = new AuditTraceBuilder("CAT1", "USER1", 1, "My message 1").Build();

            auditManager.AddTrace(auditTrace);
            AuditTrace auditFetch = auditManager.GetTrace((int)auditTrace.Id);

            Assert.AreEqual(auditTrace.BusinessDate, auditFetch.BusinessDate);
            Assert.AreEqual(auditTrace.Category, auditFetch.Category);
            Assert.AreEqual(auditTrace.Context, auditFetch.Context);
            Assert.AreEqual(auditTrace.ExecutionDate, auditFetch.ExecutionDate);
            Assert.AreEqual(auditTrace.Item, auditFetch.Item);
        }

        [TestMethod]
        public void TestFindAuditTrace() {
            var container = GetConfiguredContainer();
            IAuditManager auditManager = container.Resolve<IAuditManager>();

            AuditTrace auditTrace1 = new AuditTraceBuilder("CAT2", "USER2", 2, "My message 2").Build();
            auditManager.AddTrace(auditTrace1);

            AuditTrace auditTrace2 = new AuditTraceBuilder("CAT3", "USER3", 3, "My message 3")
                    .WithDateBusiness(DateTime.Now)
                    .WithContext(new[] { "Context 3" })
                    .Build();

            auditManager.AddTrace(auditTrace2);

            //Criteria Category
            AuditTraceCriteria atc1 = new AuditTraceCriteriaBuilder().WithCategory("CAT2").Build();
            IList<AuditTrace> auditTraceFetch1 = new List<AuditTrace>(auditManager.FindTrace(atc1));

            Assert.AreEqual(1, auditTraceFetch1.Count);

            AuditTrace auditTraceFetched = auditTraceFetch1[0];
            Assert.AreEqual(auditTrace1.BusinessDate, auditTraceFetched.BusinessDate);
            Assert.AreEqual(auditTrace1.Category, auditTraceFetched.Category);
            Assert.AreEqual(auditTrace1.Context, auditTraceFetched.Context);
            Assert.AreEqual(auditTrace1.ExecutionDate, auditTraceFetched.ExecutionDate);
            Assert.AreEqual(auditTrace1.Item, auditTraceFetched.Item);

            DateTime dateJMinus1 = DateTime.Now.AddDays(-1);
            DateTime dateJPlus1 = DateTime.Now.AddDays(1);

            //Criteria Business Date
            AuditTraceCriteria auditTraceCriteria2 = new AuditTraceCriteriaBuilder()
                    .WithDateBusinessStart(dateJMinus1)
                    .WithDateBusinessEnd(dateJPlus1)
                    .Build();

            IList<AuditTrace> auditTraceFetch2 = new List<AuditTrace>(auditManager.FindTrace(auditTraceCriteria2));

            Assert.AreEqual(1, auditTraceFetch2.Count);

            AuditTrace auditTraceFetched2 = auditTraceFetch2[0];
            Assert.AreEqual(auditTrace2.BusinessDate, auditTraceFetched2.BusinessDate);
            Assert.AreEqual(auditTrace2.Category, auditTraceFetched2.Category);
            Assert.AreEqual(auditTrace2.Context, auditTraceFetched2.Context);
            Assert.AreEqual(auditTrace2.ExecutionDate, auditTraceFetched2.ExecutionDate);
            Assert.AreEqual(auditTrace2.Item, auditTraceFetched2.Item);


            //Criteria Exec Date
            AuditTraceCriteria auditTraceCriteria3 = new AuditTraceCriteriaBuilder()
                    .WithDateExecutionStart(dateJMinus1)
                    .WithDateExecutionEnd(dateJPlus1)
                    .Build();
            IList<AuditTrace> auditTraceFetch3 = new List<AuditTrace>(auditManager.FindTrace(auditTraceCriteria3));

            Assert.AreEqual(2, auditTraceFetch3.Count);

            //Criteria Item

            AuditTraceCriteria auditTraceCriteria4 = new AuditTraceCriteriaBuilder().WithItem(2).Build();
            IList<AuditTrace> auditTraceFetch4 = new List<AuditTrace>(auditManager.FindTrace(auditTraceCriteria4));

            Assert.AreEqual(1, auditTraceFetch4.Count);

            AuditTrace auditTraceFetched4 = auditTraceFetch4[0];

            Assert.AreEqual(auditTrace1.BusinessDate, auditTraceFetched4.BusinessDate);
            Assert.AreEqual(auditTrace1.Category, auditTraceFetched4.Category);
            Assert.AreEqual(auditTrace1.Context, auditTraceFetched4.Context);
            Assert.AreEqual(auditTrace1.ExecutionDate, auditTraceFetched4.ExecutionDate);
            Assert.AreEqual(auditTrace1.Item, auditTraceFetched4.Item);

            //Criteria User
            AuditTraceCriteria auditTraceCriteria5 = new AuditTraceCriteriaBuilder().WithUser("USER3").Build();
            IList<AuditTrace> auditTraceFetch5 = new List<AuditTrace>(auditManager.FindTrace(auditTraceCriteria5));

            Assert.AreEqual(1, auditTraceFetch5.Count);

            AuditTrace auditTraceFetched5 = auditTraceFetch5[0];

            Assert.AreEqual(auditTrace2.BusinessDate, auditTraceFetched5.BusinessDate);
            Assert.AreEqual(auditTrace2.Category, auditTraceFetched5.Category);
            Assert.AreEqual(auditTrace2.Context, auditTraceFetched5.Context);
            Assert.AreEqual(auditTrace2.ExecutionDate, auditTraceFetched5.ExecutionDate);
            Assert.AreEqual(auditTrace2.Item, auditTraceFetched5.Item);

        }
    }
}
