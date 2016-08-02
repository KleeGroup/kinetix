using System;
using System.ServiceModel;
using Kinetix.ComponentModel;

namespace Kinetix.Reporting {
    /// <summary>
    /// Interface de définition d'un générateur de documents.
    /// </summary>
    [ServiceContract]
    public interface IReportGenerator {
        /// <summary>
        /// Génération d'un document au format donné.
        /// </summary>
        /// <param name="reportModelPrimaryKey">Clef primaire du modèle d'édition..</param>
        /// <param name="xmlData">Données utilisées par le modèle au format XML.</param>
        /// <param name="format">Format de fichier.</param>
        /// <returns>Document.</returns>
        [OperationContract]
        byte[] CreateFile(object reportModelPrimaryKey, string xmlData, FileFormat format);

        /// <summary>
        /// Génération d'un document au format donné.
        /// </summary>
        /// <param name="reportModelPrimaryKey">Clef primaire du modèle d'édition..</param>
        /// <param name="xmlData">Données utilisées par le modèle au format XML.</param>
        /// <param name="format">Format de fichier.</param>
        /// <param name="callback">Callback.</param>
        /// <param name="state">Etat.</param>
        /// <returns>Etat courant.</returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginCreateFile(object reportModelPrimaryKey, string xmlData, FileFormat format, AsyncCallback callback, object state);

        /// <summary>
        /// Termine la création d'un document au format donné.
        /// </summary>
        /// <param name="result">Etat courant.</param>
        /// <returns>Document.</returns>
        byte[] EndCreateFile(IAsyncResult result);
    }
}
