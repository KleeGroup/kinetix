using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Kinetix.ComponentModel;

namespace Kinetix.Data.SqlClient {

    /// <summary>
    /// Paramètre de tri des résultats et de limit des résultats.
    /// Les objets remontés sont triés par les valeurs associées à la map.
    /// </summary>
    public class QueryParameter {

        /// <summary>
        /// Indique que le count de toutes les pages doit être fourni.
        /// </summary>
        public const string InlineCountAllPages = "allpages";

        /// <summary>
        /// Liste des ordres de tri.
        /// </summary>
        private readonly List<string> _sortList = new List<string>();

        /// <summary>
        /// Map des colonnes de tri.
        /// </summary>
        private readonly Dictionary<string, SortOrder> _mapSort = new Dictionary<string, SortOrder>();

        /// <summary>
        /// Limit.
        /// </summary>
        private int _limit = 0;

        /// <summary>
        /// Offset.
        /// </summary>
        private int _offset = 0;

        /// <summary>
        /// Max rows.
        /// </summary>
        private int _maxRows = 0;

        /// <summary>
        /// If manualSort.
        /// </summary>
        private bool? _isManualSort = false;

        /// <summary>
        /// Constructeur vide.
        /// </summary>
        public QueryParameter() {
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="limit">Nombre de lignes à ramener.</param>
        public QueryParameter(int limit) {
            this.Limit = limit;
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="limit">Nombre de lignes à ramener.</param>
        /// <param name="offset">Nombre de lignes à sauter.</param>
        public QueryParameter(int limit, int offset) {
            this.Limit = limit;
            this.Offset = offset;
        }

        /// <summary>
        /// Constructeur à partir d'un nom de champ et d'un ordre de tri.
        /// </summary>
        /// <param name="enumCol">Type énuméré présentant la colonne.</param>
        /// <param name="order">Ordre de tri de cette colonne.</param>
        public QueryParameter(Enum enumCol, SortOrder order) {
            AddSortParam(enumCol, order);
        }

        /// <summary>
        /// Constructeur à partir d'un nom de champ et d'un ordre de tri.
        /// </summary>
        /// <param name="enumCol">Type énuméré présentant la colonne.</param>
        /// <param name="order">Ordre de tri de cette colonne.</param>
        /// <param name="limit">Nombre de lignes à ramener.</param>
        public QueryParameter(Enum enumCol, SortOrder order, int limit) {
            this.Limit = limit;
            AddSortParam(enumCol, order);
        }

        /// <summary>
        /// Constructeur à partir d'un nom de champ et d'un ordre de tri.
        /// </summary>
        /// <param name="enumCol">Type énuméré présentant la colonne.</param>
        /// <param name="order">Ordre de tri de cette colonne.</param>
        /// <param name="limit">Nombre de lignes à ramener.</param>
        /// <param name="offset">Nombre de lignes à sauter.</param>
        public QueryParameter(Enum enumCol, SortOrder order, int limit, int offset) {
            this.Limit = limit;
            this.Offset = offset;
            AddSortParam(enumCol, order);
        }

        /// <summary>
        /// Nombre de lignes à remener.
        /// </summary>
        public int Limit {
            get {
                return _limit;
            }

            set {
                _limit = value;
            }
        }

        /// <summary>
        /// Nombre de lignes maximum.
        /// </summary>
        public int MaxRows {
            get {
                return _maxRows;
            }

            set {
                _maxRows = value;
            }
        }

        /// <summary>
        /// Nombre de lignes à sauter.
        /// </summary>
        public int Offset {
            get {
                return _offset;
            }

            set {
                _offset = value;
            }
        }

        /// <summary>
        /// Indique que l'inlineCount est demandé.
        /// </summary>
        public bool InlineCount {
            get;
            set;
        }

        /// <summary>
        /// Retourne la liste des nom des paramètres.
        /// </summary>
        public ICollection<string> SortedFields {
            get {
                return new ReadOnlyCollection<string>(_sortList);
            }
        }

        /// <summary>
        /// Indique si un tri manuel doit être effectué.
        /// </summary>
        public bool IsManualSort {
            get {
                if (_isManualSort == null) {
                    throw new NotSupportedException("Call ExcludeColumns first !");
                }

                return _isManualSort.Value;
            }
        }

        /// <summary>
        /// Define the sort traduction.
        /// </summary>
        /// <returns>Sort condition.</returns>
        public string SortCondition {
            get {
                StringBuilder orderClause = new StringBuilder();
                bool isFirst = true;
                foreach (string sort in _sortList) {
                    orderClause.Append(' ');
                    if (isFirst) {
                        isFirst = false;
                    } else {
                        orderClause.Append(", ");
                    }

                    string orderCol = FirstToUpper(sort);
                    orderClause.Append('"' + orderCol + "\" " + ((_mapSort[sort] == SortOrder.Desc) ? "desc" : "asc"));
                }

                return orderClause.ToString();
            }
        }

        /// <summary>
        /// Retourne le tri associé au champ.
        /// </summary>
        /// <param name="fieldName">Nom du champ.</param>
        /// <returns>Ordre de tri.</returns>
        public SortOrder this[string fieldName] {
            get {
                return _mapSort[fieldName];
            }
        }

        /// <summary>
        /// Indique si le tri est réalisé sur une colonne.
        /// </summary>
        /// <param name="memberName">Membre.</param>
        /// <returns>True si le tri se fait sur la colonne.</returns>
        public bool IsSortBy(string memberName) {
            return _mapSort.ContainsKey(memberName);
        }

        /// <summary>
        /// Indique l'ordre de tri sur une colonne.
        /// </summary>
        /// <param name="memberName">Membre.</param>
        /// <returns>True si le tri se fait sur la colonne.</returns>
        public SortOrder GetSortOrder(string memberName) {
            return _mapSort[memberName];
        }

        /// <summary>
        /// Disable pagination.
        /// </summary>
        public void DisablePagination() {
            // TODO : Optimisation possible avec un paramètre client-side.
            this.Limit = 0;
            this.Offset = 0;
        }

        /// <summary>
        /// Retourne les paramètres à appliquer en cas de colonnes à exclure.
        /// </summary>
        /// <param name="columns">Liste des colonnes à exclure.</param>
        /// <returns>Paramètres.</returns>
        public QueryParameter ExcludeColumns(params string[] columns) {
            if (columns == null) {
                throw new ArgumentNullException("columns");
            }

            foreach (string col in columns) {
                if (this.IsSortBy(col)) {
                    _isManualSort = true;
                    return null;
                }
            }

            _isManualSort = false;
            return this;
        }

        /// <summary>
        /// Applique le tri et le filtrage à la liste.
        /// </summary>
        /// <typeparam name="TSource">Type source.</typeparam>
        /// <param name="list">Liste source.</param>
        /// <returns>Liste triée.</returns>
        public ICollection<TSource> Apply<TSource>(ICollection<TSource> list) {
            if (list == null) {
                throw new ArgumentNullException("list");
            }

            if (_sortList.Count != 1) {
                throw new NotImplementedException();
            }

            if (this.InlineCount) {
                QueryContext.InlineCount = list.Count;
            }

            string sortColumn = _sortList[0];
            SortOrder sortOrder = _mapSort[sortColumn];

            BeanDefinition beanDef = BeanDescriptor.GetDefinition(typeof(TSource));
            BeanPropertyDescriptor propertyDescriptor = beanDef.Properties[FirstToUpper(sortColumn)];

            if (sortOrder == SortOrder.Asc) {
                list = list.OrderBy(x => propertyDescriptor.GetValue(x)).ToList();
            } else {
                list = list.OrderByDescending(x => propertyDescriptor.GetValue(x)).ToList();
            }

            // If this.Limit == 0 we disable pagination.
            return list.Skip(this.Offset).Take(this.Limit > 0 ? this.Limit : list.Count).ToList();
        }

        /// <summary>
        /// Remap columns to persistant names.
        /// </summary>
        /// <param name="beanType">Bean.</param>
        public void RemapSortColumn(Type beanType) {
            if (beanType == null) {
                throw new ArgumentNullException("beanType");
            }

            BeanDefinition beanDef = BeanDescriptor.GetDefinition(beanType);
            foreach (string sort in _sortList.ToArray()) {
                string orderCol = FirstToUpper(sort);
                if (beanDef.Properties.Contains(orderCol)) {
                    string colName = beanDef.Properties[orderCol].MemberName;
                    if (!string.IsNullOrEmpty(colName)) {
                        this.RemapSortColumn(sort, colName);
                    }
                }
            }
        }

        /// <summary>
        /// Remap column name.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <param name="columnName">Column name.</param>
        public void RemapSortColumn(string propertyName, string columnName) {
            // Lecture de l'état initial.
            int index = _sortList.IndexOf(propertyName);
            SortOrder order = _mapSort[propertyName];

            // Suppression de l'état initial.
            _sortList.RemoveAt(index);
            _mapSort.Remove(propertyName);

            // Création du nouvel état.
            _sortList.Insert(index, columnName);
            _mapSort[columnName] = order;
        }

        /// <summary>
        /// Ajoute un critère de tri.
        /// </summary>
        /// <param name="enumCol">Type énuméré présentant la colonne.</param>
        /// <param name="order">Ordre de tri.</param>
        public void AddSortParam(Enum enumCol, SortOrder order) {
            if (enumCol == null) {
                throw new ArgumentNullException("enumCol");
            }

            string column = enumCol.ToString();
            _sortList.Add(column);
            _mapSort.Add(column, order);
        }

        /// <summary>
        /// Ajoute la chaine de tri.
        /// </summary>
        /// <param name="param">Param.</param>
        public void AddSortParam(string param) {
            if (param == null) {
                throw new ArgumentNullException("param");
            }

            string[] orderClause = param.Split(',');
            foreach (string orderby in orderClause) {
                string[] order = orderby.Split(' ');
                if (order.Length == 1 || order[1] != "desc") {
                    _sortList.Add(order[0]);
                    _mapSort.Add(order[0], SortOrder.Asc);
                } else {
                    _sortList.Add(order[0]);
                    _mapSort.Add(order[0], SortOrder.Desc);
                }
            }
        }

        /// <summary>
        /// Returns the value wih the first character uppered.
        /// </summary>
        /// <param name="value">Value to parse.</param>
        /// <returns>Parsed value.</returns>
        private static string FirstToUpper(string value) {
            if (string.IsNullOrEmpty(value)) {
                return value;
            }

            return value.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + value.Substring(1);
        }
    }
}
