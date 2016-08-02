using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using Kinetix.ComponentModel;
using Microsoft.SqlServer.Server;

namespace Kinetix.Data.SqlClient {
    /// <summary>
    /// Collection de paramètres pour les commandes Sql Server.
    /// </summary>
    public sealed class SqlServerParameterCollection : IDataParameterCollection, IList<SqlServerParameter> {

        /// <summary>
        /// Mot-clés de définition des paramètres dans les
        /// requêtes SQL Server.
        /// </summary>
        internal const string ParamValue = "@";

        /// <summary>
        /// Nom du type SQL Server dédié aux int.
        /// </summary>
        private const string IntDataType = "type_int_list";

        /// <summary>
        /// Nom du type SQL Server dédié aux varchar.
        /// </summary>
        private const string VarCharDataType = "type_varchar_list";

        /// <summary>
        /// Taille du champ du type SQL Server dédié aux varchar.
        /// </summary>
        private const int VarCharLength = 10;

        /// <summary>
        /// Nom de la colonne dans le type table.
        /// </summary>
        private const string ColDataTypeName = "n";

        /// <summary>
        /// Map contenant les mots exclus de la recherche full texte.
        /// </summary>
        private static Dictionary<string, string> _noiseWordsMap;

        private readonly IDataParameterCollection _innerCollection;
        private readonly IDbCommand _innerCommand;
        private readonly List<SqlServerParameter> _list;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="command">Commande.</param>
        internal SqlServerParameterCollection(IDbCommand command) {
            _innerCommand = command;
            _innerCollection = command.Parameters;
            _list = new List<SqlServerParameter>();
        }

        /// <summary>
        /// Retourne le nombre d'éléments de la collection.
        /// </summary>
        public int Count {
            get {
                return _list.Count;
            }
        }

        /// <summary>
        /// Indique si la collection a une taille fixe.
        /// </summary>
        bool IList.IsFixedSize {
            get {
                return _innerCollection.IsFixedSize;
            }
        }

        /// <summary>
        /// Indique si la collection est en lecture seule.
        /// </summary>
        bool IList.IsReadOnly {
            get {
                return _innerCollection.IsReadOnly;
            }
        }

        /// <summary>
        /// Indique si la collection est en lecture seule.
        /// </summary>
        bool ICollection<SqlServerParameter>.IsReadOnly {
            get {
                return _innerCollection.IsReadOnly;
            }
        }

        /// <summary>
        /// Indique si la collection est synchronisée.
        /// </summary>
        bool ICollection.IsSynchronized {
            get {
                return _innerCollection.IsSynchronized;
            }
        }

        /// <summary>
        /// Retourne le point de synchronisation pour la collection.
        /// </summary>
        object ICollection.SyncRoot {
            get {
                return _innerCollection.SyncRoot;
            }
        }

        /// <summary>
        /// Map contenant les mots exclus de la recherche full texte.
        /// </summary>
        private static Dictionary<string, string> NoiseWords {
            get {
                if (_noiseWordsMap == null) {
                    Dictionary<string, string> noiseWords = new Dictionary<string, string> {
                                                                    { "A", null },
                                                                    { "B", null },
                                                                    { "C", null },
                                                                    { "D", null },
                                                                    { "E", null },
                                                                    { "F", null },
                                                                    { "G", null },
                                                                    { "H", null },
                                                                    { "I", null },
                                                                    { "J", null },
                                                                    { "K", null },
                                                                    { "L", null },
                                                                    { "M", null },
                                                                    { "N", null },
                                                                    { "O", null },
                                                                    { "P", null },
                                                                    { "Q", null },
                                                                    { "R", null },
                                                                    { "S", null },
                                                                    { "T", null },
                                                                    { "U", null },
                                                                    { "V", null },
                                                                    { "W", null },
                                                                    { "X", null },
                                                                    { "Y", null },
                                                                    { "Z", null },
                                                                    { "LA", null },
                                                                    { "LE", null },
                                                                    { "L'", null },
                                                                    { "LES", null },
                                                                    { "UN", null },
                                                                    { "UNE", null },
                                                                    { "DE", null },
                                                                    { "DES", null },
                                                                    { "DU", null },
                                                                    { "AU", null },
                                                                    { "AUX", null },
                                                                    { "À", null }
                                                                };
                    _noiseWordsMap = noiseWords;
                }

                return _noiseWordsMap;
            }
        }

