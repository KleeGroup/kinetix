using System;
using System.Collections.Generic;
using Kinetix.ServiceModel;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Implémentation simple de IResourceLoader.
    /// S'appuie sur ReferenceBrokerTestHelper.
    /// </summary>
    public class ServiceResourceLoaderMock : IResourceLoader {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string LoadCurrentLangueCode() {
            return ReferenceBrokerTestHelper.LangueCode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string LoadLangueCodeDefaut() {
            return "EN";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="referenceType"></param>
        /// <returns></returns>
        public ICollection<ReferenceResource> LoadReferenceResourceListByReferenceType(Type referenceType) {
            throw new NotImplementedException();
        }
    }
}
