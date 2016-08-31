using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Kinetix.Data.SqlClient.Test {
    /// <summary>
    /// Connexion de test.
    /// </summary>
    public class TestDbConnection : DbConnection {
        private readonly IList _list;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="list">Liste des données à retourner.</param>
        public TestDbConnection(IList list) {
            _list = list;
        }

        /// <summary>
        /// Obtient ou définit la chaîne de connexion à la base de données.
        /// Voir System.Data.SqlClient.SqlConnection pour plus d'information
        /// sur la syntaxe.
        /// </summary>
        public override string ConnectionString {
            get;
            set;
        }

        /// <summary>
        /// Retourne le nom de la source de données utilisée par la connexion.
        /// </summary>
        public override string Database {
            get {
                return "Test";
            }
        }

        /// <summary>
        /// Retourne l'état de la connexion.
        /// </summary>
        public override ConnectionState State {
            get {
                return ConnectionState.Open;
            }
        }

        /// <summary>
        /// Retourne la version du serveur.
        /// </summary>
        public override string ServerVersion {
            get {
                return "test";
            }
        }

        /// <summary>
        /// Retourne la source de données.
        /// </summary>
        public override string DataSource {
            get {
                return "Test";
            }
        }

        /// <summary>
        /// Change la base de données courante pour une connexion.
        /// Cette méthode n'est pas supportée.
        /// </summary>
        /// <param name="databaseName">Nom de la nouvelle base de données.</param>
        public override void ChangeDatabase(string databaseName) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Ferme la connexion.
        /// La connexion est libérée ou rendu au pool de connexion en fonction du
        /// paramétrage de la source de données.
        /// </summary>
        public override void Close() {
            return;
        }

        /// <summary>
        /// Ouvre une connexion base de données.
        /// </summary>
        public override void Open() {
            return;
        }

        /// <summary>
        /// Crée une nouvelle commande.
        /// </summary>
        /// <returns>Non supporté.</returns>
        protected override DbCommand CreateDbCommand() {
            TestDbCommand command = new TestDbCommand(_list);
            command.Connection = this;
            return command;
        }

        /// <summary>
        /// Début une nouvelle transaction.
        /// Cette méthode n'est pas supportée, il faut utiliser 
        /// System.Transaction.TransactionScope.
        /// </summary>
        /// <param name="isolationLevel">Niveau d'isolation de la transaction.</param>
        /// <returns>Non supporté.</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) {
            throw new NotSupportedException();
        }
    }
}