        /// <summary>
        /// Obtient ou définit un paramètre de la collection.
        /// </summary>
        /// <param name="index">Numéro du paramètre.</param>
        /// <returns>Paramètre.</returns>
        public SqlServerParameter this[int index] {
            get {
                return _list[index];
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                _list[index] = value;
                _innerCollection[index] = value.InnerParameter;
            }
        }

        /// <summary>
        /// Obtient ou définit un paramètre de la collection.
        /// </summary>
        /// <param name="index">Numéro du paramètre.</param>
        /// <returns>Paramètre.</returns>
        object IList.this[int index] {
            get {
                return this[index];
            }

            set {
                this[index] = (SqlServerParameter)value;
            }
        }

        /// <summary>
        /// Obtient ou définit un paramètre de la collection.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <returns>Paramètre.</returns>
        public SqlServerParameter this[string parameterName] {
            get {
                int index = IndexOf(parameterName);
                return _list[index];
            }

            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                int index = IndexOf(parameterName);
                _list[index] = value;
                _innerCollection[index] = value.InnerParameter;
            }
        }

        /// <summary>
        /// Obtient ou définit un paramètre de la collection.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <returns>Paramètre.</returns>
        object IDataParameterCollection.this[string parameterName] {
            get {
                return this[parameterName];
            }

            set {
                this[parameterName] = (SqlServerParameter)value;
            }
        }

        /// <summary>
        /// Ajoute un paramètre la collection.
        /// </summary>
        /// <param name="parameter">Nouveau paramètre.</param>
        /// <returns>Paramètre ajouté.</returns>
        public SqlServerParameter Add(SqlServerParameter parameter) {
            if (parameter == null) {
                throw new ArgumentNullException("parameter");
            }

            _list.Add(parameter);
            _innerCollection.Add(parameter.InnerParameter);
            return parameter;
        }

        /// <summary>
        /// Ajoute un paramètre la collection.
        /// </summary>
        /// <param name="value">Nouveau paramètre.</param>
        /// <returns>Indice d'ajout.</returns>
        int IList.Add(object value) {
            this.Add((SqlServerParameter)value);
            return _list.Count - 1;
        }

        /// <summary>
        /// Ajoute un paramètre la collection.
        /// </summary>
        /// <param name="parameter">Nouveau paramètre.</param>
        void ICollection<SqlServerParameter>.Add(SqlServerParameter parameter) {
            this.Add(parameter);
        }

        /// <summary>
        /// Ajouter un paramètre structuré.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre SQL Server.</param>
        /// <param name="list">Collection des entiers.</param>
        /// <param name="typeName">Nom défini par type de table.</param>
        /// <returns>Le paramètre créé.</returns>
        /// <typeparam name="T">Type interne de la collection.</typeparam>
        public SqlServerParameter AddStructuredParameter<T>(string parameterName, ICollection<T> list, string typeName)
            where T : new() {
            return AddStructuredParameter<T>(parameterName, list, typeName, null);
        }

        /// <summary>
        /// Ajouter un paramètre structuré.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre SQL Server.</param>
        /// <param name="list">Collection des entiers.</param>
        /// <param name="typeName">Nom défini par type de table.</param>
        /// <param name="columnsAvailable">Définir des colonnes disponibles dans le paramètre table.</param>
        /// <returns>Le paramètre créé.</returns>
        /// <typeparam name="T">Type interne de la collection.</typeparam>
        public SqlServerParameter AddStructuredParameter<T>(string parameterName, ICollection<T> list, string typeName, string[] columnsAvailable)
            where T : new() {
            if (string.IsNullOrEmpty(parameterName)) {
                throw new ArgumentNullException("parameterName");
            }

            if (list == null) {
                throw new ArgumentNullException("list");
            }

            if (typeName == null) {
                throw new ArgumentNullException("typeName");
            }

            SqlServerParameter parameter = new SqlServerParameter(_innerCommand.CreateParameter()) {
                ParameterName = parameterName,
                SqlDbType = SqlDbType.Structured,
                Direction = ParameterDirection.Input,
                TypeName = typeName,
                Value = ConvertTo<T>(list, columnsAvailable)
            };
            this.Add(parameter);

            return parameter;
        }

