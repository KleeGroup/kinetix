using System;
using System.Transactions;
using Kinetix.Data.SqlClient;

namespace Kinetix.ServiceModel {
    /// <summary>
    /// Permet de créer un nouveau contexte transactionnel.
    /// </summary>
    public sealed class ServiceScope : IDisposable {

        private TransactionScope _scope;
        private bool _completed = false;

        /// <summary>
        /// Crée une nouvelle transaction.
        /// </summary>
        /// <param name="scopeOption">Option.</param>
        public ServiceScope(TransactionScopeOption scopeOption) {
            switch (scopeOption) {
                case TransactionScopeOption.Required:
                    if (Transaction.Current == null) {
                        _scope = new TransactionScope(scopeOption, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted, });
                    } else {
                        _scope = new TransactionScope(scopeOption);
                    }

                    break;
                case TransactionScopeOption.RequiresNew:
                    _scope = new TransactionScope(scopeOption, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted, });
                    break;
                case TransactionScopeOption.Suppress:
                    _scope = new TransactionScope(scopeOption);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Crée une nouvelle transaction.
        /// </summary>
        /// <param name="scopeOption">Option.</param>
        /// <param name="scopeTimeout">Timeout.</param>
        public ServiceScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout) {
            switch (scopeOption) {
                case TransactionScopeOption.Required:
                    if (Transaction.Current == null) {
                        _scope = new TransactionScope(scopeOption, new TransactionOptions() { Timeout = scopeTimeout, IsolationLevel = IsolationLevel.ReadCommitted, });
                    } else {
                        _scope = new TransactionScope(scopeOption, new TransactionOptions() { Timeout = scopeTimeout, IsolationLevel = IsolationLevel.Unspecified });
                    }

                    break;
                case TransactionScopeOption.RequiresNew:
                    _scope = new TransactionScope(scopeOption, new TransactionOptions() { Timeout = scopeTimeout, IsolationLevel = IsolationLevel.ReadCommitted, });
                    break;
                case TransactionScopeOption.Suppress:
                    _scope = new TransactionScope(scopeOption, new TransactionOptions() { Timeout = scopeTimeout });
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Crée une nouvelle transaction.
        /// </summary>
        /// <param name="scopeOption">Option.</param>
        /// <param name="scopeTimeout">Timeout.</param>
        /// <param name="level">Niveau d'isolation.</param>
        public ServiceScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout, IsolationLevel level) {
            TransactionOptions option = new TransactionOptions() {
                Timeout = scopeTimeout,
                IsolationLevel = level,
            };

            _scope = new TransactionScope(scopeOption, option);
        }

        /// <summary>
        /// Valide la transaction.
        /// </summary>
        public void Complete() {
            _scope.Complete();
            _completed = true;
        }

        /// <summary>
        /// Libère le contexte.
        /// </summary>
        public void Dispose() {
            if (!_completed) {
                // Libération des objets associés à la transaction dans le cas d'une annulation (connexions).
                TransactionalContext context = SqlServerManager.Instance.CurrentTransactionalContext();
                if (context != null) {
                    Transaction t = Transaction.Current;
                    if (t != null) {
                        context.ReleaseConnections(t);
                    }
                }
            }

            _scope.Dispose();
            _scope = null;
        }
    }
}
