using System.ServiceModel;

namespace Kinetix.Configuration {

    /// <summary>
    /// Contract for the business config loading service.
    /// </summary>
    [ServiceContract]
    public interface IBusinessConfigLoader {

        /// <summary>
        /// Load a business config item from its key.
        /// </summary>
        /// <param name="key">Businnes config item key.</param>
        /// <returns>Item.</returns>
        [OperationContract]
        BusinessConfigItem LoadBusinessConfigItem(string key);
    }
}
