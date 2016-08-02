using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.Unity;

namespace Kinetix.ServiceModel.Unity {

    /// <summary>
    /// ServiceBehavior permettant de s'appuyer sur Unity.
    /// </summary>
    public class UnityServiceBehavior : IServiceBehavior {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="container">Le conteneur Unity.</param>
        public UnityServiceBehavior(IUnityContainer container)
            : this(container, null) {
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="container">Le conteneur.</param>
        /// <param name="unityResolveName">Nom du conteneur Unity à résoudre.</param>
        public UnityServiceBehavior(IUnityContainer container, string unityResolveName) {
            if (container == null) {
                throw new ArgumentNullException("container");
            }

            this.Container = container;
            this.ResolveName = unityResolveName;
        }

        /// <summary>
        /// Obtient ou définit le conteneur Unity.
        /// </summary>
        protected IUnityContainer Container {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le nom.
        /// </summary>
        protected string ResolveName {
            get;
            set;
        }

        /// <summary>
        /// Offre la possibilité d'ajouter des éléments particuliers au binding pour supporter l'implémentation du contrat.
        /// </summary>
        /// <param name="serviceDescription">Description du service.</param>
        /// <param name="serviceHostBase">ServiceHost hébergeant le service.</param>
        /// <param name="endpoints">EndPoints du service.</param>
        /// <param name="bindingParameters">Elements particuliers rajoutés au binding.</param>
        /// <remarks>Non utilisé ici.</remarks>
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) {
        }

        /// <summary>
        /// Offre la possibilité de changer des propriétés au runtime ou insertion d'extensions spécifiques.
        /// Par exemmple, Error Handlers, Message / Parameter Interceptor, Security Extension etc.
        /// </summary>
        /// <param name="serviceDescription">La description du service.</param>
        /// <param name="serviceHostBase">Le ServiceHost en cours de construction.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {
            if (serviceDescription == null) {
                throw new ArgumentNullException("serviceDescription");
            }

            if (serviceHostBase == null) {
                throw new ArgumentNullException("serviceHostBase");
            }

            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers) {
                foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints) {
                    if (endpointDispatcher.ContractName != "IMetadataExchange") {
                        endpointDispatcher.DispatchRuntime.InstanceProvider = new UnityInstanceProvider(this.Container, serviceDescription.ServiceType);
                    }
                }
            }
        }

        /// <summary>
        /// Offre la possibilité d'inspecter le ServiceHost et sa description pour confirmer celle-ci.
        /// </summary>
        /// <param name="serviceDescription">La description du service.</param>
        /// <param name="serviceHostBase">Le ServiceHost en cours de construction.</param>
        /// <remarks>Non utilisé ici.</remarks>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) {
        }
    }
}