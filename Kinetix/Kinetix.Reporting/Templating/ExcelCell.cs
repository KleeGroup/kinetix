using System;
using System.Text.RegularExpressions;

namespace Kinetix.Reporting.Templating {

    /// <summary>
    /// Représente une cellule Excel.
    /// </summary>
    [CLSCompliant(false)]
    public class ExcelCell {

        /// <summary>
        /// Regex pour découper un nom de cellule : AB23 => [ AB , 23 ].
        /// </summary>
        private static readonly Regex _referencePattern = new Regex(@"([a-zA-Z]*)(\d*)");
        private readonly uint _columnIndex;
        private readonly uint _rowIndex;
        private readonly string _columnName;
        private readonly string _name;
        private readonly ExcelTemplateBuilder _builder;

        /// <summary>
        /// Créé une nouvelle instance de ExcelCell.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <param name="columnIndex">Index de la colonne.</param>
        /// <param name="rowIndex">Index de la ligne.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        private ExcelCell(ExcelTemplateBuilder builder, uint columnIndex, uint rowIndex, string columnName) {
            _builder = builder;
            _columnIndex = columnIndex;
            _columnName = columnName;
            _rowIndex = rowIndex;
            _name = _columnName + _rowIndex;
        }

        /// <summary>
        /// Nom de la cellule.
        /// </summary>
        public string Name {
            get {
                return _name;
            }
        }

        /// <summary>
        /// Nom de la colonne.
        /// </summary>
        public string ColumnName {
            get {
                return _columnName;
            }
        }

        /// <summary>
        /// Index de la ligne.
        /// </summary>
        public uint RowIndex {
            get {
                return _rowIndex;
            }
        }

        /// <summary>
        /// Index de la colonne.
        /// </summary>
        public uint ColumnIndex {
            get {
                return _columnIndex;
            }
        }

        /// <summary>
        /// Créé une cellule Excel.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <param name="rowIndex">Index de la ligne.</param>
        /// <returns>Cellule Excel.</returns>
        public static ExcelCell Create(ExcelTemplateBuilder builder, string columnName, uint rowIndex) {
            var columnIndex = IndexFromColumnName(columnName);
            return new ExcelCell(builder, columnIndex, rowIndex, columnName);
        }

        /// <summary>
        /// Créé une cellule Excel.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <param name="reference">Nom de la cellule (A1, B2, ...).</param>
        /// <returns>Cellule Excel.</returns>
        public static ExcelCell Create(ExcelTemplateBuilder builder, string reference) {
            /* Découpe le nom de la cellule. */
            var match = _referencePattern.Match(reference);
            var columnName = match.Groups[1].Value;
            var rowName = match.Groups[2].Value;
            /* Calcule les coordonnées. */
            var rowIndex = uint.Parse(rowName);
            var columnIndex = IndexFromColumnName(columnName);
            return new ExcelCell(builder, columnIndex, rowIndex, columnName);
        }

        /// <summary>
        /// Renvoie la cellule de même ligne avec la colonne demandée.
        /// </summary>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>Cellule.</returns>
        public ExcelCell ChangeColumn(string columnName) {
            return Create(_builder, columnName, _rowIndex);
        }

        /// <summary>
        /// Renvoie la cellule de même colonne avec la ligne demandée.
        /// </summary>
        /// <param name="rowIndex">Index de la ligne.</param>
        /// <returns>Cellule.</returns>
        public ExcelCell ChangeRow(uint rowIndex) {
            return Create(_builder, _columnName, rowIndex);
        }

        /// <summary>
        /// Renvoie la cellule de même ligne et de delta colonnes en plus vers la droite.
        /// </summary>
        /// <param name="delta">Delta de colonne.</param>
        /// <returns>Cellule.</returns>
        public ExcelCell ShiftColumn(uint delta) {
            var newColumnIndex = _columnIndex + delta;
            var mergeCell = _builder.GetMergCell(this);
            if (mergeCell != null) {
                /* Cas d'une cellule fusionnée : on se place après la dernière cellule de la fusion. */
                newColumnIndex = mergeCell.EndCell.ColumnIndex + 1;
            }

            var newColumnName = ColumnNameFromIndex(newColumnIndex);
            return Create(_builder, newColumnName, _rowIndex);
        }

        /// <summary>
        /// Renvoie une cellule de même colonne et de delta lignes en plus vers le bas.
        /// </summary>
        /// <param name="delta">Delta de lignes.</param>
        /// <returns>Cellule.</returns>
        public ExcelCell ShiftRow(uint delta) {
            var newRowIndex = RowIndex + delta;
            return Create(_builder, _columnName, newRowIndex);
        }

        /// <summary>
        /// Retourne le nom de la colonne à partir de son index.
        /// </summary>
        /// <param name="colIndex">Index de la colonne.</param>
        /// <returns>Nom de la colonne.</returns>
        private static string ColumnNameFromIndex(uint colIndex) {
            string s = string.Empty;
            uint index = colIndex - 'A';
            while (true) {
                uint modulo = index % 26;
                s = (char)(modulo + 'A') + s;
                if (index <= 25) {
                    break;
                }

                index = ((index - modulo) / 26) - 1;
            }

            return s;
        }

        /// <summary>
        /// Retourne l'index de la colonne à partir de son nom.
        /// </summary>
        /// <param name="columnName">Nom de la colonne.</param>
        /// <returns>Index de la colonne.</returns>
        private static uint IndexFromColumnName(string columnName) {
            uint number = 'A' - 1;
            int pow = 1;
            for (int i = columnName.Length - 1; i >= 0; i--) {
                number += (uint)((columnName[i] - 'A' + 1) * pow);
                pow *= 26;
            }

            return number;
        }
    }
}
