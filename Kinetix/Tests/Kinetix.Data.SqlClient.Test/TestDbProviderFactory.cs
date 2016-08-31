using System.Collections;
using System.Data.Common;

namespace Kinetix.Data.SqlClient.Test {
    /// <summary>
    /// Factory de connexions de test.
    /// </summary>
    public sealed class TestDbProviderFactory : DbProviderFactory {

        private static IList _list = null;
        private static TestDbConnection _lastConnection = null;

        /// <summary>
        /// Retourne la dernière connexion créée.
        /// </summary>
        public static TestDbConnection LastConnection {
            get {
                return _lastConnection;
            }
        }

        /// <summary>
        /// Définit le prochain résultat retournée par une connexion base de données.
        /// </summary>
        /// <param name="list">Liste de résultat.</param>
        public static void DefinedNextResult(IList list) {
            _list = list;
        }

        /// <summary>
        /// Crée une connexion.
        /// </summary>
        /// <returns>Connexion.</returns>
        public override DbConnection CreateConnection() {
            IList list = _list;
            _list = null;
            TestDbConnection connection = new TestDbConnection(list);
            _lastConnection = connection;
            return connection;
        }
    }
}
