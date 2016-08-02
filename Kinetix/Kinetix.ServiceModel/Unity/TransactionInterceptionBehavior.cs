using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Transactions;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Kinetix.ServiceModel.Unity {

    /// <summary>
    /// Intercepteur posant un contexte transactionnel.
    /// </summary>
    public class TransactionInterceptionBehavior : IInterceptionBehavior {

        /// <summary>
        /// Doit être exécuté quoiqu'il arrive.
        /// </summary>
        public bool WillExecute {
            get {
                return true;
            }
        }

        /// <summary>
        /// Retourne l'ensemble des interfaces requises pour l'interception.
        /// </summary>
        /// <returns>Liste vide.</returns>
        public IEnumerable<Type> GetRequiredInterfaces() {
            return new List<Type>();
        }

        /// <summary>
        /// Execution de l'intercepteur.
        /// </summary>
        /// <param name="input">Paramètres d'appel de la méthode cible.</param>
        /// <param name="getNext">Delegate à invoquer pour appeler le prochain handler de la chaine.</param>
        /// <returns>Valeur de retour de la cible.</returns>
        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            if (getNext == null) {
                throw new ArgumentNullException("getNext");
            }

            if (Transaction.Current != null) {
                return getNext()(input, getNext);
            }

            TransactionFlowAttribute[] attrs = (TransactionFlowAttribute[])input.MethodBase.GetCustomAttributes(typeof(TransactionFlowAttribute), true);
            if (Transaction.Current == null && (attrs.Length != 0 && attrs[0].Transactions == TransactionFlowOption.NotAllowed)) {
                return getNext()(input, getNext);
            }

            IMethodReturn retValue;
            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                retValue = getNext()(input, getNext);
                if (retValue.Exception == null && tx != null && Transaction.Current.TransactionInformation.Status != TransactionStatus.Aborted) {
                    tx.Complete();
                }
            }

            return retValue;
        }
    }
}
