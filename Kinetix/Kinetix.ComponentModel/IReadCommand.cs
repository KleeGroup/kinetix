using System.Data;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Interface de définition d'un broker d'accès aux données.
    /// </summary>
    public interface IReadCommand {

        /// <summary>
        /// Retourne la liste des paramétres de la commande.
        /// </summary>
        IDataParameterCollection Parameters {
            get;
        }

        /// <summary>
        /// Exécute une commande de selection et retourne un dataReader.
        /// </summary>
        /// <returns>DataReader.</returns>
        IDataReader ExecuteReader();
    }
}
