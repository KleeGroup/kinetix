using System;
using System.ServiceModel;
using Microsoft.Practices.Unity;

namespace Kinetix.ServiceModel.Unity {

    /// <summary>
    /// ServiceHost dédié pour la gestion d'Unity.
    /// </summary>
    public class UnityServiceHost : ServiceHost {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public UnityServiceHost()
            : base() {
            this.Container = new UnityContainer();
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="singleton">Objet cible.</param>
        /// <param name="baseAddresses">Url d'accès.</param>
        public UnityServiceHost(object singleton, params Uri[] baseAddresses)
            : base(singleton, baseAddresses) {
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="serviceType">Type de service.</param>
        /// <param name="baseAdresses">Url d'accès.</param>
        public UnityServiceHost(Type serviceType, params Uri[] baseAdresses)
            : base(serviceType, baseAdresses) {
        }

        /// <summary>
        /// Container Unity.
        /// </summary>
        public IUnityContainer Container {
            get;
            set;
        }

        /// <summary>
        /// Ouverture du ServiceHost.
        /// </summary>
        protected override void OnOpening() {
            base.OnOpening();
            if (this.Description.Behaviors.Find<UnityServiceBehavior>() == null) {
                this.Description.Behaviors.Add(new UnityServiceBehavior(this.Container));
            }
        }
    }
}
