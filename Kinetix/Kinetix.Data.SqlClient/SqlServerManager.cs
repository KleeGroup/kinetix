using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Transactions;
using System.Web;
using System.Windows.Forms;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Html;
using Kinetix.Monitoring.Manager;
using log4net;

namespace Kinetix.Data.SqlClient {

    /// <summary>
    /// Manager pour la gestion des appels base de données.
    /// </summary>
    public sealed class SqlServerManager : IManager, IManagerDescription {

        /// <summary>
        /// Nom du compteur de requetes SQL.
        /// </summary>
        public const string CounterSqlRequestCount = "SQL_REQUEST_COUNT";

        /// <summary>
        /// Nom du compteur d'erreurs base de données.
        /// </summary>
        public const string CounterSqlErrorCount = "SQL_ERROR_COUNT";

        /// <summary>
        /// Nom du compteur de dead-lock base de données.
        /// </summary>
        public const string CounterSqlDeadLockCount = "SQL_DEADLOCK_COUNT";

        /// <summary>
        /// Nom du compteur de timeout base de données.
        /// </summary>
        public const string CounterSqlTimeoutCount = "SQL_TIMEOUT_COUNT";

        /// <summary>
        /// Nom de la base de données de monitoring.
        /// </summary>
        public const string SqlHyperCube = "SQLSERVERDB";

        /// <summary>
        /// Nom du template ADMIN.
        /// </summary>
        public const string AdminTemplateName = "ADMIN";

        /// <summary>
        /// Nom du template COMMUN (REFERENTIEL).
        /// </summary>
        public const string CommonTemplateName = "REFERENTIEL";

        /// <summary>
        /// Nom du schema template.
        /// </summary>
        public const string SchemaTemplateName = "ETT_TEMPLATE";

        private const string SqlServerTransactionalContext = "SqlServerTransactionalContext";

        private static readonly SqlServerManager _instance = new SqlServerManager();

        [ThreadStatic]
        private static TransactionalContext _currentContext;

        private readonly Dictionary<string, ConnectionStringSettings> _connectionSettings = new Dictionary<string, ConnectionStringSettings>();
        private readonly Dictionary<string, DbProviderFactory> _factories = new Dictionary<string, DbProviderFactory>();
        private readonly List<ResourceManager> _constraintMessagesResources = new List<ResourceManager>();
        private readonly List<ResourceManager> _includeQueryResources = new List<ResourceManager>();

        /// <summary>
        /// Dictionnaire de données contenant en clef, le shortName.PropertyName des types portant des données constantes
        /// Et en valeur, la valeur de la constante statique.
        /// </summary>
        private readonly Dictionary<string, object> _constValues = new Dictionary<string, object>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        private SqlServerManager() {
            Analytics.Instance.CreateCounter("Requêtes SQL", CounterSqlRequestCount, 20, 30, 30);
            Analytics.Instance.CreateCounter("Erreur BDD", CounterSqlErrorCount, 0, 0, 31);
            Analytics.Instance.CreateCounter("Deadlock BDD", CounterSqlDeadLockCount, 0, 0, 32);
            Analytics.Instance.CreateCounter("Timeout BDD", CounterSqlTimeoutCount, 0, 0, 33);
            Analytics.Instance.OpenDataBase(SqlHyperCube, this);
        }

