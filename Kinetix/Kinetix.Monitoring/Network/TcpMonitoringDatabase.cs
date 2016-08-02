using System;
using System.Collections.Generic;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Storage;

namespace Kinetix.Monitoring.Network {
    /// <summary>
    /// Base de données centrales du service.
    /// </summary>
    public class TcpMonitoringDatabase {

        private readonly Dictionary<string, IDatabaseDefinition> _databases = new Dictionary<string, IDatabaseDefinition>();
        private readonly Dictionary<string, ICounterDefinition> _counters = new Dictionary<string, ICounterDefinition>();
        private readonly Dictionary<string, ExternalHyperCube> _hyperCubes = new Dictionary<string, ExternalHyperCube>();

        /// <summary>
        /// Retourne le dictionnaires des base de données.
        /// </summary>
        public IDictionary<string, IDatabaseDefinition> Databases {
            get {
                return _databases;
            }
        }

        /// <summary>
        /// Retourne le dictionnaires des compteurs.
        /// </summary>
        public IDictionary<string, ICounterDefinition> Counters {
            get {
                return _counters;
            }
        }

        /// <summary>
        /// Retourne le dictionnaires des cubes.
        /// </summary>
        public IDictionary<string, ExternalHyperCube> Cubes {
            get {
                return _hyperCubes;
            }
        }

        /// <summary>
        /// Ajoute la définition d'une base de données.
        /// </summary>
        /// <param name="databaseDefinition">Définition d'une base de données.</param>
        /// <param name="protocolReader">Interface de lecture.</param>
        public void AddDatabaseDefinition(IDatabaseDefinition databaseDefinition, IMonitoringProtocolReader protocolReader) {
            if (protocolReader == null) {
                throw new ArgumentNullException("protocolReader");
            }

            if (databaseDefinition == null) {
                throw new ArgumentNullException("databaseDefinition");
            }

            if (string.IsNullOrEmpty(databaseDefinition.Name)) {
                throw new NotSupportedException("databaseDefinition.Name");
            }

            string keyBase = protocolReader.HostName + "&" + protocolReader.EndPoint + "&" + protocolReader.ModuleName + "&";
            _databases[keyBase + databaseDefinition.Name] = databaseDefinition;
        }

        /// <summary>
        /// Ajoute la définition d'un compteur.
        /// </summary>
        /// <param name="counterDefinition">Définition du compteur.</param>
        /// <param name="protocolReader">Interface de lecture.</param>
        public void AddCounterDefinition(ICounterDefinition counterDefinition, IMonitoringProtocolReader protocolReader) {
            if (protocolReader == null) {
                throw new ArgumentNullException("protocolReader");
            }

            if (counterDefinition == null) {
                throw new ArgumentNullException("counterDefinition");
            }

            if (string.IsNullOrEmpty(counterDefinition.Code)) {
                throw new NotSupportedException("counterDefinition.Code");
            }

            string keyBase = protocolReader.HostName + "&" + protocolReader.EndPoint + "&" + protocolReader.ModuleName + "&";
            _counters[keyBase + counterDefinition.Code] = counterDefinition;
        }

        /// <summary>
        /// Ajoute une collection de compteurs à la base de données.
        /// </summary>
        /// <param name="counters">Collection de compteur.</param>
        /// <param name="protocolReader">Interface de lecture.</param>
        public void AddCounters(ICollection<CounterData> counters, IMonitoringProtocolReader protocolReader) {
            if (counters == null) {
                throw new ArgumentNullException("counters");
            }

            if (protocolReader == null) {
                throw new ArgumentNullException("protocolReader");
            }

            string keyBase = protocolReader.HostName + "&" + protocolReader.EndPoint + "&" + protocolReader.ModuleName + "&";
            foreach (CounterData data in counters) {
                string hyperCubeKey = keyBase + data.DatabaseName;
                ExternalHyperCube hyperCube;
                if (!_hyperCubes.TryGetValue(hyperCubeKey, out hyperCube)) {
                    hyperCube = new ExternalHyperCube(data.DatabaseName, _counters.Values);
                    _hyperCubes.Add(hyperCubeKey, hyperCube);
                }

                DateTime startDate = data.StartDate;
                DateTime mergeDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0);
                data.Level = "HEU";
                hyperCube.AddCounter(data, true, mergeDate);
            }
        }
    }
}
