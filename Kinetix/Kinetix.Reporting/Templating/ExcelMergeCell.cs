using System;

namespace Kinetix.Reporting.Templating {

    /// <summary>
    /// Représente une cellule fusionnée Excel.
    /// </summary>
    [CLSCompliant(false)]
    public class ExcelMergeCell {

        private readonly ExcelCell _startCell;
        private readonly ExcelCell _endCell;

        /// <summary>
        /// Créé une nouvelle cellule fusionnée.
        /// </summary>
        /// <param name="startCell">Début de la plage.</param>
        /// <param name="endCell">Fin de la plage.</param>
        public ExcelMergeCell(ExcelCell startCell, ExcelCell endCell) {
            _endCell = endCell;
            _startCell = startCell;
        }

        /// <summary>
        /// Début de la plage de fusion (cellule en haut à gauche).
        /// </summary>
        public ExcelCell StartCell {
            get {
                return _startCell;
            }
        }

        /// <summary>
        /// Fin de la plage de fusion (cellule en bas à droite).
        /// </summary>
        public ExcelCell EndCell {
            get {
                return _endCell;
            }
        }

        /// <summary>
        /// Indique si la cellule fusionnée contient une cellule donnée.
        /// </summary>
        /// <param name="cell">Cellule à vérifier.</param>
        /// <returns><code>True</code> si la cellule est contenue.</returns>
        public bool Contains(ExcelCell cell) {
            return
                _startCell.ColumnIndex <= cell.ColumnIndex &&
                cell.ColumnIndex <= _endCell.ColumnIndex &&
                _startCell.RowIndex <= cell.RowIndex &&
                cell.RowIndex <= _endCell.RowIndex;
        }
    }
}