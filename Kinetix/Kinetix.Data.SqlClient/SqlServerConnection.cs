using System;
using System.Data;
using System.Data.Common;

namespace Kinetix.Data.SqlClient {
    /// <summary>
    /// Initialise une nouvelle connexion base de données.
    /// </summary>
    internal sealed class SqlServerConnection : IDbConnection {
        private readonly string _connectionName;

        /// <summary>
        /// Crée une nouvelle instance de connexion.
        /// Cette connexion utilise le pool de connexion.
        /// </summary>
        /// <param name="connectionFactory">Factory de connexions.</param>
        /// <param name="connectionString">Chaine de connexion.</param>
        /// <param name="connectionName">Nom logique de la connexion.</param>
        internal SqlServerConnection(DbProviderFactory connectionFactory, string connectionString, string connectionName) {
            _connectionName = connectionName;
            SqlConnection = connectionFactory.CreateConnection();
            SqlConnection.ConnectionString = connectionString;
        }

        /// <summary>
        /// Obtient ou définit la chaîne de connexion à la base de données.
        /// Voir System.Data.SqlClient.SqlConnection pour plus d'information
        /// sur la syntaxe.
        /// </summary>
        string IDbConnection.ConnectionString {
            get {
                return SqlConnection.ConnectionString;
            }

            set {
                SqlConnection.ConnectionString = value;
            }
        }

        /// <summary>
        /// Retourne le temps d'attente à l'ouverture d'une connexion en secondes.
        /// </summary>
        public int ConnectionTimeout {
            get {
                return SqlConnection.ConnectionTimeout;
            }
        }

        /// <summary>
        /// Retourne le nom de la source de données utilisée par la connexion.
        /// </summary>
        public string Database {
            get {
                return SqlConnection.Database;
            }
        }

        /// <summary>
        /// Retourne l'état de la connexion.
        /// </summary>
        public ConnectionState State {
            get {
                return SqlConnection.State;
            }
        }

        /// <summary>
        /// Retourne la connexion interne.
        /// </summary>
        internal IDbConnection SqlConnection {
            get;
            private set;
        }

        /// <summary>
        /// Retourne le nom logique de la source de données associée
        /// à la connexion.
        /// </summary>
        internal string ConnectionName {
            get {
                return _connectionName;
            }
        }

        /// <summary>
        /// Début une nouvelle transaction.
        /// Cette méthode n'est pas supportée, il faut utiliser
        /// System.Transaction.TransactionScope.
        /// </summary>
        /// <returns>Non supporté.</returns>
        IDbTransaction IDbConnection.BeginTransaction() {
            throw new NotSupportedException(SR.TransactionNotSupported);
        }

        /// <summary>
        /// Début une nouvelle transaction.
        /// Cette méthode n'est pas supportée, il faut utiliser
        /// System.Transaction.TransactionScope.
        /// </summary>
        /// <param name="il">Niveau d'isolation de la transactio.</param>
        /// <returns>Non supporté.</returns>
        IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il) {
            throw new NotSupportedException(SR.TransactionNotSupported);
        }

        /// <summary>
        /// Change la base de données courante pour une connexion.
        /// Cette méthode n'est pas supportée.
        /// </summary>
        /// <param name="databaseName">Nom de la nouvelle base de données.</param>
        void IDbConnection.ChangeDatabase(string databaseName) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Ferme la connexion.
        /// La connexion est libérée ou rendu au pool de connexion en fonction du
        /// paramétrage de la source de données.
        /// </summary>
        public void Close() {
            SqlConnection.Close();
        }

        /// <summary>
        /// Crée une nouvelle commande.
        /// </summary>
        /// <returns>Non supporté.</returns>
        IDbCommand IDbConnection.CreateCommand() {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Libère les resources non managées.
        /// </summary>
        public void Dispose() {
            SqlConnection.Dispose();
            SqlConnection = null;
        }

        /// <summary>
        /// Ouvre une connexion base de données.
        /// </summary>
        public void Open() {
            SqlConnection.Open();
        }
    }
}
