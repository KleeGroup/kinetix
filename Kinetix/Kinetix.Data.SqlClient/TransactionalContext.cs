using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Transactions;

namespace Kinetix.Data.SqlClient {
    /// <summary>
    /// Context transactionnel pour un processus.
    /// Retient la liste des transactions ouvertes et les connexions associées.
    /// Les connexions sont automatiquement libérées lors du commit de la transaction.
    /// </summary>
    public class TransactionalContext {
        private Dictionary<Transaction, Dictionary<string, SqlServerConnection>> _transactionMap = new Dictionary<Transaction, Dictionary<string, SqlServerConnection>>();

        /// <summary>
        /// Libère les connexions associées à une transaction.
        /// </summary>
        /// <param name="tran">Transaction.</param>
        public void ReleaseConnections(Transaction tran) {
            Exception exception = null;
            try {
                lock (this) {
                    Dictionary<string, SqlServerConnection> connectionMap;
                    if (_transactionMap.TryGetValue(tran, out connectionMap)) {
                        _transactionMap.Remove(tran);
                        foreach (SqlServerConnection connection in connectionMap.Values) {
                            try {
                                connection.Dispose();
                            } catch (DbException ex) {
                                exception = new SqlServerException(ex.Message, ex);
                            }
                        }

                        connectionMap.Clear();
                    }
                }
            } finally {
                if (_transactionMap.Count == 0) {
                    SqlServerManager.ClearTransactionnalContext();
                }
            }

            if (exception != null) {
                throw exception;
            }
        }

        /// <summary>
        /// Enregistre une connexion. Un contexte transactionnel doit être disponible.
        /// </summary>
        /// <param name="connection">Connexion.</param>
        internal void RegisterConnection(SqlServerConnection connection) {
            Transaction currentTransaction = Transaction.Current;
            if (currentTransaction == null) {
                throw new NotSupportedException();
            }

            lock (this) {
                Dictionary<string, SqlServerConnection> connectionMap;
                if (!_transactionMap.TryGetValue(currentTransaction, out connectionMap)) {
                    connectionMap = new Dictionary<string, SqlServerConnection>();
                    _transactionMap[currentTransaction] = connectionMap;
                    currentTransaction.TransactionCompleted += new TransactionCompletedEventHandler(CurrentTransaction_TransactionCompleted);
                }

                connectionMap.Add(connection.ConnectionName, connection);
            }
        }

        /// <summary>
        /// Retourne une connexion à partir de son nom.
        /// </summary>
        /// <param name="connectionName">Nom de la source de données.</param>
        /// <returns>Connexion.</returns>
        internal SqlServerConnection GetConnection(string connectionName) {
            Transaction currentTransaction = Transaction.Current;
            if (currentTransaction == null) {
                throw new NotSupportedException();
            }

            Dictionary<string, SqlServerConnection> connectionMap;
            if (!_transactionMap.TryGetValue(currentTransaction, out connectionMap)) {
                return null;
            }

            SqlServerConnection connection;
            connectionMap.TryGetValue(connectionName, out connection);
            return connection;
        }

        /// <summary>
        /// Libère les connexions en fin de transaction.
        /// </summary>
        /// <param name="sender">Source de l'évènement.</param>
        /// <param name="e">Evènements.</param>
        private void CurrentTransaction_TransactionCompleted(object sender, TransactionEventArgs e) {
            ReleaseConnections(e.Transaction);
        }
    }
}