        /// <summary>
        /// Retourne une instance du manager.
        /// </summary>
        public static SqlServerManager Instance {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Nom du manager.
        /// </summary>
        string IManagerDescription.Name {
            get {
                return "Sql Server";
            }
        }

        /// <summary>
        /// Retourne un objet décrivant le service.
        /// </summary>
        IManagerDescription IManager.Description {
            get {
                return this;
            }
        }

        /// <summary>
        /// Image du manager.
        /// </summary>
        string IManagerDescription.Image {
            get {
                return "DB.png";
            }
        }

        /// <summary>
        /// Image.
        /// </summary>
        byte[] IManagerDescription.ImageData {
            get {
                return IR.DB_png;
            }
        }

        /// <summary>
        /// Priorité d'affichage du manager.
        /// </summary>
        int IManagerDescription.Priority {
            get {
                return 30;
            }
        }

        /// <summary>
        /// Type mime de l'image.
        /// </summary>
        string IManagerDescription.ImageMimeType {
            get {
                return "image/png";
            }
        }

        /// <summary>
        /// Attend le démarrage de la base de données ou l'évènement d'arrêt.
        /// </summary>
        /// <param name="connectionName">Nom de la connexion.</param>
        /// <param name="stopEvent">Evènement d'arrêt.</param>
        /// <param name="loop">True si il faut boucler jusqu'à la disponibilité de la base ou la levée de stopEvent.</param>
        /// <returns>True si la base est démarrée.</returns>
        public static bool WaitForDatabase(string connectionName, WaitHandle stopEvent, bool loop) {
            if (stopEvent == null) {
                throw new ArgumentNullException("stopEvent");
            }

            ConnectionStringSettings connectionSetting = ConfigurationManager.ConnectionStrings[connectionName];
            if (connectionSetting == null) {
                throw new SqlServerException("Connection setting not found !");
            }

            while (true) {
                try {
                    DbProviderFactory factory = DbProviderFactories.GetFactory(connectionSetting.ProviderName);
                    using (DbConnection connection = factory.CreateConnection()) {
                        connection.ConnectionString = connectionSetting.ConnectionString;
                        connection.Open();
                        RecordValidConnectionString(connectionSetting.ConnectionString);
                        return true;
                    }
                } catch (Exception e) {
                    ILog log = LogManager.GetLogger("Application");
                    if (log.IsDebugEnabled) {
                        log.Debug("Impossible d'ouvrir une connexion base de données", e);
                    } else if (log.IsWarnEnabled) {
                        log.Warn("Echec de la tentative de connexion base de données.");
                    }

                    if (loop) {
                        if (stopEvent.WaitOne(60000, false)) {
                            return false;
                        }
                    } else {
                        if (CheckConnectionStringValidity(connectionSetting.ConnectionString)) {
                            return false;
                        }

                        throw new SqlServerException("Impossible d'ouvrir une connexion base de données", e);
                    }
                }
            }
        }

        /// <summary>
        /// Libération des ressources consommées par le manager lors du undeploy.
        /// Exemples : connexions, thread, flux.
        /// </summary>
        void IManager.Close() {
        }

        /// <summary>
        /// Extension de la méthode toString().
        /// Permet à chaque Manager de présenter son propre état.
        /// </summary>
        /// <param name="writer">Writer HTML.</param>
        void IManagerDescription.ToHtml(System.Web.UI.HtmlTextWriter writer) {
            HtmlPageRenderer.ToHtml(SqlHyperCube, writer);
        }

        /// <summary>
        /// Enregistre les valeurs de constantes statiques des DTOs pour une assembly donnée.
        /// </summary>
        /// <param name="assembly">L'assembly à analyser.</param>
        public void RegisterConstDataTypes(Assembly assembly) {
            if (assembly == null) {
                throw new ArgumentNullException("assembly");
            }

            foreach (Type type in assembly.GetTypes()) {
                if (type.IsPublic && type.IsClass && type.Namespace != null && type.Namespace.IndexOf("DataContract", StringComparison.Ordinal) != -1) {
                    foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Static)) {
                        _constValues[type.Name + "." + field.Name] = field.GetRawConstantValue();
                    }
                }
            }
        }

        /// <summary>
        /// Retourne la valeur de la constante statique a partir de son nom court (ClassName.FieldName).
        /// </summary>
        /// <param name="shortName">Nom court.</param>
        /// <returns>Valeur de la constante statique.</returns>
        public object GetConstValueByShortName(string shortName) {
            if (string.IsNullOrEmpty(shortName)) {
                throw new ArgumentNullException("shortName");
            }

            return _constValues[shortName];
        }

        /// <summary>
        /// Enregistre la configuration d'une connexion base de données.
        /// </summary>
        /// <param name="connectionStringSettings">Configuration.</param>
        public void RegisterConnectionStringSettings(ConnectionStringSettings connectionStringSettings) {
            if (connectionStringSettings == null) {
                throw new ArgumentNullException("connectionStringSettings");
            }

            _connectionSettings[connectionStringSettings.Name] = connectionStringSettings;
        }

        /// <summary>
        /// Enregistre une nouvelle factory de connexion.
        /// </summary>
        /// <param name="factoryName">Nom de la factory.</param>
        /// <param name="factory">Factory.</param>
        public void RegisterProviderFactory(string factoryName, DbProviderFactory factory) {
            _factories[factoryName] = factory;
        }

