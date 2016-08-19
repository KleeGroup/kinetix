using System;
using System.Reflection;
using System.ServiceModel;
using Kinetix.ServiceModel;
using Microsoft.Practices.Unity;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Implémentation par défaut de IResourceServiceFactory.
    /// </summary>
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, IncludeExceptionDetailInFaults = true)]
    public class DefaultResourceServiceFactory : IResourceServiceFactory {

        /// <summary>
        /// Constructeur par défaut.
        /// </summary>
        public DefaultResourceServiceFactory() {
        }

        /// <inheritDoc cref="IResourceServiceFactory.GetLoaderService" />
        public IResourceLoader GetLoaderService() {
            return (IResourceLoader)ServiceManager.Instance.Container.Resolve<IResourceLoader>();
        }

        /// <inheritDoc cref="IResourceServiceFactory.GetWriterService" />
        public IResourceWriter GetWriterService() {
            return (IResourceWriter)ServiceManager.Instance.Container.Resolve<IResourceWriter>();
        }
    }
}