        /// <summary>
        /// Ajoute les paramètres pour une clause IN portant sur des entiers.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre SQL Server.</param>
        /// <param name="list">Collection des entiers à insérer dans le IN.</param>
        /// <returns>Le paramètre créé.</returns>
        /// <remarks>Dans la requête, le corps du IN doit s'écrire de la manière suivante : n in (select * from @parameterName).</remarks>
        public SqlServerParameter AddInParameter(string parameterName, IEnumerable<int> list) {
            return AddInParameter(parameterName, (IEnumerable)list, IntDataType, SqlDbType.Int);
        }

        /// <summary>
        /// Ajoute les paramètres pour une clause IN portant sur des chaines de caractères.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre SQL Server.</param>
        /// <param name="list">Collection des entiers à insérer dans le IN.</param>
        /// <returns>Le paramètre créé.</returns>
        /// <remarks>Dans la requête, le corps du IN doit s'écrire de la manière suivante : n in (select * from @parameterName).</remarks>
        public SqlServerParameter AddInParameter(string parameterName, IEnumerable<string> list) {
            return AddInParameter(parameterName, (IEnumerable)list, VarCharDataType, SqlDbType.VarChar);
        }

        /// <summary>
        /// Ajout un nouveau paramètre à partir de son nom et de sa valeur.
        /// Le paramètre est un paramètre d'entrée.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <param name="value">Valeur du paramètre.</param>
        /// <returns>Paramètre.</returns>
        public SqlServerParameter AddWithValue(string parameterName, object value) {
            IDbDataParameter param = _innerCommand.CreateParameter();
            param.ParameterName = ParamValue + parameterName;

            if (value == null) {
                param.Value = DBNull.Value;
            } else {
                Type t = value.GetType();

                if (t == typeof(string)) {
                    param.DbType = DbType.String;
                } else if (t == typeof(byte)) {
                    param.DbType = DbType.Byte;
                } else if (t == typeof(short)) {
                    param.DbType = DbType.Int16;
                } else if (t == typeof(int)) {
                    param.DbType = DbType.Int32;
                } else if (t == typeof(long)) {
                    param.DbType = DbType.Int64;
                } else if (t == typeof(decimal)) {
                    param.DbType = DbType.Decimal;
                } else if (t == typeof(float)) {
                    param.DbType = DbType.Single;
                } else if (t == typeof(double)) {
                    param.DbType = DbType.Double;
                } else if (t == typeof(Guid)) {
                    param.DbType = DbType.Guid;
                } else if (t == typeof(bool)) {
                    param.DbType = DbType.Boolean;
                } else if (t == typeof(byte[])) {
                    param.DbType = DbType.Binary;
                } else if (t == typeof(DateTime)) {
                    param.DbType = DbType.DateTime2;
                } else if (t == typeof(System.Data.SqlTypes.SqlDateTime)) {
                    param.DbType = DbType.DateTime2;
                } else if (t == typeof(System.TimeSpan)) {
                    ((SqlParameter)param).SqlDbType = SqlDbType.Time;
                } else if (t == typeof(ChangeAction)) {
                    param.DbType = DbType.String;
                } else if (t == typeof(char)) {
                    param.DbType = DbType.String;
                } else {
                    throw new NotImplementedException("La gestion du type " + t.Name + " doit être implémentée.");
                }

                param.Value = value;
            }

            _innerCollection.Add(param);

            SqlServerParameter p = new SqlServerParameter(param);
            _list.Add(p);
            return p;
        }

        /// <summary>
        /// Ajout un nouveau paramètre à partir d'une colonne et de sa valeur.
        /// Le paramètre est un paramètre d'entrée.
        /// </summary>
        /// <param name="colName">Colonnne du paramètre.</param>
        /// <param name="value">Valeur du paramètre.</param>
        /// <returns>Paramètre.</returns>
        public SqlServerParameter AddWithValue(Enum colName, object value) {
            if (colName == null) {
                throw new ArgumentNullException("colName");
            }

            return AddWithValue(colName.ToString(), value);
        }

