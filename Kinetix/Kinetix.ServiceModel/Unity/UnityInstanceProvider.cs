using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.Unity;

namespace Kinetix.ServiceModel.Unity {

    /// <summary>
    /// Provider de type s'appuyant sur Unity.
    /// </summary>
    public class UnityInstanceProvider : IInstanceProvider {

        private readonly IUnityContainer _container;
        private readonly Type _type;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="container">Conteneur Unity.</param>
        /// <param name="type">Le type devant être créé.</param>
        /// <remarks>
        /// Si <paramref name="container"/> est null alors utilisation de la configuration par défaut.
        /// Si la section Unity est introuvable, résolution normale du type.
        /// </remarks>
        public UnityInstanceProvider(IUnityContainer container, Type type) {
            _container = container;
            _type = type;
        }

        /// <summary>
        /// Retourne le service à partir du contexte de l'instance d'appel.
        /// </summary>
        /// <param name="instanceContext">Le contexte d'instance.</param>
        /// <returns>L'objet.</returns>
        public object GetInstance(InstanceContext instanceContext) {
            return GetInstance(instanceContext, null);
        }

        /// <summary>
        /// Retourne l'objet à partir du contexte de l'instance d'appel.
        /// </summary>
        /// <param name="instanceContext">Le contexte d'instance.</param>
        /// <param name="message">Le message ayant déclenché la création de l'objet.</param>
        /// <returns>Le service.</returns>
        public object GetInstance(InstanceContext instanceContext, Message message) {
            return _container.Resolve(_type, _type.Name);
        }

        /// <summary>
        /// Appelé lorsque l'instance est recyclée.
        /// </summary>
        /// <param name="instanceContext">Le contexte de l'instance du service.</param>
        /// <param name="instance">Le service à recycler.</param>
        public void ReleaseInstance(InstanceContext instanceContext, object instance) {
            _container.Teardown(instance);
            IDisposable disposable = instance as IDisposable;
            if (disposable != null) {
                disposable.Dispose();
            }
        }
    }
}
