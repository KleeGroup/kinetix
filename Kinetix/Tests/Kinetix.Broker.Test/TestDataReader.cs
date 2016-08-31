using System;
using System.Data;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// DataReader pour le résultat de commandes de test.
    /// </summary>
    public sealed class TestDataReader : IDataReader {
        private readonly int _rowCount;
        private int _currentRow = -1;
        private bool _isClosed = false;
        private int _recordsAffected = 0;

        /// <summary>
        /// Crée un nouveau reader.
        /// </summary>
        /// <param name="rowCount">Nombre de lignes à retourner.</param>
        internal TestDataReader(int rowCount) {
            this._rowCount = rowCount;
        }

        /// <summary>
        /// Crée un nouveau reader.
        /// </summary>
        /// <param name="rowCount">Nombre de lignes à retourner.</param>
        /// <param name="recordsAffected">Nombre de records affectés.</param>
        internal TestDataReader(int rowCount, int recordsAffected) {
            this._rowCount = rowCount;
            this._recordsAffected = recordsAffected;
        }

        /// <summary>
        /// Retourne le numéro de la ligne courante.
        /// </summary>
        public int Depth {
            get {
                return _currentRow;
            }
        }

        /// <summary>
        /// Retourne le nombre de colonnes.
        /// </summary>
        public int FieldCount {
            get {
                if (_rowCount == 9) {
                    return 3;
                } else {
                    return 2;
                }
            }
        }

        /// <summary>
        /// Indique si le reader est fermé.
        /// </summary>
        public bool IsClosed {
            get {
                return _isClosed;
            }
        }

        /// <summary>
        /// Retourne le nombre de lignes affectées par la commande.
        /// </summary>
        public int RecordsAffected {
            get {
                return _recordsAffected;
            }
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="name">Nom du champ.</param>
        /// <returns>Valeur (peut être nulle).</returns>
        public object this[string name] {
            get {
                if ("BEA_ID".Equals(name)) {
                    return _currentRow;
                } else if ("BEA_NAME".Equals(name)) {
                    return _currentRow.ToString();
                } else {
                    throw new NotSupportedException(name);
                }
            }
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="index">Indice du champ.</param>
        /// <returns>Valeur (peut être nulle).</returns>
        public object this[int index] {
            get {
                if (index == 0) {
                    return _currentRow;
                } else {
                    return _currentRow.ToString();
                }
            }
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="i">Indice du champ.</param>
        /// <returns>Valeur.</returns>
        object IDataRecord.this[int i] {
            get {
                if (i == 0) {
                    return _currentRow;
                } else if (i == 1) {
                    return _currentRow.ToString();
                } else if (i == 3) {
                    return DBNull.Value;
                } else {
                    throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// Ferme le reader.
        /// </summary>
        public void Close() {
            _isClosed = true;
        }

        /// <summary>
        /// Libère les ressources non managées.
        /// </summary>
        public void Dispose() {
        }

        /// <summary>
        /// Retourne une table correspondant aux enregistrements retournés.
        /// </summary>
        /// <returns>Table.</returns>
        DataTable IDataReader.GetSchemaTable() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne le prochain resultset.
        /// </summary>
        /// <returns>True si un nouveau resultset a été trouvé.</returns>
        bool IDataReader.NextResult() {
            return false;
        }

        /// <summary>
        /// Passe à l'enregistrement suivant.
        /// </summary>
        /// <returns>False si la fin du curseur est atteinte.</returns>
        public bool Read() {
            _currentRow++;
            return _currentRow < _rowCount;
        }

        /// <summary>
        /// Retourne une valeur boolean.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Boolean.</returns>
        bool IDataRecord.GetBoolean(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne une valeur Byte.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Byte.</returns>
        byte IDataRecord.GetByte(int i) {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lit un caractère..
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Caractère.</returns>
        char IDataRecord.GetChar(int i) {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne un reader correspondant à la colonne.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Reader.</returns>
        IDataReader IDataRecord.GetData(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne le type de données d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Libellé du type.</returns>
        string IDataRecord.GetDataTypeName(int i) {
            if (i == 1) {
                return "string";
            } else {
                return "int";
            }
        }

        /// <summary>
        /// Retourne une date.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Date.</returns>
        DateTime IDataRecord.GetDateTime(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne un décimal.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Décimal.</returns>
        decimal IDataRecord.GetDecimal(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne un double.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Double.</returns>
        double IDataRecord.GetDouble(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne le type de données d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Type de données.</returns>
        Type IDataRecord.GetFieldType(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne un float.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Float.</returns>
        float IDataRecord.GetFloat(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne un GUID.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>GUID.</returns>
        Guid IDataRecord.GetGuid(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne un short.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Short.</returns>
        short IDataRecord.GetInt16(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne un int.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Int.</returns>
        int IDataRecord.GetInt32(int i) {
            if (i == 0) {
                return _currentRow;
            } else {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Retourne un long.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Long.</returns>
        long IDataRecord.GetInt64(int i) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne le nom d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Nom du champ.</returns>
        string IDataRecord.GetName(int i) {
            if (i == 0) {
                return "BEA_ID";
            } else if (i == 1) {
                return "BEA_NAME";
            } else if (i == 2) {
                return "BEA_OTHER";
            } else {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Retourne l'indice d'un champ.
        /// </summary>
        /// <param name="name">Nom du champ.</param>
        /// <returns>Indice.</returns>
        public int GetOrdinal(string name) {
            if ("BEA_ID".Equals(name)) {
                return 0;
            } else if ("BEA_NAME".Equals(name)) {
                return 1;
            } else {
                throw new NotSupportedException(name);
            }
        }

        /// <summary>
        /// Retourne un string.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>String.</returns>
        string IDataRecord.GetString(int i) {
            if (i == 1 || i == 2) {
                return _currentRow.ToString();
            }
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Valeur ou null.</returns>
        object IDataRecord.GetValue(int i) {
            return this[i];
        }

        /// <summary>
        /// Ecrit les valeurs d'une ligne dans un tableau.
        /// </summary>
        /// <param name="values">Tableau de sortie.</param>
        /// <returns>Nombre de champs.</returns>
        int IDataRecord.GetValues(object[] values) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Indique si la valeur d'un champ est nulle.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>True si la valeur est nulle.</returns>
        bool IDataRecord.IsDBNull(int i) {
            return false;
        }
    }
}
