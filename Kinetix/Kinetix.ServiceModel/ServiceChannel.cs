using System;
using System.ServiceModel;
using log4net;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Canal d'accès à un service.
    /// La libération du canal permet de gérer correctement les connexions
    /// vers des services distants.
    ///
    /// Toujours utiliser ServiceChannel dans un block using.
    /// </summary>
    /// <typeparam name="T">Type de l'interface du service.</typeparam>
    public sealed class ServiceChannel<T> : IDisposable {
        private readonly T _service;
        private readonly ChannelFactory _factory;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        public ServiceChannel() {
            if (!typeof(T).IsInterface) {
                throw new ArgumentException("Not an interface");
            }

            _service = (T)ServiceManager.Instance.GetService(typeof(T), out _factory);
            if (_factory != null) {
                _factory.Faulted += new EventHandler(Factory_Faulted);
            }
        }

        /// <summary>
        /// Implémentation du service.
        /// </summary>
        public T Service {
            get {
                return _service;
            }
        }

        /// <summary>
        /// Libère les ressources.
        /// Les erreurs sur la fermeture du canal sont ignorées.
        /// </summary>
        public void Dispose() {
            // Fermeture du canal de communication.
            if (_factory != null) {
                try {
                    _factory.Close();
                } catch (CommunicationObjectFaultedException) {
                    return;
                } catch (CommunicationException) {
                    return;
                } catch (TimeoutException) {
                    return;
                }
            }
        }

        /// <summary>
        /// Rajout d'une trace sur les éléments de faute.
        /// </summary>
        /// <param name="sender">Emetteur de l'évènement.</param>
        /// <param name="e">Argument de l'évènement.</param>
        private void Factory_Faulted(object sender, EventArgs e) {
            ILog log = LogManager.GetLogger("Application");
            if (log.IsErrorEnabled) {
                log.Error("ChannelFactory est en état faulted pour le service " + typeof(T).FullName);
            }

            _factory.Abort();
        }
    }
}