        /// <summary>
        /// Manager de resources.
        /// </summary>
        /// <param name="manager">Manager.</param>
        public void RegisterConstraintMessageResource(ResourceManager manager) {
            _constraintMessagesResources.Add(manager);
        }

        /// <summary>
        /// Manager de resources d'include pour les queries.
        /// </summary>
        /// <param name="manager">Manager.</param>
        public void RegisterIncludeQueryResource(ResourceManager manager) {
            _includeQueryResources.Add(manager);
        }

        /// <summary>
        /// Retourne le context transactionnel courant.
        /// </summary>
        /// <returns>Context transactionnel.</returns>
        public TransactionalContext CurrentTransactionalContext() {
            if (HttpContext.Current == null) {
                return _currentContext;
            }

            if (HttpContext.Current.Items.Contains(SqlServerTransactionalContext)) {
                return (TransactionalContext)HttpContext.Current.Items[SqlServerTransactionalContext];
            }

            return null;
        }

        /// <summary>
        /// Supprime le contexte transactionnel.
        /// </summary>
        internal static void ClearTransactionnalContext() {
            if (HttpContext.Current == null) {
                _currentContext = null;
            } else {
                HttpContext.Current.Items.Remove(SqlServerTransactionalContext);
            }
        }

        /// <summary>
        /// Fournit une connexion base de données.
        /// Si aucune connexion n'est associée au context, une nouvelle connexion
        /// est ouverte.
        /// Une transaction doit être disponible.
        /// La connexion est enregistrée dans ce context et sera automatiquement libérée.
        /// </summary>
        /// <param name="connectionName">Nom de la source de données.</param>
        /// <param name="disableCheckTransCtx">Si <code>True</code> ne vérifie pas la présence d'un contexte transactionnel.</param>
        /// <param name="mustCloseConnection">Indique si la connexion doit être fermée par l'appelant.</param>
        /// <returns>Connexion.</returns>
        internal SqlServerConnection ObtainConnection(string connectionName, bool disableCheckTransCtx, out bool mustCloseConnection) {
            SqlServerConnection connection = null;
            TransactionalContext context = null;
            mustCloseConnection = false;
            if (!disableCheckTransCtx) {
                Transaction currentTransaction = Transaction.Current;
                if (currentTransaction == null) {
                    if (HttpContext.Current == null) {
                        mustCloseConnection = true;
                    } else {
                        throw new NotSupportedException("Pas de context transactionnel !");
                    }
                } else {
                    context = ObtainTransactionalContext();
                    connection = context.GetConnection(connectionName);
                }
            } else {
                Transaction currentTransaction = Transaction.Current;
                if (currentTransaction != null) {
                    context = ObtainTransactionalContext();
                    connection = context.GetConnection(connectionName);
                } else {
                    mustCloseConnection = true;
                }
            }

            if (connection == null) {
                ConnectionStringSettings connectionSetting;
                lock (_connectionSettings) {
                    if (!_connectionSettings.TryGetValue(connectionName, out connectionSetting)) {
                        connectionSetting = ConfigurationManager.ConnectionStrings[connectionName];
                        if (connectionSetting == null) {
                            throw new SqlServerException("Connection setting not found for '" + connectionName + "' !");
                        }

                        _connectionSettings.Add(connectionName, connectionSetting);
                    }
                }

                DbProviderFactory factory;
                lock (_factories) {
                    if (!_factories.TryGetValue(connectionSetting.ProviderName, out factory)) {
                        factory = DbProviderFactories.GetFactory(connectionSetting.ProviderName);
                        _factories.Add(connectionSetting.ProviderName, factory);
                    }
                }

                connection = new SqlServerConnection(factory, connectionSetting.ConnectionString, connectionName);
                if (context != null) {
                    context.RegisterConnection(connection);
                }

                connection.Open();
            }

            if (connection.State == ConnectionState.Closed) {
                connection.Open();
            }

            return connection;
        }

