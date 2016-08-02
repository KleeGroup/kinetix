using System;
using System.Collections.Generic;

namespace Kinetix.Reporting {

    /// <summary>
    /// Class which contains the export request tokens.
    /// </summary>
    public static class ExportCache {

        /// <summary>
        /// Static dictionnary.
        /// </summary>
        private static readonly IDictionary<Guid, ExportConfiguration> ExportConfigurationMap = new Dictionary<Guid, ExportConfiguration>();

        /// <summary>
        /// Add a configuration element.
        /// </summary>
        /// <param name="configuration">Configuration element to add.</param>
        public static void AddConfiguration(ExportConfiguration configuration) {
            if (configuration == null) {
                throw new ArgumentNullException("configuration");
            }

            try {
                ExportConfigurationMap.Add(configuration.Id, configuration);
            } catch (ArgumentException) {

                // In the key has already been added, don't do anything. The configuration cannot be reloaded until the first export was done.
                return;
            }
        }

        /// <summary>
        /// Get the configuration to export.
        /// </summary>
        /// <param name="id">Identifiier in the cache configuration.</param>
        /// <returns>Configuration element.</returns>
        public static ExportConfiguration GetConfiguration(Guid id) {
            ExportConfiguration config;
            ExportConfigurationMap.TryGetValue(id, out config);
            if (config != null) {

                // Remove the configuration when accessed.
                ExportConfigurationMap.Remove(id);
                return config;
            }

            return null;
        }
    }
}
