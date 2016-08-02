using System;
using System.Collections.Generic;
using System.IO;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Storage;

namespace Kinetix.Monitoring.Network {

    /// <summary>
    /// Implémentation du protocole en version 1.
    /// </summary>
    public sealed class MonitoringProtocolV1 : IMonitoringProtocolReader, IMonitoringProtocolWriter {

        /// <summary>
        /// Crée une nouvelle implémentation du protocole.
        /// </summary>
        private MonitoringProtocolV1() {
        }

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="hostName">Nom de l'hôte.</param>
        /// <param name="endPoint">Point d'écoute.</param>
        /// <param name="moduleName">Nom du module.</param>
        private MonitoringProtocolV1(string hostName, string endPoint, string moduleName) {
            this.HostName = hostName;
            this.EndPoint = endPoint;
            this.ModuleName = moduleName;
        }

        /// <summary>
        /// Obtient le nom d'hote.
        /// </summary>
        public string HostName {
            get;
            private set;
        }

        /// <summary>
        /// Obtient l'EndPoint.
        /// </summary>
        public string EndPoint {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le nom du module.
        /// </summary>
        public string ModuleName {
            get;
            private set;
        }

        /// <summary>
        /// Retourne une instance pour l'écriture de données de monitoring sur le réseau.
        /// </summary>
        /// <returns>Writer.</returns>
        public static IMonitoringProtocolWriter CreateWriter() {
            return new MonitoringProtocolV1();
        }

        /// <summary>
        /// Retourne un reader pour la lecture du protocol de monitoring.
        /// </summary>
        /// <param name="hostName">Nom de l'hôte.</param>
        /// <param name="endPoint">Point d'écoute.</param>
        /// <param name="moduleName">Nom du module.</param>
        /// <returns>Le lecteur de protocole.</returns>
        public static IMonitoringProtocolReader CreateReader(string hostName, string endPoint, string moduleName) {
            return new MonitoringProtocolV1(hostName, endPoint, moduleName);
        }

        /// <summary>
        /// Ecrit la définition d'une base de données.
        /// </summary>
        /// <param name="writer">Writer binaire.</param>
        /// <param name="databaseDefinition">Définition de la base de données.</param>
        void IMonitoringProtocolWriter.WriteDatabaseDefinition(BinaryWriter writer, IDatabaseDefinition databaseDefinition) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            if (databaseDefinition == null) {
                throw new ArgumentNullException("databaseDefinition");
            }

            writer.Write(databaseDefinition.Name);
            writer.Write(databaseDefinition.Description);
            writer.Write(databaseDefinition.Priority);
            writer.Write(databaseDefinition.ImageMimeType);
            writer.Write(databaseDefinition.ImageData.Length);
            writer.Write(databaseDefinition.ImageData);
        }

        /// <summary>
        /// Lit la définition d'une base de données.
        /// </summary>
        /// <param name="reader">Lecteur binaire.</param>
        /// <returns>La définition de la base de données.</returns>
        IDatabaseDefinition IMonitoringProtocolReader.ReadDatabaseDefinition(BinaryReader reader) {
            if (reader == null) {
                throw new ArgumentNullException("reader");
            }

            string databaseName = reader.ReadString();

            NetworkManagerDescription managerDescription = new NetworkManagerDescription();
            managerDescription.Name = reader.ReadString();
            managerDescription.Priority = reader.ReadInt32();
            managerDescription.Image = "remote.png";
            managerDescription.ImageMimeType = reader.ReadString();
            int imageDataLength = reader.ReadInt32();
            managerDescription.ImageData = reader.ReadBytes(imageDataLength);

            return new DatabaseDefinition(databaseName, managerDescription);
        }

        /// <summary>
        /// Lit la définition d'un compteur.
        /// </summary>
        /// <param name="writer">Writer binaire.</param>
        /// <param name="counterDefinition">Définition du compteur.</param>
        void IMonitoringProtocolWriter.WriteCounterDefinition(BinaryWriter writer, ICounterDefinition counterDefinition) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            if (counterDefinition == null) {
                throw new ArgumentNullException("counterDefinition");
            }