        /// <summary>
        /// Retourne le message d'erreur associée à une violation de contrainte.
        /// </summary>
        /// <param name="indexName">Nom de l'index.</param>
        /// <param name="violation">Type de violation.</param>
        /// <returns>Message d'erreur ou null.</returns>
        internal string GetConstraintMessage(string indexName, SqlServerConstraintViolation violation) {
            string resourceName = indexName;
            if (violation == SqlServerConstraintViolation.ForeignKey) {
                resourceName += "_missing";
            }

            foreach (ResourceManager manager in _constraintMessagesResources) {
                try {
                    string constraintMessage = manager.GetString(resourceName);
                    if (!string.IsNullOrEmpty(constraintMessage)) {
                        return constraintMessage;
                    }
                } catch (MissingManifestResourceException) {
                    continue;
                }
            }

            ILog log = LogManager.GetLogger("Kinetix.Application");
            if (log.IsWarnEnabled) {
                log.Warn("La ressource " + resourceName + " n'a pas été trouvée, utilisation du message par défault.");
            }

            switch (violation) {
                case SqlServerConstraintViolation.ForeignKey:
                    resourceName = "FK_MISSING_DEFAULT_MESSAGE";
                    break;
                case SqlServerConstraintViolation.ReferenceKey:
                    resourceName = "FK_DEFAULT_MESSAGE";
                    break;
                case SqlServerConstraintViolation.Unique:
                    resourceName = "UK_DEFAULT_MESSAGE";
                    break;
                case SqlServerConstraintViolation.Check:
                    resourceName = "CK_DEFAULT_MESSAGE";
                    break;
                default:
                    return null;
            }

            foreach (ResourceManager manager in _constraintMessagesResources) {
                try {
                    return manager.GetString(resourceName);
                } catch (MissingManifestResourceException) {
                    continue;
                }
            }

            return null;
        }

        /// <summary>
        /// Cherche une query dans les ressources enregistrée.
        /// </summary>
        /// <param name="resourceName">Nom de la ressource.</param>
        /// <returns>La query à inclure.</returns>
        internal string GetIncludeQuery(string resourceName) {
            foreach (ResourceManager manager in _includeQueryResources) {
                try {
                    string includeQuery = manager.GetString(resourceName);
                    if (!string.IsNullOrEmpty(includeQuery)) {
                        return includeQuery;
                    }
                } catch (MissingManifestResourceException) {
                    continue;
                }
            }

            ILog log = LogManager.GetLogger("Kinetix.Application");
            if (log.IsWarnEnabled) {
                log.Warn("La ressource " + resourceName + " n'a pas été trouvée pour l'inclusion de requètes.");
            }

            return null;
        }

        /// <summary>
        /// Fournit le context transactionnel courant.
        /// </summary>
        /// <returns>Context transactionnel.</returns>
        private static TransactionalContext ObtainTransactionalContext() {
            TransactionalContext context = null;
            if (HttpContext.Current == null) {
                return _currentContext ?? (_currentContext = new TransactionalContext());
            }

            if (HttpContext.Current.Items.Contains(SqlServerTransactionalContext)) {
                context = (TransactionalContext)HttpContext.Current.Items[SqlServerTransactionalContext];
            } else {
                context = new TransactionalContext();
                HttpContext.Current.Items[SqlServerTransactionalContext] = context;
            }

            return context;
        }

        /// <summary>
        /// Enregistre la chaine de connexion courante comme valide.
        /// </summary>
        /// <param name="connectionString">Chaine de connexion.</param>
        private static void RecordValidConnectionString(string connectionString) {
            string fileName = Application.ExecutablePath + ".key";
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fs)) {
                writer.Write(SHA1.Create().ComputeHash(new UnicodeEncoding().GetBytes(connectionString)));
            }
        }

        /// <summary>
        /// Vérifie si la chaine de connexion courante a été enregistrée comme valide.
        /// </summary>
        /// <param name="connectionString">Chaine de connexion.</param>
        /// <returns>True si valide.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Gestion globale des exceptions.")]
        private static bool CheckConnectionStringValidity(string connectionString) {
            string fileName = Application.ExecutablePath + ".key";
            byte[] fingerPrint = SHA1.Create().ComputeHash(new UnicodeEncoding().GetBytes(connectionString));
            try {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(fs)) {
                    byte[] last = reader.ReadBytes(fingerPrint.Length);
                    for (int i = 0; i < fingerPrint.Length; i++) {
                        if (last[i] != fingerPrint[i]) {
                            return false;
                        }
                    }

                    return true;
                }
            } catch {
                return false;
            }
        }
    }
}
