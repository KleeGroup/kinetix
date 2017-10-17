using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kinetix.Broker;
using Kinetix.Data.SqlClient;

namespace Kinetix.Audit {

    /// <summary>
    /// Plugin SqlServer pour la création/récupération d'audit.
    /// </summary>
    public class SqlServerAuditTraceStorePlugin : IAuditTraceStorePlugin {

        /// <inheritDoc cref="IAuditTraceStore.ReadTrace" />
        public AuditTrace ReadTrace(int? idAuditTrace) {
            return BrokerManager.GetBroker<AuditTrace>().Get(idAuditTrace);
        }

        /// <inheritDoc cref="IAuditTraceStore.CreateTrace" />
        public void CreateTrace(AuditTrace auditTrace) {
            Debug.Assert(auditTrace != null);
            Debug.Assert(auditTrace.Id == null, "A new audit trail must not have an id");
            //---
            auditTrace.Id = (int)BrokerManager.GetBroker<AuditTrace>().Save(auditTrace);
        }

        /// <inheritDoc cref="IAuditTraceStore.FindTraceByCriteria" />
        public ICollection<AuditTrace> FindTraceByCriteria(AuditTraceCriteria auditTraceCriteria) {
            var cmd = GetSqlServerCommand("FindTraceByCriteria.sql");
            cmd.Parameters.AddWithValue("AUD_BUSINESS_DATE_START", auditTraceCriteria.StartBusinessDate);
            cmd.Parameters.AddWithValue("AUD_BUSINESS_DATE_END", auditTraceCriteria.EndBusinessDate);
            cmd.Parameters.AddWithValue("AUD_EXECUTION_DATE_START", auditTraceCriteria.StartExecutionDate);
            cmd.Parameters.AddWithValue("AUD_EXECUTION_DATE_END", auditTraceCriteria.EndExecutionDate);
            cmd.Parameters.AddWithValue(AuditTrace.Cols.AUD_CATEGORY, auditTraceCriteria.Category);
            cmd.Parameters.AddWithValue(AuditTrace.Cols.AUD_ITEM, auditTraceCriteria.Item);
            cmd.Parameters.AddWithValue(AuditTrace.Cols.AUD_USERNAME, auditTraceCriteria.Username);
            return cmd.ReadList<AuditTrace>();
        }

        /// <summary>
        /// Retourne la commande SQL Server associée au script.
        /// </summary>
        /// <param name="script">Nom du script.</param>
        /// <param name="dataSource">Datasource.</param>
        /// <param name="disableCheckTransCtx">Indique si la vérification de la présence d'un contexte transactionnel doit être désactivée.</param>
        /// <returns>Commande.</returns>
        private SqlServerCommand GetSqlServerCommand(string script, string dataSource = "default", bool disableCheckTransCtx = false) {
            if (string.IsNullOrEmpty(script)) {
                throw new ArgumentNullException("script");
            }
            if (string.IsNullOrEmpty(dataSource)) {
                throw new ArgumentNullException("dataSource");
            }
            return new SqlServerCommand(dataSource, GetType().Assembly, string.Concat(GetType().Namespace + ".SQLResources.", script), false);
        }
    }
}
