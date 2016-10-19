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

namespace Kinetix.Audit.Test
{
    [TestClass]
    public class AuditTest : MemoryBaseTest
    {

        //private IUnityContainer container;

        public override void Register()
        {
            var container = GetConfiguredContainer();

            container.RegisterType<Kinetix.Audit.IAuditManager, Kinetix.Audit.AuditManager>();
            container.RegisterType<Kinetix.Audit.IAuditTraceStorePlugin, Kinetix.Audit.MemoryAuditTraceStorePlugin>();
        }

        [TestMethod]
        public void TestAddAuditTrace()
        {
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
        public void TestFindAuditTrace()
        {
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
            IList<AuditTrace> auditTraceFetch1 = auditManager.FindTrace(atc1);

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

            IList<AuditTrace> auditTraceFetch2 = auditManager.FindTrace(auditTraceCriteria2);

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
            IList<AuditTrace> auditTraceFetch3 = auditManager.FindTrace(auditTraceCriteria3);

            Assert.AreEqual(2, auditTraceFetch3.Count);

            //Criteria Item

            AuditTraceCriteria auditTraceCriteria4 = new AuditTraceCriteriaBuilder().WithItem(2).Build();
            IList<AuditTrace> auditTraceFetch4 = auditManager.FindTrace(auditTraceCriteria4);

            Assert.AreEqual(1, auditTraceFetch4.Count);

            AuditTrace auditTraceFetched4 = auditTraceFetch4[0];

            Assert.AreEqual(auditTrace1.BusinessDate, auditTraceFetched4.BusinessDate);
            Assert.AreEqual(auditTrace1.Category, auditTraceFetched4.Category);
            Assert.AreEqual(auditTrace1.Context, auditTraceFetched4.Context);
            Assert.AreEqual(auditTrace1.ExecutionDate, auditTraceFetched4.ExecutionDate);
            Assert.AreEqual(auditTrace1.Item, auditTraceFetched4.Item);

            //Criteria User
            AuditTraceCriteria auditTraceCriteria5 = new AuditTraceCriteriaBuilder().WithUser("USER3").Build();
            IList<AuditTrace> auditTraceFetch5 = auditManager.FindTrace(auditTraceCriteria5);

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
