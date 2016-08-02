using System;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Kinetix.ServiceModel.Unity {

    /// <summary>
    /// Intercepteur posant du log.
    /// </summary>
    public class LogInterceptionBehavior : IInterceptionBehavior {

        /// <summary>
        /// Retourne <code>True</code> quoiqu'il arrive.
        /// </summary>
        public bool WillExecute {
            get {
                return true;
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
        /// Effectue l'appel de la méthode, inscrit l'exception dans le log si erreur, sinon mesure du temps.
        /// </summary>
        /// <param name="input">Méthode cible de l'appel.</param>
        /// <param name="getNext">Delegate.</param>
        /// <returns>Valeur de retour de la cible.</returns>
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            if (getNext == null) {
                throw new ArgumentNullException("getNext");
            }

            ILog log = LogManager.GetLogger("Service");
            Stopwatch watch = Stopwatch.StartNew();
            watch.Start();
            IMethodReturn retValue = getNext()(input, getNext);
            watch.Stop();
            if (retValue.Exception != null) {
                if (log.IsErrorEnabled) {
                    log.Error("Erreur sur le service " + input.MethodBase.DeclaringType.FullName + "." + input.MethodBase.Name, retValue.Exception);
                }
            } else if (log.IsInfoEnabled) {
                log.InfoFormat("Service {0}.{1}.{2}", input.MethodBase.DeclaringType.FullName, input.MethodBase.Name, watch.Elapsed.Seconds);
            }

            return retValue;
        }
    }
}
