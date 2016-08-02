using System.Data;

namespace Kinetix.Data.SqlClient {
    /// <summary>
    /// Contractualise la lecture d'un DataRecord et l'écriture des données dans un bean.
    /// </summary>
    /// <typeparam name="T">Type du bean.</typeparam>
    public interface IDataRecordAdapter<T>
        where T : new() {

        /// <summary>
        /// Lit un record et écrit ces données dans un bean.
        /// </summary>
        /// <param name="destination">Le bean à charger.</param>
        /// <param name="record">Enregistrement.</param>
        /// <returns>Bean.</returns>
        T Read(object destination, IDataRecord record);
    }
}
