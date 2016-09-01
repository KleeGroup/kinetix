using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Audit
{
    public sealed class AuditManager : IAuditManager
    {
        private static AuditManager _instance;
        private readonly IAuditTraceStore _auditTraceStore;


        public AuditManager(IAuditTraceStore auditTraceStore)
        {
            _auditTraceStore = auditTraceStore;
        }

        public void AddTrace(AuditTrace auditTrace)
        {
            _auditTraceStore.CreateTrace(auditTrace);
        }

        public IList<AuditTrace> FindTrace(AuditTraceCriteria auditTraceCriteria)
        {
            return _auditTraceStore.FindTraceByCriteria(auditTraceCriteria);
        }

        public AuditTrace GetTrace(long idAuditTrace)
        {
            return _auditTraceStore.ReadTrace(idAuditTrace);
        }

        /// <summary>
        /// Retourne un singleton.
        /// </summary>
        public static AuditManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AuditManager(new MemoryAuditTraceStorePlugin());
                }

                return _instance;
            }
        }
    }
}
