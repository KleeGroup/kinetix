using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Kinetix.Data.SqlClient {
    /// <summary>
    /// Factory d'adapter.
    /// </summary>
    /// <typeparam name="T">Type de données.</typeparam>
    internal static class DataRecordAdapterManager<T>
        where T : new() {

        private static Dictionary<string, IDataRecordAdapter<T>> _adaptorMap = new Dictionary<string, IDataRecordAdapter<T>>();

        /// <summary>
        /// Crée un adapteur.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <returns>Adapter.</returns>
        internal static IDataRecordAdapter<T> CreateAdapter(IDataRecord record) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < record.FieldCount; ++i) {
                sb.Append(record.GetName(i)).Append(',');
            }

            IDataRecordAdapter<T> adapter;
            lock (_adaptorMap) {
                if (!_adaptorMap.TryGetValue(sb.ToString(), out adapter)) {
                    adapter = InternalCreateAdapter(record);
                    _adaptorMap[sb.ToString()] = adapter;
                }
            }

            return adapter;
        }

        /// <summary>
        /// Crée un adapteur.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <returns>Adapter.</returns>
        private static IDataRecordAdapter<T> InternalCreateAdapter(IDataRecord record) {
            return (IDataRecordAdapter<T>)DataRecordAdapterFactory.Instance.CreateAdapter(record, typeof(T));
        }
    }
}
