using System.Collections;
using System.Data;
using System.Data.Common;

namespace Kinetix.Data.SqlClient.Test {
    /// <summary>
    /// Commande de test.
    /// </summary>
    public class TestDbCommand : DbCommand {

        private readonly TestDbParameterCollection _parameters = new TestDbParameterCollection();
        private readonly IList _list;

        private static TestDbCommand _lastCommand;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="list">Liste des données à retourner.</param>
        public TestDbCommand(IList list) {
            _list = list;
            _lastCommand = this;
        }

        /// <summary>
        /// Retourne la dernière commande.
        /// </summary>
        public static TestDbCommand LastCommand {
            get {
                return _lastCommand;
            }
        }

        /// <summary>
        /// Obtient la commande SQL.
        /// </summary>
        public override string CommandText {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le temps d'attente maximum pour l'exécution
        /// d'une commande.
        /// </summary>
        public override int CommandTimeout {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le type de commande.
        /// </summary>
        public override CommandType CommandType {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le mode de mise à jour des datarows.
        /// </summary>
        public override UpdateRowSource UpdatedRowSource {
            get;
            set;
        }

        /// <summary>
        /// Indique si le contrôle est visible à la conception.
        /// </summary>
        public override bool DesignTimeVisible {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la connexion de la commande.
        /// </summary>
        protected override DbConnection DbConnection {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la transaction de la commande.
        /// </summary>
        protected override DbTransaction DbTransaction {
            get;
            set;
        }

        /// <summary>
        /// Collection des paramètres.
        /// </summary>
        protected override DbParameterCollection DbParameterCollection {
            get {
                return _parameters;
            }
        }

        /// <summary>
        /// Prépare l'exécution de la commande.
        /// </summary>
        public override void Prepare() {
            return;
        }

        /// <summary>
        /// Exécute la commande de mise à jour de données.
        /// </summary>
        /// <returns>Nombre de ligne impactées.</returns>
        public override int ExecuteNonQuery() {
            if (this.CommandTimeout == -1) {
                throw new TestDbException();
            }
            return (_list == null) ? 1 : _list.Count;
        }

        /// <summary>
        /// Exécute une requête de select et retour la première valeur
        /// de la première ligne.
        /// </summary>
        /// <returns>Retourne la valeur ou null.</returns>
        public override object ExecuteScalar() {
            if (this.CommandTimeout == -1) {
                throw new TestDbException();
            }
            return 1;
        }

        /// <summary>
        /// Annule la commande.
        /// </summary>
        public override void Cancel() {
            return;
        }

        /// <summary>
        /// Crée un nouveau paramétre pour la commande.
        /// </summary>
        /// <returns>Paramètre.</returns>
        protected override DbParameter CreateDbParameter() {
            return new TestDbParameter();
        }

        /// <summary>
        /// Exécute une commande de selection et retourne un dataReader.
        /// </summary>
        /// <param name="behavior">Mode de traitement.</param>
        /// <returns>DataReader.</returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) {
            if (this.CommandTimeout == -1) {
                throw new TestDbException();
            }
            return new TestDbDataReader(_list);
        }
    }
}