            writer.Write(counterDefinition.Label);
            writer.Write(counterDefinition.Code);
            writer.Write(counterDefinition.WarningThreshold);
            writer.Write(counterDefinition.CriticalThreshold);
            writer.Write(counterDefinition.Priority);
        }

        /// <summary>
        /// Lit la définition d'un compteur.
        /// </summary>
        /// <param name="reader">Lecteur binaire.</param>
        /// <returns>La définition du compteur.</returns>
        ICounterDefinition IMonitoringProtocolReader.ReadCounterDefinition(BinaryReader reader) {
            if (reader == null) {
                throw new ArgumentNullException("reader");
            }

            CounterDefinition counterDefinition = new CounterDefinition(
                reader.ReadString(),
                reader.ReadString(),
                reader.ReadInt64(),
                reader.ReadInt64(),
                reader.ReadInt32());
            return counterDefinition;
        }

        /// <summary>
        /// Lit les données sur les compteurs.
        /// </summary>
        /// <param name="writer">Writer binaire.</param>
        /// <param name="counters">Compteurs.</param>
        void IMonitoringProtocolWriter.WriteCounterData(BinaryWriter writer, ICollection<CounterData> counters) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            if (counters == null) {
                throw new ArgumentNullException("counters");
            }

            writer.Write((byte)counters.Count);

            foreach (CounterData counter in counters) {
                string counterCode = string.IsNullOrEmpty(counter.CounterCode) ? string.Empty : counter.CounterCode;

                writer.Write(counter.Axis);
                writer.Write(counterCode);
                writer.Write(counter.DatabaseName);
                writer.Write(counter.Hits);
                writer.Write(counter.Last);
                writer.Write(counter.Level);
                writer.Write(counter.Max);
                writer.Write(counter.MaxName);
                writer.Write(counter.Min);
                writer.Write(counter.MinName);
                writer.Write(counter.StartDate.Ticks);
                writer.Write(counter.SubAvg);
                writer.Write(counter.Total);
                writer.Write(counter.TotalOfSquares);
                writer.Write(counter.Sample.Count);
                foreach (CounterSampleData sampleData in counter.Sample) {
                    writer.Write(sampleData.SampleValue);
                    writer.Write(sampleData.SampleCount);
                }
            }
        }

        /// <summary>
        /// Lit les données sur les compteurs.
        /// </summary>
        /// <param name="reader">Lecteur binaire.</param>
        /// <returns>La liste des compteurs.</returns>
        ICollection<CounterData> IMonitoringProtocolReader.ReadCounterData(BinaryReader reader) {
            if (reader == null) {
                throw new ArgumentNullException("reader");
            }

            byte dataCount = reader.ReadByte();
            List<CounterData> counters = new List<CounterData>(dataCount);
            for (int i = 0; i < dataCount; ++i) {

                CounterData data = new CounterData() {
                    Axis = reader.ReadString(),
                    CounterCode = reader.ReadString(),
                    DatabaseName = reader.ReadString(),
                    Hits = reader.ReadDouble(),
                    Last = reader.ReadDouble(),
                    Level = reader.ReadString(),
                    Max = reader.ReadDouble(),
                    MaxName = reader.ReadString(),
                    Min = reader.ReadDouble(),
                    MinName = reader.ReadString(),
                    StartDate = new DateTime(reader.ReadInt64()),
                    SubAvg = reader.ReadDouble(),
                    Total = reader.ReadDouble(),
                    TotalOfSquares = reader.ReadDouble(),
                };

                int sampleCount = reader.ReadInt32();
                for (int j = 0; j < sampleCount; ++j) {
                    data.Sample.Add(new CounterSampleData() {
                        SampleValue = reader.ReadDouble(),
                        SampleCount = reader.ReadInt32()
                    });
                }

                counters.Add(data);
            }

            return counters;
        }
    }
}
