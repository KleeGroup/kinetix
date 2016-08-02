using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;

namespace Kinetix.ServiceModel.Unity {

    /// <summary>
    /// Argument dédié à la modification de la chaine d'interception lors de l'enregistrement d'un service dans le ServiceManager.
    /// </summary>
    public class InterceptionPipelineEventArgs : EventArgs {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="contractType">Type de l'interface contractualisant le service.</param>
        /// <param name="interceptors">Liste des intercepteurs.</param>
        public InterceptionPipelineEventArgs(Type contractType, IEnumerable<InjectionMember> interceptors) {
            if (contractType == null) {
                throw new ArgumentNullException("contractType");
            }

            if (interceptors == null) {
                throw new ArgumentNullException("interceptors");
            }

            this.ContractType = contractType;
            this.Interceptors = new List<InjectionMember>(interceptors);
        }

        /// <summary>
        /// Type du contrat de service.
        /// </summary>
        public Type ContractType {
            get;
            private set;
        }

        /// <summary>
        /// Liste des intercepteurs Unity mis en place pour l'appel du service.
        /// Modifier le contenu de cette collection pour changer le pipeline d'exécution par défaut.
        /// </summary>
        public IList<InjectionMember> Interceptors {
            get;
            private set;
        }
    }
}
