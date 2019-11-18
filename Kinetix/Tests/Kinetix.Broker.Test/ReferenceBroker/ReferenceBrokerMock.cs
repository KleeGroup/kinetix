using System;
using Kinetix.ServiceModel;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Surcharge de ReferenceBroker utilisant le TestStore.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReferenceBrokerMock<T> : ReferenceBroker<T>
        where T : class, new() {
        /// <summary>
        /// Constructeur. Pas de surcharge particulière.
        /// </summary>
        /// <param name="dataSourceName">Nom de la source de données.</param>
        /// <param name="resourceLoader">Service de chargement des ressources.</param>
        /// <param name="resourceWriter">Service d'écriture des ressources.</param>
        public ReferenceBrokerMock(string dataSourceName, IResourceLoader resourceLoader, IResourceWriter resourceWriter)
            : base(dataSourceName, resourceLoader, resourceWriter) {
        }

        /// <summary>
        /// Surcharge utilisant le TestStore.
        /// </summary>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        protected override IStore<T> CreateStore(string dataSourceName) {
            Type storeType = typeof(TestStore<>);
            Type realStoreType = storeType.MakeGenericType(typeof(T));
            return (IStore<T>)Activator.CreateInstance(realStoreType, dataSourceName);
        }
    }

}
