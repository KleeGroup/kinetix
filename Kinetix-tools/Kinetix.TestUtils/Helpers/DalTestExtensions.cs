using System;
using System.Data.SqlClient;
using Kinetix.Broker;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;

namespace Kinetix.TestUtils.Helpers {

    /// <summary>
    /// Méthodes d'extensions des tests de DAL.
    /// </summary>
    public static class DalTestExtensions {

        /// <summary>
        /// Test un appel de DAL en ne vérifiant que la syntaxe et le modèle.
        /// Les erreurs dues aux données ne mettent pas le test en échec :
        ///   - ZeroRowException
        ///   - Foreign Key.
        /// </summary>
        /// <param name="testClass">Classe de test.</param>
        /// <param name="action">Action pour appeler la DAL.</param>
        public static void CheckDalSyntax(this IDalTest testClass, Action action) {

            try {
                action();
            } catch (BrokerException be) {

                /* Cas liés aux données : le test passe. */
                switch (be.Message) {
                    case "Zero row deleted":
                    case "Zero record affected":
                        return;
                }

                /* Autres cas : on relance l'exception */
                throw be;
            } catch (CollectionBuilderException cbe) {

                /* Cas liés aux données : le test passe. */
                switch (cbe.Message) {
                    case "Zero row selected !":
                        return;
                }

                /* Autres cas : on relance l'exception */
                throw cbe;
            } catch (SqlException se) {

                if (HandleSqlException(se)) {
                    return;
                }

                throw se;
            } catch (ConstraintException ce) {

                var sqlException = ce.InnerException as SqlException;
                if (sqlException != null) {
                    if (HandleSqlException(sqlException)) {
                        return;
                    }
                }

                throw ce;
            } catch (NotSupportedException nse) {

                /* Cas d'un ExecuteScalar qui renvoie null : le test passe. */
                if (nse.Message == "Null result is not supported.") {
                    return;
                }

                throw nse;
            }
        }

        /// <summary>
        /// Gère une exception SQL.
        /// </summary>
        /// <param name="sqlException">Exception SQL.</param>
        /// <returns><code>True</code> si le test passe.</returns>
        private static bool HandleSqlException(SqlException sqlException) {

            /* Cas d'une violation de clé étrangère : le test passe. */
            var sqlMessage = sqlException.Message;
            if (sqlMessage.Contains("the REFERENCE constraint")) {
                return true;
            }

            if (sqlMessage.Contains("the FOREIGN KEY constraint")) {
                return true;
            }

            return false;
        }
    }
}