        /// <summary>
        /// Ajoute comme nouveaux paramètres toutes les propriétés publiques primitives du bean considéré.
        /// Le nom du paramètre correspond au nom de la propriété.
        /// </summary>
        /// <param name="bean">Le bean en question.</param>
        /// <returns>La liste des paramètres ajoutés.</returns>
        /// <exception cref="System.NotSupportedException">Si le bean contient une propriété non primitive.</exception>
        public ICollection<SqlServerParameter> AddBeanProperties(object bean) {
            if (bean == null) {
                throw new ArgumentNullException("bean");
            }

            ICollection<SqlServerParameter> parameterList = new List<SqlServerParameter>();
            BeanDefinition beanDefinition = BeanDescriptor.GetDefinition(bean);
            foreach (BeanPropertyDescriptor property in
                beanDefinition.Properties.Where(property => property.PropertyType != typeof(ChangeAction))) {
                if (property.PrimitiveType == null) {
                    continue;
                }

                if (property.PrimitiveType.Name == "ICollection`1") {
                    continue;
                }

                string parameterName = property.MemberName ?? property.PropertyName;
                SqlServerParameter parameter = AddWithValue(parameterName, property.GetValue(bean, true));
                parameterList.Add(parameter);
            }

            return parameterList;
        }

        /// <summary>
        /// Ajoute une liste de bean en paramètre (La colonne InsertKey est obligatoire).
        /// </summary>
        /// <typeparam name="T">Type du bean.</typeparam>
        /// <param name="collection">Collection à passer en paramètre.</param>
        /// <returns>Parameter.</returns>
        public SqlServerParameter AddBeanCollectionProperties<T>(ICollection<T> collection)
            where T : class, new() {
            SqlServerParameter parameter = new SqlServerParameterBeanCollection<T>(collection, false).CreateParameter(_innerCommand);
            _list.Add(parameter);
            return parameter;
        }

        /// <summary>
        /// Ajoute un nouveau paramètre à partir de son nom et de sa valeur,
        /// et null si la condition n'est pas rempli. Le paramètre est un paramètre
        /// d'entrée.
        /// </summary>
        /// <param name="condition">Condition de non-nullité.</param>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <param name="valueIfTrue">Valeur du paramètre si vrai.</param>
        /// <param name="valueIfFalse">Valeur du paramètre si faux.</param>
        /// <returns>Paramètre.</returns>
        public SqlServerParameter AddWithValue(bool condition, string parameterName, object valueIfTrue, object valueIfFalse) {
            return this.AddWithValue(parameterName, condition ? valueIfTrue : valueIfFalse);
        }

        /// <summary>
        /// Ajout un nouveau paramètre en tant que critère de rechercher fulltext à partir de son nom et de sa valeur.
        /// Le paramètre est un paramètre d'entrée.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <param name="value">Valeur du paramètre.</param>
        /// <returns>Paramètre.</returns>
        public SqlServerParameter AddFullTextWithValue(string parameterName, string value) {
            StringBuilder criteria = new StringBuilder();
            if (value != null) {
                string[] words = value.Trim().Split(' ');
                foreach (string word in words.Where(word => !NoiseWords.ContainsKey(word.ToUpperInvariant()))) {
                    if (criteria.Length != 0) {
                        criteria.Append(" AND ");
                    }

                    criteria.Append(string.Format(CultureInfo.CurrentCulture, "(FORMSOF(INFLECTIONAL, \"{0}\") OR FORMSOF(THESAURUS, \"{0}\"))", RemoveIllegalChar(word)));
                }
            }

            return this.AddWithValue(parameterName, (value == null || string.IsNullOrEmpty(criteria.ToString())) ? null : criteria.ToString());
        }

        /// <summary>
        /// Efface tous paramètres.
        /// </summary>
        public void Clear() {
            _list.Clear();
            _innerCollection.Clear();
        }

        /// <summary>
        /// Indique si la collection contient un paramètre.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <returns>True si la collection contient le paramètre.</returns>
        public bool Contains(string parameterName) {
            return _innerCollection.Contains(parameterName);
        }

