using System;
using System.Data;

namespace Kinetix.Data.SqlClient {

    /// <summary>
    /// DataReader pour le résultat de commandes Sql Server.
    /// </summary>
    public sealed class SqlServerDataReader : IDataReader {

        private IDataReader _innerDataReader;
        private SqlServerConnection _connection;
        private QueryParameter _queryParameter;

        private long _readIndex = 0;
        private int _readOffset = 0;
        private int _readLimit = 0;

        /// <summary>
        /// Crée un nouveau reader.
        /// </summary>
        /// <param name="dataReader">Reader interne.</param>
        /// <param name="connection">Connexion à fermer si non nulle.</param>
        /// <param name="queryParameter">Paramètres de requêtage.</param>
        internal SqlServerDataReader(IDataReader dataReader, SqlServerConnection connection, QueryParameter queryParameter) {
            _connection = connection;
            _innerDataReader = dataReader;
            _queryParameter = queryParameter;

            if (_queryParameter != null) {
                _readOffset = _queryParameter.Offset;
                _readLimit = _queryParameter.Limit;
            }
        }

        /// <summary>
        /// Destructor.
        /// </summary>
        ~SqlServerDataReader() {
            Dispose(false);
        }

        /// <summary>
        /// Retourne le numéro de la ligne courante.
        /// </summary>
        public int Depth {
            get {
                return _innerDataReader.Depth;
            }
        }

        /// <summary>
        /// Retourne le nombre de colonnes.
        /// </summary>
        public int FieldCount {
            get {
                return _innerDataReader.FieldCount;
            }
        }

        /// <summary>
        /// Indique si le reader est fermé.
        /// </summary>
        public bool IsClosed {
            get {
                return _innerDataReader.IsClosed;
            }
        }

