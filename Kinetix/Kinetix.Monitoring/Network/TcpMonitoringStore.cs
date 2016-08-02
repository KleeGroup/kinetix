using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Sockets;
using System.ServiceModel.Configuration;
using System.Text;
using System.Web;
using Kinetix.Configuration;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Storage;

namespace Kinetix.Monitoring.Network {
    /// <summary>
    /// Store pour l'envoi des données de monitoring vers un service centralisé.
    /// </summary>
    public sealed class TcpMonitoringStore : IMonitoringStore {

        /// <summary>
        /// Début d'une trame d'envoi de définition d'un client.
        /// </summary>
        public const byte CstFrameClientDefinition = 5;

        /// <summary>
        /// Début d'une trame d'envoi de définition d'une base.
        /// </summary>
        public const byte CstFrameDatabaseDefinition = 10;

        /// <summary>
        /// Début d'une trame d'envoi de définition de compteur.
        /// </summary>
        public const byte CstFrameCounterDefinition = 11;

        /// <summary>
        /// Début d'une trame d'envoi de compteur.
        /// </summary>
        public const byte CstFrameCounterData = 20;
        private readonly IMonitoringProtocolWriter _protocolWriter;
        private readonly List<IDatabaseDefinition> _databaseList = new List<IDatabaseDefinition>();
        private readonly List<IDatabaseDefinition> _databaseChangeList = new List<IDatabaseDefinition>();
        private readonly List<ICounterDefinition> _counterList = new List<ICounterDefinition>();
        private readonly List<ICounterDefinition> _counterChangeList = new List<ICounterDefinition>();
        private readonly string _hostName;
        private readonly string _endPoint;
        private readonly string _moduleName;
        private readonly string _monitoringHost;
        private readonly int _monitoringPort;

        private TcpClient _client;
        private MemoryStream _stream;
        private BinaryWriter _writer;
        private bool _definitionTransmitted;

        /// <summary>
        /// Crée une nouvelle instance pointant vers un service de monitoring.
        /// </summary>
        /// <param name="monitoringHost">Nom de l'hôte.</param>
        /// <param name="monitoringPort">Port TCP d'écoute.</param>
        public TcpMonitoringStore(string monitoringHost, int monitoringPort) {
            _monitoringHost = monitoringHost;
            _monitoringPort = monitoringPort;

            _client = new TcpClient();
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream, Encoding.UTF8);
            _writer.Seek(4, SeekOrigin.Begin);
            _protocolWriter = MonitoringProtocolV1.CreateWriter();

            ModuleSection section = (ModuleSection)ConfigurationManager.GetSection(
                    ModuleSection.ModuleSectionName);

            _hostName = Environment.MachineName;
            _endPoint = GetEndPointUri();
            if (section != null && section.Module != null) {
                _moduleName = section.Module.Name;
            } else {
                _moduleName = Process.GetCurrentProcess().ProcessName;
            }
        }

        /// <summary>
        /// Crée une base de données.
        /// </summary>
        /// <param name="databaseDefinition">Définition de la base de données.</param>
        void IMonitoringStore.CreateDatabase(IDatabaseDefinition databaseDefinition) {
            lock (_databaseList) {
                _databaseList.Add(databaseDefinition);
                _databaseChangeList.Add(databaseDefinition);
            }
        }

        /// <summary>
        /// Crée une instance de CounterDefinition en la conservant en cache.
        /// </summary>
        /// <param name="counterDefinition">Définition du compteur.</param>
        void IMonitoringStore.CreateCounter(ICounterDefinition counterDefinition) {
            lock (_counterList) {
                _counterList.Add(counterDefinition);
                _counterChangeList.Add(counterDefinition);
            }
        }

