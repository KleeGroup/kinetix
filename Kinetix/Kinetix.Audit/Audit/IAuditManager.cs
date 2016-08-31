using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetix.Audit
{
    interface IAuditManager
    {
        /// <summary>
        /// Add an audit trail.
        /// </summary>
        void AddTrace(AuditTrace auditTrace);

        /// <summary>
        /// Search an audit trail.
        /// </summary>
        /// <param name="auditTraceCriteria">Criteria.</param>
        /// <returns>The matching audit traces.</returns>
        IList<AuditTrace> FindTrace(AuditTraceCriteria auditTraceCriteria);

        /// <summary>
        /// Get an audit trail.
        /// </summary>
        /// <returns>the audit trace for the desired id.</returns>
        AuditTrace GetTrace(long idAuditTrace);
    }
}
