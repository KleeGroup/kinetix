using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Web;
using Kinetix.ComponentModel;
using Kinetix.Monitoring.Counter;
using log4net;

namespace Kinetix.Data.SqlClient {
    /// <summary>
    /// Commande d'appel à SqlServer.
    /// </summary>
    public sealed class SqlServerCommand : IReadCommand {

        private const byte TimeOutErrorClass = 11;
        private const int TimeOutErrorCode1 = -2146232060;
        private const int TimeOutErrorCode2 = -2;

        private readonly bool _disableCheckTransCtx;
        private readonly string _commandName;
        private SqlServerConnection _connection;
        private bool _mustCloseConnection;
        private IDbCommand _innerCommand;
        private SqlServerParameterCollection _parameterColl;
        private string _parserKey;

        /// <summary>
        /// Crée une nouvelle commande à partir de la ressource contenue.
        /// </summary>
        /// <param name="connectionName">Nom de la connection base de données.</param>
        /// <param name="assembly">Assembly contenant la ressource.</param>
        /// <param name="resourcePath">Nom de la ressource.</param>
        /// <param name="disableCheckTransCtx">Si <code>True</code> alors ne vérifie pas la présence d'un contexte transactionnel.</param>
        public SqlServerCommand(string connectionName, Assembly assembly, string resourcePath, bool disableCheckTransCtx = false) {
            if (string.IsNullOrEmpty(connectionName)) {
                throw new ArgumentNullException("connectionName");
            }

            if (assembly == null) {
                throw new ArgumentNullException("assembly");
            }

            if (string.IsNullOrEmpty(resourcePath)) {
                throw new ArgumentNullException("resourcePath");
            }

            _commandName = resourcePath;
            _disableCheckTransCtx = disableCheckTransCtx;
            _parserKey = resourcePath;

            string commandText;
            using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourcePath))) {
                commandText = reader.ReadToEnd();
            }

            _connection = SqlServerManager.Instance.ObtainConnection(connectionName, disableCheckTransCtx, out _mustCloseConnection);
            if (string.IsNullOrEmpty(commandText)) {
                throw new NotSupportedException(SR.ResourceNotFound);
            }

            _innerCommand = _connection.SqlConnection.CreateCommand();
            _innerCommand.CommandType = CommandType.Text;
            _innerCommand.CommandText = commandText;

            if (HttpContext.Current == null) {
                _innerCommand.CommandTimeout = 0;
            }
        }

        /// <summary>
        /// Crée une nouvelle commande à partir d'une requête SQL contenu
        /// dans un gestionnaire de ressource.
        /// </summary>
        /// <param name="connectionName">Nom de la connexion base de données.</param>
        /// <param name="manager">Gestionnaire de ressource.</param>
        /// <param name="resourceName">Nom de la ressource.</param>
        /// <param name="disableCheckTransCtx">Si <code>True</code> alors ne vérifie pas la présence d'un contexte transactionnel.</param>
        /// <returns>Commande.</returns>
        [Obsolete("Stockage des requêtes SQL dans des fichiers SQL préférable.")]
        public SqlServerCommand(string connectionName, ResourceManager manager, string resourceName, bool disableCheckTransCtx = false) {
            if (connectionName == null) {
                throw new ArgumentNullException("connectionName");
            }

            if (manager == null) {
                throw new ArgumentNullException("manager");
            }

            if (resourceName == null) {
                throw new ArgumentNullException("resourceName");
            }

            _commandName = resourceName;
            _disableCheckTransCtx = disableCheckTransCtx;
            _connection = SqlServerManager.Instance.ObtainConnection(connectionName, disableCheckTransCtx, out _mustCloseConnection);
            string commandText = manager.GetString(resourceName, CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(commandText)) {
                throw new NotSupportedException(SR.ResourceNotFound);
            }

            _innerCommand = _connection.SqlConnection.CreateCommand();
            _innerCommand.CommandType = CommandType.Text;
            _innerCommand.CommandText = commandText;
            _parserKey = manager.BaseName + "." + resourceName;

            if (HttpContext.Current == null) {
                _innerCommand.CommandTimeout = 0;
            }
        }

        /// <summary>
        /// Crée une nouvelle commande pour une procédure stockée.
        /// </summary>
        /// <param name="connectionName">Nom de la connexion base de données.</param>
        /// <param name="procName">Nom de la procédure stockée.</param>
        /// <param name="disableCheckTransCtx">Si <code>True</code> alors ne vérifie pas la présence d'un contexte transactionnel.</param>
        public SqlServerCommand(string connectionName, string procName, bool disableCheckTransCtx = false) {
            if (connectionName == null) {
                throw new ArgumentNullException("connectionName");
            }

            if (procName == null) {
                throw new ArgumentNullException("procName");
            }

            _commandName = procName;
            _disableCheckTransCtx = disableCheckTransCtx;
            _connection = SqlServerManager.Instance.ObtainConnection(connectionName, disableCheckTransCtx, out _mustCloseConnection);
            _innerCommand = _connection.SqlConnection.CreateCommand();
            _innerCommand.CommandText = procName;
            _innerCommand.CommandType = CommandType.StoredProcedure;

            if (HttpContext.Current == null) {
                _innerCommand.CommandTimeout = 0;
            }
        }

        /// <summary>
        /// Crée une nouvelle commande à partir d'une requête.
        /// </summary>
        /// <param name="connectionName">Nom de la connexion base de données.</param>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="commandText">Requête SQL de la commande.</param>
        /// <param name="disableCheckTransCtx">Si <code>True</code> alors ne vérifie pas la présence d'un contexte transactionnel.</param>
        public SqlServerCommand(string connectionName, string commandName, string commandText, bool disableCheckTransCtx = false) {
            _commandName = commandName;
            _connection = SqlServerManager.Instance.ObtainConnection(connectionName, disableCheckTransCtx, out _mustCloseConnection);
            _innerCommand = _connection.SqlConnection.CreateCommand();
            _innerCommand.CommandText = commandText;

            if (HttpContext.Current == null) {
                _innerCommand.CommandTimeout = 0;
            }
        }

        /// <summary>
        /// Crée une nouvelle commande à partir d'une requête.
        /// </summary>
        /// <param name="connectionName">Nom de la connexion base de données.</param>
        /// <param name="commandName">Nom de la commande.</param>
        /// <param name="commandType">Type de la commande.</param>
        internal SqlServerCommand(string connectionName, string commandName, CommandType commandType) {
            _commandName = commandName;
            _connection = SqlServerManager.Instance.ObtainConnection(connectionName, false, out _mustCloseConnection);
            _innerCommand = _connection.SqlConnection.CreateCommand();
            _innerCommand.CommandType = commandType;
            _innerCommand.Connection = _connection.SqlConnection;

            if (HttpContext.Current == null) {
                _innerCommand.CommandTimeout = 0;
            }
        }

        /// <summary>
        /// Obtient ou définit le nom du manager pour logguer les messages.
        /// </summary>
        public string LogManagerName {
            get;
            set;
        }

        /// <summary>
        /// Obtient la commande SQL.
        /// </summary>
        public string CommandText {
            get {
                return _innerCommand.CommandText;
            }

            internal set {
                _innerCommand.CommandText = value;
            }
        }

        /// <summary>
        /// Obtient ou définit le temps d'attente maximum pour l'exécution
        /// d'une commande (par défaut 30s).
        /// </summary>
        public int CommandTimeout {
            get {
                return _innerCommand.CommandTimeout;
            }

            set {
                _innerCommand.CommandTimeout = value;
            }
        }

        /// <summary>
        /// Obtient ou définit le nombre maximum d'éléments remontés par la requête (injection dynamique dans le TOP).
        /// </summary>
        public int? MaxRows {
            get;
            set;
        }

        /// <summary>
        /// Retourne la base de données utilisée.
        /// </summary>
        public string InitialCatalog {
            get {
                SqlConnectionStringBuilder connection = new SqlConnectionStringBuilder(_connection.SqlConnection.ConnectionString);
                return connection.InitialCatalog;
            }
        }

        /// <summary>
        /// Obtient ou définit le type de commande.
        /// </summary>
        public CommandType CommandType {
            get {
                return _innerCommand.CommandType;
            }
        }

        /// <summary>
        /// Retourne la liste des paramétres de la commande.
        /// </summary>
        public SqlServerParameterCollection Parameters {
            get { return _parameterColl ?? (_parameterColl = new SqlServerParameterCollection(_innerCommand)); }
        }

        /// <summary>
        /// Obtient ou définit les paramètres de la requête (limit, offset, tri).
        /// </summary>
        public QueryParameter QueryParameters {
            get;
            set;
        }

        /// <summary>
        /// Retourne la liste des paramétres de la commande.
        /// </summary>
        IDataParameterCollection IReadCommand.Parameters {
            get {
                return this.Parameters;
            }
        }

        /// <summary>
        /// Annule la commande.
        /// </summary>
        public void Cancel() {
            _innerCommand.Cancel();
        }

        /// <summary>
        /// Crée un nouveau paramétre pour la commande.
        /// </summary>
        /// <returns>Paramètre.</returns>
        public SqlServerParameter CreateParameter() {
            return new SqlServerParameter(_innerCommand.CreateParameter());
        }

        /// <summary>
        /// Libère les ressources non managées.
        /// </summary>
        public void Dispose() {
            _innerCommand.Dispose();
            _innerCommand = null;
            if (_disableCheckTransCtx) {
                _connection.Close();
                _connection.Dispose();
            }

            _connection = null;
            _parameterColl = null;
        }

        /// <summary>
        /// Exécute la commande de mise à jour de données.
        /// </summary>
        /// <param name="minRowsAffected">Nombre minimum de lignes affectées.</param>
        /// <param name="maxRowsAffected">Nombre maximum de lignes affectées.</param>
        /// <returns>Nombre de ligne impactées.</returns>
        public int ExecuteNonQuery(int minRowsAffected, int maxRowsAffected) {
            int rowsAffected = this.ExecuteNonQuery();
            if (rowsAffected < minRowsAffected) {
                throw rowsAffected == 0
                          ? new SqlServerException(SR.ExceptionZeroRowAffected)
                          : new SqlServerException(
                              string.Format(
                                  CultureInfo.CurrentCulture,
                                  SR.ExceptionTooFewRowsAffected,
                                  rowsAffected));
            }

            if (rowsAffected > maxRowsAffected) {
                throw new SqlServerException(string.Format(
                    CultureInfo.CurrentCulture,
                    SR.ExceptionTooManyRowsAffected,
                    rowsAffected));
            }

            if (_mustCloseConnection) {
                _connection.Close();
            }

            return rowsAffected;
        }

        /// <summary>
        /// Exécute la commande de mise à jour de données.
        /// </summary>
        /// <returns>Nombre de ligne impactées.</returns>
        public int ExecuteNonQuery() {
            SqlCommandListener listener = new SqlCommandListener(this);
            try {
                CommandParser.ParseCommand(_innerCommand, _parserKey, null);
                return _innerCommand.ExecuteNonQuery();
            } catch (DbException sqle) {
                throw listener.HandleException(sqle);
            } finally {
                if (_mustCloseConnection) {
                    _connection.Close();
                }

                listener.Dispose();
            }
        }

        /// <summary>
        /// Exécute une commande de selection et retourne un dataReader.
        /// </summary>
        /// <returns>DataReader.</returns>
        public SqlServerDataReader ExecuteReader() {
            SqlCommandListener listener = new SqlCommandListener(this);
            try {
                CommandParser.ParseCommand(_innerCommand, _parserKey, this.MaxRows, this.QueryParameters);
                return new SqlServerDataReader(_innerCommand.ExecuteReader(), _mustCloseConnection ? _connection : null, this.QueryParameters);
            } catch (DbException sqle) {
                throw listener.HandleException(sqle);
            } finally {
                listener.Dispose();
            }
        }

        /// <summary>
        /// Exécute une commande de selection et retourne un dataReader.
        /// </summary>
        /// <returns>DataReader.</returns>
        IDataReader IReadCommand.ExecuteReader() {
            return this.ExecuteReader();
        }

        /// <summary>
        /// Exécute une requête de select et retourne la première valeur
        /// de la première ligne.
        /// </summary>
        /// <returns>Retourne la valeur ou null.</returns>
        public object ExecuteScalar() {
            SqlCommandListener listener = new SqlCommandListener(this);
            try {
                CommandParser.ParseCommand(_innerCommand, _parserKey, null);
                object value = _innerCommand.ExecuteScalar();
                return (value == DBNull.Value) ? null : value;
            } catch (DbException sqle) {
                throw listener.HandleException(sqle);
            } finally {
                if (_mustCloseConnection) {
                    _connection.Close();
                }

                listener.Dispose();
            }
        }

        /// <summary>
        /// Exécute une requête de select et retour la première valeur
        /// de la première ligne.
        /// </summary>
        /// <param name="minRowsAffected">Nombre minimum de lignes affectées.</param>
        /// <param name="maxRowsAffected">Nombre maximum de lignes affectées.</param>
        /// <returns>Retourne la valeur ou null.</returns>
        public object ExecuteScalar(int minRowsAffected, int maxRowsAffected) {
            using (SqlServerDataReader reader = this.ExecuteReader()) {
                if (reader.Read()) {
                    int rowsAffected = reader.RecordsAffected;
                    if (rowsAffected > maxRowsAffected) {
                        throw new SqlServerException(string.Format(
                            CultureInfo.CurrentCulture,
                            SR.ExceptionTooManyRowsAffected,
                            rowsAffected));
                    }

                    if (rowsAffected < minRowsAffected) {
                        throw new SqlServerException(string.Format(
                            CultureInfo.CurrentCulture,
                            SR.ExceptionTooFewRowsAffected,
                            rowsAffected));
                    }

                    return reader.GetValue(0);
                }

                throw new SqlServerException(SR.ExceptionZeroRowAffected);
            }
        }

        /// <summary>
        /// Classe permettant le suivi de l'éxécution des commandes.
        /// </summary>
        private sealed class SqlCommandListener : IDisposable {
            private static string checkConstraintPattern = "CK_[A-Z_]*";
            private static string foreignKeyConstraintPattern = "FK_[A-Z_]*";
            private static string uniqueKeyConstraintPattern = "UK_[A-Z_]*";
            private readonly IDisposable _logDisposable;
            private readonly SqlServerCommand _command;

            /// <summary>
            /// Crée une nouvelle instance.
            /// </summary>
            /// <param name="command">Commande.</param>
            public SqlCommandListener(SqlServerCommand command) {
                _command = command;
                _logDisposable = ThreadContext.Stacks["NDC"].Push(_command._commandName);
                Analytics.Instance.IncValue(SqlServerManager.CounterSqlRequestCount, 1);
                Analytics.Instance.StartProcess(_command._commandName);
            }

            /// <summary>
            /// Prend en charge une exception Sql Server.
            /// </summary>
            /// <param name="exception">Exception.</param>
            /// <returns>Exception.</returns>
            public Exception HandleException(DbException exception) {
                Analytics.Instance.IncValue(SqlServerManager.CounterSqlErrorCount, 1);

                SqlException sqlException = exception as SqlException;
                if (sqlException == null) {
                    return new SqlServerException(exception.Message, exception);
                }

                SqlErrorMessage message = null;
                ILog log = LogManager.GetLogger(string.IsNullOrEmpty(_command.LogManagerName) ? "Sql" : _command.LogManagerName);
                foreach (SqlError error in sqlException.Errors) {
                    if (log.IsErrorEnabled) {
                        log.ErrorFormat(
                            CultureInfo.CurrentCulture,
                            "Error class:{0} message:{1} line:{2} number:{3} proc:{4} server:{5} source:{6} state:{7}",
                            error.Class,
                            error.Message,
                            error.LineNumber,
                            error.Number,
                            error.Procedure,
                            error.Server,
                            error.Source,
                            error.State);
                    }

                    if (error.Number == 1205) {
                        // Erreur de deadlock.
                        Analytics.Instance.IncValue(SqlServerManager.CounterSqlDeadLockCount, 1);
                    } else if (error.Class == TimeOutErrorClass && (error.Number == TimeOutErrorCode1 || error.Number == TimeOutErrorCode2)) {
                        // Erreur de timeout.
                        Analytics.Instance.IncValue(SqlServerManager.CounterSqlTimeoutCount, 1);
                        return new SqlServerTimeoutException(exception.Message, exception);
                    } else if (error.Class == 16 && error.Number == 547) {
                        // Erreur de contrainte.
                        message = HandleConstraintException(error.Message);
                    } else if (error.Class == 14 && error.Number == 2601) {
                        // Erreur de contrainte.
                        message = HandleUniqueConstraintException(error.Message);
                    } else if (error.Class == 14 && error.Number == 2627) {
                        // Erreur de contrainte.
                        message = HandleUniqueConstraintException(error.Message);
                    }
                }

                if (message != null) {
                    return new ComponentModel.ConstraintException(message.Message, message.Code, exception);
                }

                return new SqlServerException(exception.Message, exception);
            }

            /// <summary>
            /// Libère les ressources de la commande.
            /// </summary>
            public void Dispose() {
                long duration = Analytics.Instance.StopProcess(SqlServerManager.SqlHyperCube);
                ILog log = LogManager.GetLogger(string.IsNullOrEmpty(_command.LogManagerName) ? "Kinetix.Sql" : _command.LogManagerName);
                if (log.IsInfoEnabled) {
                    log.Info("Commande sql : " + _command._commandName + " " + duration);
                }

                if (log.IsDebugEnabled) {
                    log.Debug(_command._innerCommand.CommandText);
                    foreach (SqlServerParameter parameter in _command.Parameters) {
                        byte[] dataArray = parameter.Value as byte[];
                        if (dataArray != null) {
                            log.Debug(parameter.ParameterName + " : byte[" + dataArray.Length + "]");
                        } else {
                            log.Debug(parameter.ParameterName + " : " + Convert.ToString(parameter.Value, CultureInfo.InvariantCulture));
                        }
                    }
                }

                _logDisposable.Dispose();

                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Convertit une erreur de contrainte en message.
            /// </summary>
            /// <param name="initialMessage">Message initial.</param>
            /// <returns>Message final.</returns>
            /// <remarks>
            /// Prend en charge les messages 547 et 2601 dans les langues 1033 et 1036.
            /// </remarks>
            private static SqlErrorMessage HandleConstraintException(string initialMessage) {
                Match match = new Regex(foreignKeyConstraintPattern).Match(initialMessage);
                if (match.Success) {
                    return HandleForeignConstraintException(initialMessage, match.Value);
                }

                match = new Regex(checkConstraintPattern).Match(initialMessage);
                if (match.Success) {
                    return HandleCheckConstraintException(match.Value);
                }

                return null;
            }

            /// <summary>
            /// Convertit une erreur de contrainte de clé étrangère en message.
            /// </summary>
            /// <param name="initialMessage">Message initial.</param>
            /// <param name="index">Nom de l'index.</param>
            /// <returns>Message final.</returns>
            private static SqlErrorMessage HandleForeignConstraintException(string initialMessage, string index) {
                SqlServerConstraintViolation violation =
                            initialMessage.Contains("FOREIGN KEY") ?
                            SqlServerConstraintViolation.ForeignKey :
                            SqlServerConstraintViolation.ReferenceKey;

                string message = SqlServerManager.Instance.GetConstraintMessage(index, violation);
                if (string.IsNullOrEmpty(message)) {
                    message = SqlServerManager.Instance.GetConstraintMessage("FK_DEFAULT_MESSAGE", violation);
                }

                return new SqlErrorMessage(message, index);
            }

            /// <summary>
            /// Convertit une erreur de contrainte CHECK en message.
            /// </summary>
            /// <param name="constraintName">Nom de la contrainte.</param>
            /// <returns>Message final.</returns>
            private static SqlErrorMessage HandleCheckConstraintException(string constraintName) {
                return new SqlErrorMessage(SqlServerManager.Instance.GetConstraintMessage(constraintName, SqlServerConstraintViolation.Check), constraintName);
            }

            /// <summary>
            /// Convertit une erreur de contrainte en message.
            /// </summary>
            /// <param name="initialMessage">Message initial.</param>
            /// <returns>Message final.</returns>
            private static SqlErrorMessage HandleUniqueConstraintException(string initialMessage) {
                Match match = new Regex(uniqueKeyConstraintPattern).Match(initialMessage);
                if (!match.Success) {
                    return null;
                }

                string index = match.Value;
                return new SqlErrorMessage(SqlServerManager.Instance.GetConstraintMessage(index, SqlServerConstraintViolation.Unique), index);
            }
        }

        /// <summary>
        /// Classe formalisant la remontée d'une erreur SQL une fois parsée.
        /// </summary>
        private sealed class SqlErrorMessage {

            /// <summary>
            /// Constructeur.
            /// </summary>
            /// <param name="message">Message d'erreur.</param>
            /// <param name="code">Code de l'erreur.</param>
            public SqlErrorMessage(string message, string code) {
                this.Message = message;
                this.Code = code;
            }

            /// <summary>
            /// Obtient le message d'erreur.
            /// </summary>
            public string Message {
                get;
                private set;
            }

            /// <summary>
            /// Obtient le code d'erreur.
            /// </summary>
            public string Code {
                get;
                private set;
            }
        }
    }
}