        /// <summary>
        /// Sauvegarde les compteurs.
        /// </summary>
        /// <param name="counters">Compteurs.</param>
        void IMonitoringStore.StoreCounters(ICollection<CounterData> counters) {
            if (counters == null) {
                throw new ArgumentNullException("counters");
            }

            lock (_writer) {
                lock (_databaseList) {
                    List<IDatabaseDefinition> list = _databaseChangeList;
                    if (!_definitionTransmitted) {
                        list = _databaseList;
                    }

                    foreach (IDatabaseDefinition databaseDefinition in list) {
                        _writer.Write(CstFrameDatabaseDefinition);
                        _protocolWriter.WriteDatabaseDefinition(_writer, databaseDefinition);
                        this.SendData();
                    }

                    _databaseChangeList.Clear();
                }

                lock (_counterList) {
                    List<ICounterDefinition> list = _counterChangeList;
                    if (!_definitionTransmitted) {
                        list = _counterList;
                    }

                    foreach (ICounterDefinition counterDefinition in list) {
                        _writer.Write(CstFrameCounterDefinition);
                        _protocolWriter.WriteCounterDefinition(_writer, counterDefinition);
                        this.SendData();
                    }

                    _counterChangeList.Clear();
                }

                _definitionTransmitted = true;

                if (counters.Count > 0) {
                    _writer.Write(CstFrameCounterData);
                    _protocolWriter.WriteCounterData(_writer, counters);
                    this.SendData();
                }
            }
        }

        /// <summary>
        /// Traite une exception et l'enregistre en base de données.
        /// </summary>
        /// <param name="exception">Exception.</param>
        /// <returns>Numéro d'enregistrement en base de données.</returns>
        int IMonitoringStore.HandleException(Exception exception) {
            return 0;
        }

        /// <summary>
        /// Libère les ressources de l'objet.
        /// </summary>
        public void Dispose() {
            _writer.Close();
            _writer = null;
            _stream.Dispose();
            _stream = null;
            _client.Close();
            _client = null;
        }

        /// <summary>
        /// Retourne l'uri associé au point d'accès principal de l'application.
        ///
        /// Pour les applications, l'uri est de la forme :
        ///   - MACHINE_NAME;http[s]://logical_name:port/[path]/.
        /// </summary>
        /// <returns>Uri du point d'accès principal de l'application.</returns>
        private static string GetEndPointUri() {
            if (HttpContext.Current != null) {
                return Environment.MachineName;
            } else {
                try {
                    System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    ServiceModelSectionGroup serviceModelSection = ServiceModelSectionGroup.GetSectionGroup(configuration);
                    if (serviceModelSection != null) {
                        foreach (ServiceElement service in serviceModelSection.Services.Services) {
                            foreach (ServiceEndpointElement sep in service.Endpoints) {
                                if (sep.Address != null) {
                                    return sep.Address.ToString();
                                }
                            }
                        }
                    }
                } catch (ArgumentException) {
                    return Environment.MachineName;
                }

                return Environment.MachineName;
            }
        }

        /// <summary>
        /// Envoi les données au serveur.
        /// </summary>
        [DebuggerStepThrough]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Les exceptions du monitoring ne doivent pas remontée.")]
        private void SendData() {
            int length = (int)_writer.BaseStream.Position;
            _writer.Seek(0, SeekOrigin.Begin);
            _writer.Write(length - 4);

            try {
                if (this.CheckConnection()) {
                    try {
                        _client.GetStream().Write(_stream.GetBuffer(), 0, length);
                    } catch {
                        _client.Close();
                        _client = new TcpClient();
                    }
                }
            } finally {
                _writer.Seek(4, SeekOrigin.Begin);
            }
        }

        /// <summary>
        /// Vérifie que le connexion avec le serveur est ouverte.
        /// </summary>
        /// <returns>Indique si la connexion est active.</returns>
        [DebuggerStepThrough]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Les exceptions du monitoring ne doivent pas remontée.")]
        private bool CheckConnection() {
            try {
                if (!_client.Connected) {
                    _client.Connect(_monitoringHost, _monitoringPort);

                    using (MemoryStream stream = new MemoryStream())
                    using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8)) {
                        writer.Seek(4, SeekOrigin.Begin);

                        writer.Write(CstFrameClientDefinition);
                        writer.Write(_hostName);
                        writer.Write(_endPoint);
                        writer.Write(_moduleName);
                        writer.Write((byte)1);

                        int length = (int)writer.BaseStream.Position;
                        writer.Seek(0, SeekOrigin.Begin);
                        writer.Write(length - 4);

                        _client.GetStream().Write(stream.GetBuffer(), 0, length);
                        _definitionTransmitted = false;
                    }
                }

                return true;
            } catch {
                _client.Close();
                _client = new TcpClient();
                return false;
            }
        }
    }
}
