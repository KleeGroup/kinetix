using System;
using System.Data;

namespace Kinetix.Data.SqlClient {

    /// <summary>
    /// Base des adapteurs.
    /// </summary>
    public abstract class AbstractDataReaderAdapter {

        /// <summary>
        /// Retourne un Boolean.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Boolean.</returns>
        protected static bool? ReadBoolean(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetBoolean(idx);
        }

        /// <summary>
        /// Retourne un Boolean.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Boolean.</returns>
        protected static bool ReadNonNullableBoolean(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                throw new ArgumentNullException("record");
            }

            return record.GetBoolean(idx);
        }

        /// <summary>
        /// Retourne un Byte.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Byte.</returns>
        protected static byte? ReadByte(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetByte(idx);
        }

        /// <summary>
        /// Retourne un DateTime.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>DateTime.</returns>
        protected static DateTime? ReadDateTime(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetDateTime(idx);
        }


        /// <summary>
        /// Retourne un DateTime non null.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>DateTime.</returns>
        protected static DateTime ReadNonNullableDateTime(IDataRecord record, int idx)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx))
            {
                throw new ArgumentNullException("record");
            }

            return record.GetDateTime(idx);
        }

        /// <summary>
        /// Retourne un TimeSpan.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>TimeSpan.</returns>
        protected static TimeSpan? ReadTimeSpan(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return (TimeSpan?)record.GetValue(idx);
        }

        /// <summary>
        /// Retourne un TimeSpan.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>TimeSpan.</returns>
        protected static TimeSpan ReadNonNullableTimeSpan(IDataRecord record, int idx)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx))
            {
                throw new ArgumentNullException("record");
            }

            return (TimeSpan)record.GetValue(idx);
        }

        /// <summary>
        /// Retourne un decimal.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Decimal.</returns>
        protected static decimal? ReadDecimal(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetDecimal(idx);
        }

        /// <summary>
        /// Retourne un int32.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Entier.</returns>
        protected static int? ReadInt(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetInt32(idx);
        }


        /// <summary>
        /// Retourne un int32 non null.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Entier.</returns>
        protected static int ReadNonNullableInt(IDataRecord record, int idx)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx))
            {
                throw new ArgumentNullException("record");
            }

            return record.GetInt32(idx);
        }

        /// <summary>
        /// Retourne un short.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Short.</returns>
        protected static short? ReadShort(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetInt16(idx);
        }

        /// <summary>
        /// Retourne un long.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Long.</returns>
        protected static long? ReadLong(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetInt64(idx);
        }

        /// <summary>
        /// Retourne un float.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Float.</returns>
        protected static float? ReadFloat(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetFloat(idx);
        }

        /// <summary>
        /// Retourne un double.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Double.</returns>
        protected static double? ReadDouble(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetDouble(idx);
        }

        /// <summary>
        /// Retourne un string.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>String.</returns>
        protected static string ReadString(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetString(idx);
        }

        /// <summary>
        /// Retourne un char.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Char.</returns>
        protected static char? ReadChar(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetChar(idx);
        }

        /// <summary>
        /// Retourne un char.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Tableau de char..</returns>
        protected static char[] ReadCharArray(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return (char[])record.GetValue(idx);
        }

        /// <summary>
        /// Retourne un guid.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Guid.</returns>
        protected static Guid? ReadGuid(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return record.GetGuid(idx);
        }

        /// <summary>
        /// Retourne un byte[].
        /// </summary>
        /// <param name="record">Record.</param>
        /// <param name="idx">Index.</param>
        /// <returns>Byte[].</returns>
        protected static byte[] ReadByteArray(IDataRecord record, int idx) {
            if (record == null) {
                throw new ArgumentNullException("record");
            }

            if (record.IsDBNull(idx)) {
                return null;
            }

            return (byte[])record.GetValue(idx);
        }
    }
}
