using System;
using System.Collections.Generic;

namespace Kinetix.Broker {

    /// <summary>
    /// Seletion des columns à sauvegarder ou à ignorer pour la sauvegarder.
    /// </summary>
    public class ColumnSelector {

        private readonly ICollection<string> _columnList = new List<string>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="columnList">List of selected columns.</param>
        public ColumnSelector(params Enum[] columnList) {
            this.Add(columnList);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="columnList">List of selected columns.</param>
        public ColumnSelector(params string[] columnList) {
            this.Add(columnList);
        }

        /// <summary>
        /// Get the selected columns.
        /// </summary>
        public ICollection<string> ColumnList {
            get {
                return _columnList;
            }
        }

        /// <summary>
        /// Add columns.
        /// </summary>
        /// <param name="columnList">List of selected columns.</param>
        public void Add(params Enum[] columnList) {
            foreach (Enum col in columnList) {
                _columnList.Add(col.ToString());
            }
        }

        /// <summary>
        /// Add columns.
        /// </summary>
        /// <param name="columnList">List of selected columns.</param>
        public void Add(params string[] columnList) {
            foreach (string col in columnList) {
                _columnList.Add(col);
            }
        }
    }
}