        /// <summary>
        /// Indique si la collection contient un paramètre.
        /// </summary>
        /// <param name="item">Paramètre.</param>
        /// <returns>True si la collection contient le paramètre.</returns>
        public bool Contains(SqlServerParameter item) {
            return _list.Contains(item);
        }

        /// <summary>
        /// Indique si la collection contient un paramètre.
        /// </summary>
        /// <param name="value">Paramètre.</param>
        /// <returns>True si la collection contient le paramètre.</returns>
        bool IList.Contains(object value) {
            return _list.Contains((SqlServerParameter)value);
        }

        /// <summary>
        /// Copie la collection dans un tableau d'objet.
        /// Cette méthode n'est pas supportée.
        /// </summary>
        /// <param name="array">Tableau de sortie.</param>
        /// <param name="arrayIndex">Index de début de copie.</param>
        public void CopyTo(SqlServerParameter[] array, int arrayIndex) {
            _list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Copie la collection dans un tableau d'objet.
        /// Cette méthode n'est pas supportée.
        /// </summary>
        /// <param name="array">Tableau de sortie.</param>
        /// <param name="index">Index de début de copie.</param>
        void ICollection.CopyTo(Array array, int index) {
            ((IList)_list).CopyTo(array, index);
        }

        /// <summary>
        /// Retourne un enumérateur sur la collection.
        /// </summary>
        /// <returns>Enumerateur.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Retourne un enumérateur sur la collection.
        /// </summary>
        /// <returns>Enumerateur.</returns>
        IEnumerator<SqlServerParameter> IEnumerable<SqlServerParameter>.GetEnumerator() {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Retourne la position d'un paramètre dans la collection.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        /// <returns>Postion du paramètre ou -1 si il est absent de la collection.</returns>
        public int IndexOf(string parameterName) {
            return _innerCollection.IndexOf(parameterName);
        }

        /// <summary>
        /// Retourne la position d'un paramètre dans la collection.
        /// </summary>
        /// <param name="item">Paramètre.</param>
        /// <returns>Postion du paramètre ou -1 si il est absent de la collection.</returns>
        public int IndexOf(SqlServerParameter item) {
            return _list.IndexOf(item);
        }

        /// <summary>
        /// Retourne la position d'un paramètre dans la collection.
        /// </summary>
        /// <param name="value">Paramètre.</param>
        /// <returns>Postion du paramètre ou -1 si il est absent de la collection.</returns>
        int IList.IndexOf(object value) {
            return this.IndexOf((SqlServerParameter)value);
        }

        /// <summary>
        /// Ajoute un paramètre à la collection.
        /// </summary>
        /// <param name="index">Index d'insertion (0 pour insérer en première position).</param>
        /// <param name="item">Paramètre.</param>
        public void Insert(int index, SqlServerParameter item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            _list.Insert(index, item);
            _innerCollection.Insert(index, item.InnerParameter);
        }

        /// <summary>
        /// Ajoute un paramètre à la collection.
        /// </summary>
        /// <param name="index">Index d'insertion (0 pour insérer en première position).</param>
        /// <param name="value">Paramètre.</param>
        void IList.Insert(int index, object value) {
            this.Insert(index, (SqlServerParameter)value);
        }

        /// <summary>
        /// Supprime un paramètre de la collection.
        /// </summary>
        /// <param name="item">Paramètre.</param>
        /// <returns>True si le paramètre a été supprimé.</returns>
        public bool Remove(SqlServerParameter item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }

            bool isRemoved = _list.Remove(item);
            _innerCollection.Remove(item.InnerParameter);
            return isRemoved;
        }

        /// <summary>
        /// Supprime un paramètre de la collection.
        /// </summary>
        /// <param name="value">Paramètre.</param>
        void IList.Remove(object value) {
            this.Remove((SqlServerParameter)value);
        }

        /// <summary>
        /// Supprime un paramètre de la collection.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre.</param>
        public void RemoveAt(string parameterName) {
            int index = IndexOf(parameterName);
            this.RemoveAt(index);
        }

        /// <summary>
        /// Supprime un paramètre de la collection.
        /// </summary>
        /// <param name="index">Indice du paramètre.</param>
        public void RemoveAt(int index) {
            _innerCollection.RemoveAt(index);
            _list.RemoveAt(index);
        }

        /// <summary>
        /// Supprime les caractères spéciaux de la recherche.
        /// </summary>
        /// <param name="word">Mot sur quoi porte la recherche.</param>
        /// <returns>Le mot transformé.</returns>
        private static string RemoveIllegalChar(string word) {
            string replaceWord = word;
            replaceWord = replaceWord.Replace("*", string.Empty);
            replaceWord = replaceWord.Replace("<", string.Empty);
            replaceWord = replaceWord.Replace(">", string.Empty);
            replaceWord = replaceWord.Replace("\\", string.Empty);
            replaceWord = replaceWord.Replace("/", string.Empty);
            replaceWord = replaceWord.Replace(":", string.Empty);
            replaceWord = replaceWord.Replace("?", string.Empty);
            return replaceWord.Trim();
        }

        /// <summary>
        /// Remplir un DataTable basé sur une collection donnée.
        /// </summary>
        /// <typeparam name="T">Type d'entité.</typeparam>
        /// <param name="list">Liste des objets.</param>
        /// <param name="columnsAvailable">Les colonnes de tableau.</param>
        /// <returns>DataTable.</returns>
        private static DataTable ConvertTo<T>(ICollection<T> list, string[] columnsAvailable)
            where T : new() {
            DataTable table = CreateTable<T>(columnsAvailable);
            Type entityType = typeof(T);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);
            foreach (T item in list) {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties) {
                    if (columnsAvailable == null || columnsAvailable.Contains(prop.Name)) {
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                    }
                }

                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Crée un DataTable basé sur une collection donnée.
        /// </summary>
        /// <typeparam name="T">Type d'entité.</typeparam>
        /// <param name="columnsAvailable">Les colonnes de tableau.</param>
        /// <returns>DataTable.</returns>
        private static DataTable CreateTable<T>(string[] columnsAvailable)
            where T : new() {
            Type entityType = typeof(T);
            DataTable table = new DataTable(entityType.Name);
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(entityType);
            foreach (PropertyDescriptor prop in properties) {
                if (columnsAvailable == null || columnsAvailable.Contains(prop.Name)) {
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                }
            }

            return table;
        }

        /// <summary>
        /// Construit le paramètre pour une clause IN.
        /// </summary>
        /// <param name="parameterName">Nom du paramètre dans la requête.</param>
        /// <param name="list">Liste des valeurs du IN.</param>
        /// <param name="typeName">Nom du type en base de données.</param>
        /// <param name="sqlDbType">Type SQL du IN.</param>
        /// <returns>Le paramètre créé.</returns>
        private SqlServerParameter AddInParameter(string parameterName, IEnumerable list, string typeName, SqlDbType sqlDbType) {
            if (string.IsNullOrEmpty(parameterName)) {
                throw new ArgumentNullException("parameterName");
            }

            if (list == null) {
                throw new ArgumentNullException("list");
            }

            SqlMetaData metaData = sqlDbType == SqlDbType.VarChar ? new SqlMetaData(ColDataTypeName, sqlDbType, VarCharLength) : new SqlMetaData(ColDataTypeName, sqlDbType);
            List<SqlDataRecord> dataRecordList = new List<SqlDataRecord>();
            foreach (object item in list) {
                SqlDataRecord record = new SqlDataRecord(metaData);
                record.SetValue(0, item);
                dataRecordList.Add(record);
            }

            if (dataRecordList.Count == 0) {
                SqlDataRecord record = new SqlDataRecord(metaData);
                record.SetValue(0, null);
                dataRecordList.Add(record);
            }

            SqlServerParameter parameter = new SqlServerParameter(_innerCommand.CreateParameter()) {
                ParameterName = ParamValue + parameterName,
                SqlDbType = SqlDbType.Structured,
                Direction = ParameterDirection.Input,
                TypeName = typeName,
                Value = dataRecordList
            };
            this.Add(parameter);

            return parameter;
        }
    }
}