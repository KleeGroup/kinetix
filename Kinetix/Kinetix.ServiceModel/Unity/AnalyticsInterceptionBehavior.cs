using System;
using System.Collections.Generic;
using System.Web;
using Kinetix.Monitoring.Counter;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Kinetix.ServiceModel.Unity {

    /// <summary>
    /// Intercepteur permettant d'ajouter Analytics.
    /// </summary>
    public class AnalyticsInterceptionBehavior : IInterceptionBehavior {

        private const string CounterTotalServiceCallCount = "TOTAL_SERVICE_CALL_COUNT";
        private const string CounterTotalServiceErrorCount = "TOTAL_SERVICE_ERROR_COUNT";
        private const string ServiceHyperCube = "SERVICEDB";

        /// <summary>
        /// Ne s'exécute que dans un environnement Web.
        /// </summary>
        public bool WillExecute {
            get {
                return HttpContext.Current != null;
            }
        }

        /// <summary>
        /// Retourne la liste des interfaces requises.
        /// </summary>
        /// <returns>Liste vide ici.</returns>
        public IEnumerable<Type> GetRequiredInterfaces() {
            return new List<Type>();
        }

        /// <summary>
        /// Invocation de la méthode, rajoute les advices nécessaires.
        /// </summary>
        /// <param name="input">Methode cible.</param>
        /// <param name="getNext">Delegate suivant dans la chaine d'appel.</param>
        /// <returns>Valeur de retour de la méthode.</returns>
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            if (getNext == null) {
                throw new ArgumentNullException("getNext");
            }

            Analytics.Instance.IncValue(CounterTotalServiceCallCount, 1);
            Analytics.Instance.StartProcess(input.MethodBase.DeclaringType.FullName + "." + input.MethodBase.Name);
            IMethodReturn retValue = getNext()(input, getNext);
            Analytics.Instance.StopProcess(ServiceHyperCube);
            if (retValue.Exception != null) {
                Analytics.Instance.IncValue(CounterTotalServiceErrorCount, 100);
                Analytics.Instance.HandleException(retValue.Exception);
            }

            return retValue;
        }
    }
}
