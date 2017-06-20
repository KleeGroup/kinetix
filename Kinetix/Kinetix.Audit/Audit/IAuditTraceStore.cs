using System.Collections.Generic;

namespace Kinetix.Audit {
    public interface IAuditTraceStore
    {

        /// <summary>
        /// Get an audit trail.
        /// </summary>
        /// <param name="idAuditTrace">the audit trail defined by its id.</param>
        /// <returns>the AuditTrace</returns>
        AuditTrace ReadTrace(int? idAuditTrace);

        /// <summary>
        /// Save a new audit trail.
        /// Attention: The audit MUST NOT have an id.
        /// </summary>
        /// <param name="auditTrace">AuditTrace to save.</param>
        void CreateTrace(AuditTrace auditTrace);

        /// <summary>
        /// Fetch all Audit Trace mathing the provided criteria.
        /// </summary>
        /// <param name="atc">AuditTraceCriteria.</param>
        /// <returns>the matching taces for the provided criteria</returns>
        ICollection<AuditTrace> FindTraceByCriteria(AuditTraceCriteria atc);

    }
}