        /// <summary>
        /// Retourne le nombre de lignes affectées par la commande.
        /// </summary>
        public int RecordsAffected {
            get {
                return _innerDataReader.RecordsAffected;
            }
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="name">Nom du champ.</param>
        /// <returns>Valeur (peut être nulle).</returns>
        public object this[string name] {
            get {
                object val = _innerDataReader[name];
                return DBNull.Value.Equals(val) ? null : val;
            }
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="index">Indice du champ.</param>
        /// <returns>Valeur (peut être nulle).</returns>
        public object this[int index] {
            get {
                object val = _innerDataReader[index];
                return DBNull.Value.Equals(val) ? null : val;
            }
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="i">Indice du champ.</param>
        /// <returns>Valeur.</returns>
        object IDataRecord.this[int i] {
            get {
                return _innerDataReader[i];
            }
        }

        /// <summary>
        /// Ferme le reader.
        /// </summary>
        public void Close() {
            _innerDataReader.Close();
        }

        /// <summary>
        /// Libère les ressources non managées.
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Retourne une table correspondant aux enregistrements retournés.
        /// </summary>
        /// <returns>Table.</returns>
        DataTable IDataReader.GetSchemaTable() {
            return _innerDataReader.GetSchemaTable();
        }

        /// <summary>
        /// Retourne le prochain resultset.
        /// </summary>
        /// <returns>True si un nouveau resultset a été trouvé.</returns>
        bool IDataReader.NextResult() {
            return _innerDataReader.NextResult();
        }

        /// <summary>
        /// Passe à l'enregistrement suivant.
        /// </summary>
        /// <returns>False si la fin du curseur est atteinte.</returns>
        public bool Read() {
            if (_queryParameter != null) {
                // Décalage jusqu'à l'offset.
                if (_readOffset > 0) {
                    while (_readIndex < _readOffset) {
                        if (_innerDataReader.Read()) {
                            ++_readIndex;
                        } else {
                            QueryContext.InlineCount = _readIndex;
                            return false;
                        }
                    }
                }

                // Gestion de la limite.
                if (_readLimit > 0) {
                    if (_readIndex >= _readOffset + _readLimit) {
                        while (_innerDataReader.Read()) {
                            ++_readIndex;
                        }

                        QueryContext.InlineCount = _readIndex;
                        return false;
                    }
                }

                // Lecture de la données.
                bool hasData = _innerDataReader.Read();
                if (hasData) {
                    ++_readIndex;
                    return true;
                } else {
                    QueryContext.InlineCount = _readIndex;
                    return false;
                }
            }

            return _innerDataReader.Read();
        }

        /// <summary>
        /// Retourne une valeur boolean.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Boolean ou null.</returns>
        public bool? GetBoolean(int index) {
            return IsDBNull(index) ? null : (bool?)this.ReadBoolean(index);
        }

        /// <summary>
        /// Retourne une valeur boolean.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Boolean.</returns>
        bool IDataRecord.GetBoolean(int i) {
            return this.ReadBoolean(i);
        }

        /// <summary>
        /// Retourne une valeur Byte.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Byte ou null.</returns>
        public byte? GetByte(int index) {
            return IsDBNull(index) ? null : (byte?)_innerDataReader.GetByte(index);
        }

        /// <summary>
        /// Retourne une valeur Byte.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Byte.</returns>
        byte IDataRecord.GetByte(int i) {
            return _innerDataReader.GetByte(i);
        }

        /// <summary>
        /// Lit n octets et les écrit dans un tableau.
        /// </summary>
        /// <param name="index">Indice de la colonne à lire.</param>
        /// <param name="fieldOffset">Offset de début de lecture.</param>
        /// <param name="buffer">Buffer de sortie.</param>
        /// <param name="bufferoffset">Offset de début d'écriture.</param>
        /// <param name="length">Nombre d'octets à écrire.</param>
        /// <returns>Nombre d'octets lus.</returns>
        public long GetBytes(int index, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
            return _innerDataReader.GetBytes(index, fieldOffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Lit n octets et les écrit dans un tableau.
        /// </summary>
        /// <param name="i">Indice de la colonne à lire.</param>
        /// <param name="fieldOffset">Offset de début de lecture.</param>
        /// <param name="buffer">Buffer de sortie.</param>
        /// <param name="bufferoffset">Offset de début d'écriture.</param>
        /// <param name="length">Nombre d'octets à écrire.</param>
        /// <returns>Nombre d'octets lus.</returns>
        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
            return this.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Lit un caractère..
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Caractère.</returns>
        char IDataRecord.GetChar(int i) {
            return ((IDataRecord)_innerDataReader).GetChar(i);
        }

        /// <summary>
        /// Lit n caractères et les écrit dans un tableau.
        /// </summary>
        /// <param name="index">Indice de la colonne à lire.</param>
        /// <param name="fieldoffset">Offset de début de lecture.</param>
        /// <param name="buffer">Buffer de sortie.</param>
        /// <param name="bufferoffset">Offset de début d'écriture.</param>
        /// <param name="length">Nombre de caractères à écrire.</param>
        /// <returns>Nombre de caractères lus.</returns>
        public long GetChars(int index, long fieldoffset, char[] buffer, int bufferoffset, int length) {
            return _innerDataReader.GetChars(index, fieldoffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Lit n caractères et les écrit dans un tableau.
        /// </summary>
        /// <param name="i">Indice de la colonne à lire.</param>
        /// <param name="fieldoffset">Offset de début de lecture.</param>
        /// <param name="buffer">Buffer de sortie.</param>
        /// <param name="bufferoffset">Offset de début d'écriture.</param>
        /// <param name="length">Nombre de caractères à écrire.</param>
        /// <returns>Nombre de caractères lus.</returns>
        long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
            return this.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Retourne un reader correspondant à la colonne.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Reader.</returns>
        IDataReader IDataRecord.GetData(int i) {
            return ((IDataRecord)_innerDataReader).GetData(i);
        }

        /// <summary>
        /// Retourne le type de données d'un champ.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Libellé du type.</returns>
        public string GetDataTypeName(int index) {
            return _innerDataReader.GetDataTypeName(index);
        }

        /// <summary>
        /// Retourne le type de données d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Libellé du type.</returns>
        string IDataRecord.GetDataTypeName(int i) {
            return this.GetDataTypeName(i);
        }

        /// <summary>
        /// Retourne une date.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Date ou null.</returns>
        public DateTime? GetDateTime(int index) {
            return IsDBNull(index) ? null : (DateTime?)_innerDataReader.GetDateTime(index);
        }

        /// <summary>
        /// Retourne une date.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Date.</returns>
        DateTime IDataRecord.GetDateTime(int i) {
            return _innerDataReader.GetDateTime(i);
        }

        /// <summary>
        /// Retourne un décimal.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Décimal ou null.</returns>
        public decimal? GetDecimal(int index) {
            return IsDBNull(index) ? null : (decimal?)_innerDataReader.GetDecimal(index);
        }

        /// <summary>
        /// Retourne un décimal.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Décimal.</returns>
        decimal IDataRecord.GetDecimal(int i) {
            return _innerDataReader.GetDecimal(i);
        }

        /// <summary>
        /// Retourne un double.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Double ou null.</returns>
        public double? GetDouble(int index) {
            return IsDBNull(index) ? null : (double?)_innerDataReader.GetDouble(index);
        }

        /// <summary>
        /// Retourne un double.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Double.</returns>
        double IDataRecord.GetDouble(int i) {
            return _innerDataReader.GetDouble(i);
        }

        /// <summary>
        /// Retourne le type de données d'un champ.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Type de données.</returns>
        public Type GetFieldType(int index) {
            return _innerDataReader.GetFieldType(index);
        }

        /// <summary>
        /// Retourne le type de données d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Type de données.</returns>
        Type IDataRecord.GetFieldType(int i) {
            return _innerDataReader.GetFieldType(i);
        }

        /// <summary>
        /// Retourne un float.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Float ou null.</returns>
        public float? GetFloat(int index) {
            return IsDBNull(index) ? null : (float?)_innerDataReader.GetFloat(index);
        }

        /// <summary>
        /// Retourne un float.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Float.</returns>
        float IDataRecord.GetFloat(int i) {
            return _innerDataReader.GetFloat(i);
        }

        /// <summary>
        /// Retourne un GUID.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>GUID.</returns>
        Guid IDataRecord.GetGuid(int i) {
            return _innerDataReader.GetGuid(i);
        }

        /// <summary>
        /// Retourne un short.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Short ou null.</returns>
        public short? GetInt16(int index) {
            return IsDBNull(index) ? null : (short?)_innerDataReader.GetInt16(index);
        }

        /// <summary>
        /// Retourne un short.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Short.</returns>
        short IDataRecord.GetInt16(int i) {
            return _innerDataReader.GetInt16(i);
        }

        /// <summary>
        /// Retourne un int.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Int ou null.</returns>
        public int? GetInt32(int index) {
            return IsDBNull(index) ? null : (int?)_innerDataReader.GetInt32(index);
        }

        /// <summary>
        /// Retourne un int.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Int.</returns>
        int IDataRecord.GetInt32(int i) {
            return _innerDataReader.GetInt32(i);
        }

        /// <summary>
        /// Retourne un long.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Long ou null.</returns>
        public long? GetInt64(int index) {
            return IsDBNull(index) ? null : (long?)_innerDataReader.GetInt64(index);
        }

        /// <summary>
        /// Retourne un long.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Long.</returns>
        long IDataRecord.GetInt64(int i) {
            return _innerDataReader.GetInt64(i);
        }

        /// <summary>
        /// Retourne le nom d'un champ.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Nom du champ.</returns>
        public string GetName(int index) {
            return _innerDataReader.GetName(index);
        }

        /// <summary>
        /// Retourne le nom d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Nom du champ.</returns>
        string IDataRecord.GetName(int i) {
            return _innerDataReader.GetName(i);
        }

        /// <summary>
        /// Retourne l'indice d'un champ.
        /// </summary>
        /// <param name="name">Nom du champ.</param>
        /// <returns>Indice.</returns>
        public int GetOrdinal(string name) {
            return _innerDataReader.GetOrdinal(name);
        }

        /// <summary>
        /// Retourne un string.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>String ou null.</returns>
        public string GetString(int index) {
            return IsDBNull(index) ? null : _innerDataReader.GetString(index);
        }

        /// <summary>
        /// Retourne un string.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>String.</returns>
        string IDataRecord.GetString(int i) {
            return _innerDataReader.GetString(i);
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>Valeur ou null.</returns>
        public object GetValue(int index) {
            return IsDBNull(index) ? null : _innerDataReader.GetValue(index);
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Valeur ou null.</returns>
        object IDataRecord.GetValue(int i) {
            return _innerDataReader.GetValue(i);
        }

        /// <summary>
        /// Ecrit les valeurs d'une ligne dans un tableau.
        /// </summary>
        /// <param name="values">Tableau de sortie.</param>
        /// <returns>Nombre de champs.</returns>
        int IDataRecord.GetValues(object[] values) {
            return _innerDataReader.GetValues(values);
        }

        /// <summary>
        /// Indique si la valeur d'un champ est nulle.
        /// </summary>
        /// <param name="index">Indice.</param>
        /// <returns>True si la valeur est nulle.</returns>
        public bool IsDBNull(int index) {
            return _innerDataReader.IsDBNull(index);
        }

        /// <summary>
        /// Indique si la valeur d'un champ est nulle.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>True si la valeur est nulle.</returns>
        bool IDataRecord.IsDBNull(int i) {
            return _innerDataReader.IsDBNull(i);
        }

        /// <summary>
        /// Implementation of IDisposable.
        /// </summary>
        /// <param name="disposing">False will only dispose unmanaged resources.</param>
        private void Dispose(bool disposing) {
            if (disposing) {
                // dispose managed resources.
            }

            // dispose unmanaged resources.
            if (_innerDataReader != null) {
                _innerDataReader.Dispose();
                _innerDataReader = null;
            }

            if (_connection != null) {
                _connection.Close();
            }
        }

        /// <summary>
        /// Retourne une valeur boolean.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Boolean.</returns>
        private bool ReadBoolean(int i) {
            object value = _innerDataReader.GetValue(i);
            if (value is int) {
                return (int)value != 0;
            }

            return _innerDataReader.GetBoolean(i);
        }
    }
}
