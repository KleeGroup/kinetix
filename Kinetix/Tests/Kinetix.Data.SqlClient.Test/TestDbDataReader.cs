using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using Kinetix.ComponentModel;

namespace Kinetix.Data.SqlClient.Test {
    /// <summary>
    /// DataReader pour le résultat de commandes de test.
    /// </summary>
    public sealed class TestDbDataReader : DbDataReader {
        private readonly IList _list;
        private readonly IList<BeanPropertyDescriptor> _descriptors;
        private readonly BeanPropertyDescriptorCollection _descriptorMap;
        private int _index = -1;

        /// <summary>
        /// Crée un nouveau reader.
        /// </summary>
        /// <param name="list">Liste interne.</param>
        internal TestDbDataReader(IList list) {
            if (list == null) {
                throw new ArgumentNullException("list");
            }
            _list = list;
            _descriptorMap = BeanDescriptor.GetCollectionDefinition(list).Properties;
            _descriptors = new List<BeanPropertyDescriptor>(_descriptorMap);
        }

        /// <summary>
        /// Indique si le reader a des lignes de données.
        /// </summary>
        public override bool HasRows {
            get {
                return _list.Count > 0;
            }
        }

        /// <summary>
        /// Retourne le numéro de la ligne courante.
        /// </summary>
        public override int Depth {
            get {
                return _index;
            }
        }

        /// <summary>
        /// Retourne le nombre de colonnes.
        /// </summary>
        public override int FieldCount {
            get {
                return _descriptors.Count;
            }
        }

        /// <summary>
        /// Indique si le reader est fermé.
        /// </summary>
        public override bool IsClosed {
            get {
                return false;
            }
        }

        /// <summary>
        /// Retourne le nombre de lignes affectées par la commande.
        /// </summary>
        public override int RecordsAffected {
            get {
                return _list.Count;
            }
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="i">Indice du champ.</param>
        /// <returns>Valeur.</returns>
        public override object this[int i] {
            get {
                object o = _descriptors[i].GetValue(_list[_index]);
                if (o != null) {
                    return o;
                } else {
                    return DBNull.Value;
                }
            }
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="name">Nom du champ.</param>
        /// <returns>Valeur (peut être nulle).</returns>
        public override object this[string name] {
            get {
                object o = _descriptorMap[name].GetValue(_list[_index]);
                if (o != null) {
                    return o;
                } else {
                    return DBNull.Value;
                }
            }
        }

        /// <summary>
        /// Ferme le reader.
        /// </summary>
        public override void Close() {
            return;
        }

        /// <summary>
        /// Retourne une table correspondant aux enregistrements retournés.
        /// </summary>
        /// <returns>Table.</returns>
        public override DataTable GetSchemaTable() {
            return null;
        }

        /// <summary>
        /// Retourne le prochain resultset.
        /// </summary>
        /// <returns>True si un nouveau resultset a été trouvé.</returns>
        public override bool NextResult() {
            return false;
        }

        /// <summary>
        /// Passe à l'enregistrement suivant.
        /// </summary>
        /// <returns>False si la fin du curseur est atteinte.</returns>
        public override bool Read() {
            if (_index < _list.Count - 1) {
                _index++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retourne une valeur boolean.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Boolean.</returns>
        public override bool GetBoolean(int i) {
            return (bool)this[i];
        }

        /// <summary>
        /// Retourne une valeur Byte.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Byte.</returns>
        public override byte GetByte(int i) {
            return (byte)this[i];
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
        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) {
            byte[] array = (byte[])this[i];
            return array.Length;
        }

        /// <summary>
        /// Lit un caractère..
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Caractère.</returns>
        public override char GetChar(int i) {
            return (char)this[i];
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
        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) {
            char[] array = (char[])this[i];
            return buffer.Length;
        }

        /// <summary>
        /// Retourne le type de données d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Libellé du type.</returns>
        public override string GetDataTypeName(int i) {
            return _descriptors[i].PropertyType.Name;
        }

        /// <summary>
        /// Retourne une date.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Date.</returns>
        public override DateTime GetDateTime(int i) {
            return (DateTime)this[i];
        }

        /// <summary>
        /// Retourne un décimal.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Décimal.</returns>
        public override decimal GetDecimal(int i) {
            return (decimal)this[i];
        }

        /// <summary>
        /// Retourne un double.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Double.</returns>
        public override double GetDouble(int i) {
            return (double)this[i];
        }

        /// <summary>
        /// Retourne le type de données d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Type de données.</returns>
        public override Type GetFieldType(int i) {
            return _descriptors[i].PropertyType;
        }

        /// <summary>
        /// Retourne un float.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Float.</returns>
        public override float GetFloat(int i) {
            return (float)this[i];
        }

        /// <summary>
        /// Retourne un GUID.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>GUID.</returns>
        public override Guid GetGuid(int i) {
            return (Guid)this[i];
        }

        /// <summary>
        /// Retourne un short.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Short.</returns>
        public override short GetInt16(int i) {
            return (short)this[i];
        }

        /// <summary>
        /// Retourne un int.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Int.</returns>
        public override int GetInt32(int i) {
            return (int)this[i];
        }

        /// <summary>
        /// Retourne un long.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Long.</returns>
        public override long GetInt64(int i) {
            return (long)this[i];
        }

        /// <summary>
        /// Retourne le nom d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Nom du champ.</returns>
        public override string GetName(int i) {
            BeanPropertyDescriptor desc = _descriptors[i];
            return String.IsNullOrEmpty(desc.MemberName) ?
                    desc.PropertyName :
                    desc.MemberName.ToLower(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Retourne l'indice d'un champ.
        /// </summary>
        /// <param name="name">Nom du champ.</param>
        /// <returns>Indice.</returns>
        public override int GetOrdinal(string name) {
            return _descriptors.IndexOf(_descriptorMap[name]);
        }

        /// <summary>
        /// Retourne un string.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>String.</returns>
        public override string GetString(int i) {
            return (string)this[i];
        }

        /// <summary>
        /// Retourne la valeur d'un champ.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>Valeur ou null.</returns>
        public override object GetValue(int i) {
            return this[i];
        }

        /// <summary>
        /// Ecrit les valeurs d'une ligne dans un tableau.
        /// </summary>
        /// <param name="values">Tableau de sortie.</param>
        /// <returns>Nombre de champs.</returns>
        public override int GetValues(object[] values) {
            _list.CopyTo(values, 0);
            return _list.Count;
        }

        /// <summary>
        /// Indique si la valeur d'un champ est nulle.
        /// </summary>
        /// <param name="i">Indice.</param>
        /// <returns>True si la valeur est nulle.</returns>
        public override bool IsDBNull(int i) {
            return DBNull.Value.Equals(this[i]);
        }

        /// <summary>
        /// Retourne un énumérateur.
        /// </summary>
        /// <returns>Enumérateur.</returns>
        public override IEnumerator GetEnumerator() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne le reader associé à un champ.
        /// </summary>
        /// <param name="ordinal">Position.</param>
        /// <returns>Reader.</returns>
        protected override DbDataReader GetDbDataReader(int ordinal) {
            return this;
        }
    }
}
