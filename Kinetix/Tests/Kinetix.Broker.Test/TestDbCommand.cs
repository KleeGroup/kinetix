using System;
using System.Data;
using Kinetix.ComponentModel;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Classe de test sur le IDbCommand permettant de simuler l'accès au data reader.
    /// </summary>
    public sealed class TestDbCommand : IReadCommand {

        private readonly IDataReader _reader;

        /// <summary>
        /// Crée la commande.
        /// </summary>
        /// <param name="reader">IDataReader.</param>
        /// <returns>IDbCommand contenant la commande.</returns>
        public TestDbCommand(IDataReader reader) {
            this._reader = reader;
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        public string CommandText {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        public int CommandTimeout {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        public CommandType CommandType {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        public IDbConnection Connection {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        public IDataParameterCollection Parameters {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        public IDbTransaction Transaction {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        public UpdateRowSource UpdatedRowSource {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        /// <returns>Non implémenté.</returns>
        public IDbDataParameter CreateParameter() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        /// <returns>Non implémenté.</returns>
        public int ExecuteNonQuery() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        /// <param name="behavior">Non utilisé.</param>
        /// <returns>Levée d'exception.</returns>
        public IDataReader ExecuteReader(CommandBehavior behavior) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne le data reader.
        /// </summary>
        /// <returns>IDataReader.</returns>
        public IDataReader ExecuteReader() {
            return _reader;
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        /// <returns>Non implémenté.</returns>
        public object ExecuteScalar() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        public void Prepare() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        public void Cancel() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Méthode non implémentée.
        /// </summary>
        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
