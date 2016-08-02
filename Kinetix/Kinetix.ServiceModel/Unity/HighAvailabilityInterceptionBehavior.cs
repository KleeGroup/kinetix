using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Kinetix.ServiceModel.Unity {

    /// <summary>
    /// Intercepteur permettant de gérer la haute disponibilité sous SQL Server.
    /// L'intercepteur suit le fonctionnement suivant :
    ///     - Serialisation des paramètres d'entrée.
    ///     - Exécution du service.
    ///     - Si erreur de connexion à la base ou deadlock, alors rejeu à partir des paramètres d'entrée sérialisés.
    /// </summary>
    public class HighAvailabilityInterceptionBehavior : IInterceptionBehavior {

        /// <summary>
        /// L'intercepteur s'execute que s'il correspond au service d'appel de plus au niveau.
        /// Stockage d'un booléen dans la TLS du thread courant avec comme clef _HAIB_ContextID.
        /// Retourne donc si le slot vient d'être créé ou non.
        /// </summary>
        public bool WillExecute {
            get {
                string key = "_HAIB_" + Thread.CurrentContext.ContextID.ToString(CultureInfo.InvariantCulture);
                LocalDataStoreSlot threadData = Thread.GetNamedDataSlot(key);
                if (Thread.GetData(threadData) == null) {
                    Thread.SetData(threadData, true);
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Retourne l'ensemble des interfaces requises.
        /// </summary>
        /// <returns>Liste vide.</returns>
        public IEnumerable<Type> GetRequiredInterfaces() {
            return new List<Type>();
        }

        /// <summary>
        /// Invocation de l'intercepteur.
        /// </summary>
        /// <param name="input">Méthode cible.</param>
        /// <param name="getNext">Delegate à invoquer pour appeler le prochain handler de la chaine d'interception.</param>
        /// <returns>Valeur de retour de la cible.</returns>
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            if (getNext == null) {
                throw new ArgumentNullException("getNext");
            }

            byte[] args = null;
            if (input.Arguments.Count != 0) {
                args = SerializeArguments(input.Arguments);
            }

            return EnsureServiceCall(input, getNext, args);
        }

        /// <summary>
        /// Execute le service en effectuant un rejeu si erreur de connexion à la base ou deadlock.
        /// </summary>
        /// <param name="input">Méthode cible.</param>
        /// <param name="getNext">Delegate à invoquer pour appeler le prochain handler de la chaine d'interception.</param>
        /// <param name="args">Arguments d'appel.</param>
        /// <returns>Valeur de retour de la cible.</returns>
        private static IMethodReturn EnsureServiceCall(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext, byte[] args) {
            IMethodReturn retValue = getNext()(input, getNext);
            if (retValue.Exception != null && retValue.Exception is SqlException) {
                SqlException e = (SqlException)retValue.Exception;
                if (e.Number == 1205) {
                    // Si deadlock.
                    if (input.Arguments.Count != 0) {
                        RestoreArguments(input, args);
                    }

                    return EnsureServiceCall(input, getNext, args);
                } else if (e.ErrorCode >= 20) {
                    // http://msdn.microsoft.com/fr-fr/library/system.data.sqlclient.sqlexception(v=VS.100).aspx
                    // Si connexion fermée, alors on tente le rejeu.
                    if (input.Arguments.Count != 0) {
                        RestoreArguments(input, args);
                    }

                    return EnsureServiceCall(input, getNext, args);
                }
            }

            return retValue;
        }

        /// <summary>
        /// Restaure les arguments d'appel de la méthode à partir de ceux précédemment sérialisés.
        /// </summary>
        /// <param name="input">Methode appelée.</param>
        /// <param name="args">Arguments précédemment sérialisés.</param>
        private static void RestoreArguments(IMethodInvocation input, byte[] args) {
            object[] parameters;
            using (MemoryStream ms = new MemoryStream(args)) {
                BinaryFormatter formatter = new BinaryFormatter();
                parameters = (object[])formatter.Deserialize(ms);
            }

            for (int i = 0; i < input.Arguments.Count; ++i) {
                input.Arguments[i] = parameters[i];
            }
        }

        /// <summary>
        /// Sérialise les paramètres d'appel de la méthode.
        /// </summary>
        /// <param name="inputArgs">Arguments d'appel de la méthode.</param>
        /// <returns>Le buffer contenant les paramètres sérialisés.</returns>
        private static byte[] SerializeArguments(IParameterCollection inputArgs) {
            object[] args = new object[inputArgs.Count];
            for (int i = 0; i < inputArgs.Count; ++i) {
                args[i] = inputArgs[i];
            }

            using (MemoryStream ms = new MemoryStream()) {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, args);
                return ms.GetBuffer();
            }
        }
    }
}
