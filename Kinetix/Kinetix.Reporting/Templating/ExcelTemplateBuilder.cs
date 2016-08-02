using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Kinetix.ComponentModel;

namespace Kinetix.Reporting.Templating {

    /// <summary>
    /// Builder de template Excel.
    /// Le builder se crée à partir d'un fichier Excel existant.
    /// Il permet ensuite d'écrire dans le fichier Excel en mémoire.
    /// Il fournit ensuite l'Excel final.
    /// </summary>
    [CLSCompliant(false)]
    public class ExcelTemplateBuilder : IDisposable {

        private readonly SpreadsheetDocument _excelDocument;
        private readonly Stream _documentStream;
        private readonly WorksheetPart _worksheetPart;
        private readonly Worksheet _worksheet;
        private readonly SharedStringTablePart _shareStringPart;
        private readonly ICollection<ExcelMergeCell> _mergeCells = new List<ExcelMergeCell>();
        private BooleanConfig _booleanConfig;
        private ExcelCell _currentCell;

        /// <summary>
        /// Créé une nouvelle instance de ExcelTemplateBuilder.
        /// </summary>
        /// <param name="templateStream">Stream du fichier édité.</param>
        /// <param name="sheetIndex">Index de la feuille du classeur à ouvrir, en base 1.</param>
        private ExcelTemplateBuilder(Stream templateStream, int sheetIndex) {
            /* Copie le stream du template dans un stream en mémoire. */
            _documentStream = new MemoryStream();
            templateStream.Seek(0, SeekOrigin.Begin);
            templateStream.CopyTo(_documentStream);
            _documentStream.Seek(0, SeekOrigin.Begin);

            /* Ouvre le stream dans un ExcelDocument en édition. */
            _excelDocument = SpreadsheetDocument.Open(_documentStream, true);

            /* Sélectionne les parts. */
            var worksheetParts = _excelDocument.WorkbookPart.WorksheetParts;
            var internalSheetIndex = worksheetParts.Count() - sheetIndex; // Les feuilles sont stockées dans l'ordre inverse d'affichage.
            _worksheetPart = worksheetParts.ElementAt(internalSheetIndex);
            _worksheet = _worksheetPart.Worksheet;
            _shareStringPart = _excelDocument.WorkbookPart.SharedStringTablePart;

            /* Lit les cellules fusionnées. */
            ReadMergeCells();

            /* Focus sur la première cellule */
            this.SetFocus("A1");
        }

        /// <summary>
        /// Créé un builder à partir du chemin d'un fichier Excel servant de template.
        /// </summary>
        /// <param name="templatePath">Chemin du fichier Excel template.</param>
        /// <param name="sheetIndex">Index de la feuille du classeur à ouvrir, en base 1.</param>
        /// <returns>Builder.</returns>
        public static ExcelTemplateBuilder Create(string templatePath, int sheetIndex = 1) {
            /* Charge une copie du fichier template en mémoire. */
            using (var fs = File.OpenRead(templatePath))
            using (var ms = new MemoryStream()) {
                fs.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return Create(ms, sheetIndex);
            }
        }

        /// <summary>
        /// Créé un builder à partir du stream d'un fichier Excel servant de template.
        /// </summary>
        /// <param name="templateStream">Stream fichier Excel template.</param>
        /// <param name="sheetIndex">Index de la feuille du classeur à ouvrir, en base 1.</param>
        /// <returns>Builder.</returns>
        public static ExcelTemplateBuilder Create(Stream templateStream, int sheetIndex = 1) {
            return new ExcelTemplateBuilder(templateStream, sheetIndex);
        }

        /// <summary>
        /// Ecrit le fichier Excel final dans le stream.
        /// </summary>
        /// <param name="stream">Stream cible.</param>
        /// <returns>Le builder.</returns>
        public ExcelTemplateBuilder Build(Stream stream) {

            /* Sauvegarde les modifications en cours dans le stream. */
            _worksheet.Save();
            _excelDocument.WorkbookPart.Workbook.Save();
            _excelDocument.Close();

            /* Recopie le stream du document. */
            _documentStream.Seek(0, SeekOrigin.Begin);
            _documentStream.CopyTo(stream);

            return this;
        }

        /// <summary>
        /// Ecrit le fichier Excel final au chemin fourni.
        /// </summary>
        /// <param name="path">Chemin.</param>
        /// <returns>Le builder.</returns>
        public ExcelTemplateBuilder Build(string path) {
            if (string.IsNullOrEmpty(path)) {
                throw new ArgumentNullException("path");
            }

            /* Ecrit le stream dans le fichier. */
            using (var fs = File.Create(path)) {
                this.Build(fs);
            }

            return this;
        }

        /// <summary>
        /// Définit la config pour lire des booléens.
        /// </summary>
        /// <param name="yes">L'identifieur pour "true".</param>
        /// <param name="no">L'identifieur pour "false".</param>
        /// <returns>Le builder.</returns>
        public ExcelTemplateBuilder SetBooleanConfig(string yes, string no) {
            _booleanConfig = new BooleanConfig { True = yes, False = no };
            return this;
        }

        /// <summary>
        /// Déplace le focus sur une cellule donnée.
        /// </summary>
        /// <param name="cellName">Nom de la cellule.</param>
        /// <returns>Le builder.</returns>
        public ExcelTemplateBuilder SetFocus(string cellName) {
            if (string.IsNullOrEmpty(cellName)) {
                throw new ArgumentNullException("cellName");
            }

            _currentCell = ExcelCell.Create(this, cellName);
            return this;
        }

        /// <summary>
        /// Ecrit dans la cellule courante une valeur.
        /// </summary>
        /// <param name="value">Valeur à écrire.</param>
        /// <returns>Le builder.</returns>
        public ExcelTemplateBuilder WriteCell(string value) {
            WriteCellCore(value);
            return this;
        }

        /// <summary>
        /// Récupère la valeur d'une celle correspondant au type de la propriété passé en paramètre.
        /// </summary>
        /// <param name="propertyType">Le type de la propriété.</param>
        /// <returns>La valeur de la cellule.</returns>
        public object ReadCell(Type propertyType) {
            return ReadCellCore(propertyType);
        }

        /// <summary>
        /// Ecrit dans la cellule courante une valeur.
        /// </summary>
        /// <param name="value">Valeur à écrire.</param>
        /// <returns>Le builder.</returns>
        public ExcelTemplateBuilder WriteCell(int value) {
            WriteCellCore(value.ToString(), CellValues.Number);
            return this;
        }

        /// <summary>
        /// Récupère une ligne du tableau excel du type passé en paramètre de la méthode générique.
        /// </summary>
        /// <typeparam name="TRow">Le type de l'objet de retour.</typeparam>
        /// <returns>Une ligne du fichier excel.</returns>
        public TRow ReadRow<TRow>()
            where TRow : new() {

            var row = new TRow();
            var def = BeanDescriptor.GetDefinition(row);

            var initialColumn = _currentCell.ColumnName;
            var emptyCellsNb = 0;

            /* Parcourt des propriétés du bean. */
            foreach (var propDesc in def.Properties) {
                var value = ReadCellCore(propDesc.PrimitiveType);

                // On suppose que la première ligne vide correspond à la fin du document.
                if (value == null) {
                    emptyCellsNb++;
                    if (emptyCellsNb == def.Properties.Count) {
                        return default(TRow);
                    }
                }

                propDesc.SetValue(row, value);

                /* Passe à la colonne suivante. */
                _currentCell = _currentCell.ShiftColumn(1);
            }

            /* On retourne à la cellule initiale de la ligne. */
            _currentCell = _currentCell.ChangeColumn(initialColumn);

            return row;
        }

        /// <summary>
        /// Récupère la liste des lignes du tableau excel sous le format du type passé en paramètre.
        /// </summary>
        /// <typeparam name="TRow">Le type de l'objet de retour.</typeparam>
        /// <returns>La liste des lignes du fichier Excel.</returns>
        public ICollection<TRow> ReadRows<TRow>()
            where TRow : new() {
            var initialRow = _currentCell.RowIndex;
            var rows = new List<TRow>();

            /* Parcourt les lignes. */
            while (true) {

                /* On lit la ligne. */
                var row = ReadRow<TRow>();

                if (row == null) {
                    break;
                }

                rows.Add(row);

                /* Passe à la ligne suivante. */
                _currentCell = _currentCell.ShiftRow(1);
            }

            /* Retourne à la première ligne. */
            _currentCell = _currentCell.ChangeRow(initialRow);

            return rows;
        }

        /// <summary>
        /// Déplace le focus vers la même colonne et delta lignes plus bas.
        /// </summary>
        /// <param name="delta">Le delta.</param>
        /// <returns>Le builder.</returns>
        public ExcelTemplateBuilder ShiftRow(uint delta) {
            _currentCell = _currentCell.ShiftRow(delta);
            return this;
        }

        /// <summary>
        /// Ecrit une ligne démarrant à la cellule courante.
        /// Les valeurs de cellules sont lues dans l'ordre des propriétés du bean <code>row</code>.
        /// </summary>
        /// <typeparam name="TRow">Type de la ligne.</typeparam>
        /// <param name="row">Bean de la ligne.</param>
        /// <returns>Le builder.</returns>
        public ExcelTemplateBuilder WriteRow<TRow>(TRow row) {
            var def = BeanDescriptor.GetDefinition(row);
            var initialColumn = _currentCell.ColumnName;

            /* Parcourt des propriétés du bean. */
            foreach (var propDesc in def.Properties) {
                /* Valeur de la propriété pour le bean. */
                object value = propDesc.GetValue(row);

                var cellValue = CellValues.SharedString;

                /* Cas d'une cellule vide. */
                var strValue = value == null ? string.Empty : value.ToString();

                /* Cas d'un decimal. */
                if (propDesc.PrimitiveType == typeof(decimal)) {
                    cellValue = CellValues.Number;
                    strValue = strValue.Replace(" ", string.Empty).Replace(",", ".");
                }

                /* Ecrit dans la cellule courante. */
                WriteCellCore(strValue, cellValue);

                /* Passe à la colonne suivante. */
                _currentCell = _currentCell.ShiftColumn(1);
            }

            /* On retourne à la cellule initiale de la ligne. */
            _currentCell = _currentCell.ChangeColumn(initialColumn);
            return this;
        }

        /// <summary>
        /// Ecrit une liste de lignes démarrant à la cellule courante.
        /// Les valeurs de cellules sont lues dans l'ordre des propriétés du bean <code>row</code>.
        /// Les lignes sont écrites dans l'ordre de la collection.
        /// </summary>
        /// <typeparam name="TRow">Type de la ligne.</typeparam>
        /// <param name="rows">Liste des beans de ligne.</param>
        /// <returns>Le builder.</returns>
        public ExcelTemplateBuilder WriteRows<TRow>(ICollection<TRow> rows) {
            var initialRow = _currentCell.RowIndex;

            /* Parcourt les lignes. */
            foreach (var row in rows) {

                /* Ecrit la ligne. */
                WriteRow(row);

                /* Passe à la ligne suivante. */
                _currentCell = _currentCell.ShiftRow(1);
            }

            /* Retourne à la première ligne. */
            _currentCell = _currentCell.ChangeRow(initialRow);

            return this;
        }

        /// <summary>
        /// Dispose les ressources.
        /// </summary>
        public void Dispose() {
            _excelDocument.Dispose();
            _documentStream.Dispose();
        }

        /// <summary>
        /// Obtient la cellule fusionnée qui contient une cellule, si elle existe.
        /// </summary>
        /// <param name="cell">Cellule à rechercher.</param>
        /// <returns>Cellule fusionnée.</returns>
        internal ExcelMergeCell GetMergCell(ExcelCell cell) {
            foreach (var excelMergeCell in _mergeCells) {
                if (excelMergeCell.Contains(cell)) {
                    return excelMergeCell;
                }
            }

            return null;
        }

        /// <summary>
        /// Lit la liste des cellules fusionnées du tableau.
        /// </summary>
        private void ReadMergeCells() {
            var mergedCells = _worksheet.GetFirstChild<MergeCells>();
            if (mergedCells == null) {
                return;
            }

            foreach (MergeCell mergeCell in mergedCells) {
                var arr = mergeCell.Reference.Value.Split(':');
                var start = arr[0];
                var end = arr[1];
                var excelMergeCell = new ExcelMergeCell(ExcelCell.Create(this, start), ExcelCell.Create(this, end));
                _mergeCells.Add(excelMergeCell);
            }
        }

        /// <summary>
        /// Met à jour la valeur d'une cellule.
        /// </summary>
        /// <param name="text">Texte.</param>
        /// <param name="cellValue">Valeur de cellule.</param>
        private void WriteCellCore(string text, CellValues cellValue = CellValues.SharedString) {

            /* Obtient la cellule. */
            Cell cell = GetCell(_currentCell);
            switch (cellValue) {
                case CellValues.SharedString:
                    var index = InsertSharedStringItem(text);
                    cell.CellValue = new CellValue(index.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                    break;
                case CellValues.Number:
                    cell.CellValue = new CellValue(text);
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                    break;
                default:
                    throw new NotSupportedException("Valeur non supportée : " + cellValue.ToString());
            }
        }

        /// <summary>
        /// Récupère la valeur d'une celle correspondant au type de la propriété passé en paramètre.
        /// </summary>
        /// <param name="propertyType">Le type de la propriété.</param>
        /// <returns>La valeur de la cellule.</returns>
        private object ReadCellCore(Type propertyType) {
            /* Obtient la cellule. */
            Cell cell = GetCell(_currentCell);

            if (cell == null || cell.CellValue == null) {
                return null;
            }

            if (propertyType == typeof(decimal)) {
                try {
                    return decimal.Parse(cell.CellValue.Text.Replace(".", ","));
                } catch (FormatException) {
                    return null;
                }
            }

            if (propertyType == typeof(int)) {
                try {
                    return int.Parse(cell.CellValue.Text);
                } catch (FormatException) {
                    return null;
                }
            }

            if (propertyType == typeof(string)) {
                return GetSharedStringValueByIndex(int.Parse(cell.CellValue.Text));
            }

            if (propertyType == typeof(bool) && _booleanConfig != null) {
                return cell.CellValue.Text == _booleanConfig.True ? true : cell.CellValue.Text == _booleanConfig.False ? false : (bool?)null;
            }

            if (propertyType == typeof(DateTime)) {
                try {
                    return DateTime.FromOADate(double.Parse(cell.CellValue.Text));
                } catch (FormatException) {
                    return null;
                }
            }

            throw new NotSupportedException("Type non supporté : " + propertyType);
        }

        // Given a worksheet, a column name, and a row index,
        // gets the cell at the specified column and
        private Cell GetCell(ExcelCell excelCell) {
            /* Obtient la ligne. */
            Row row = GetRow(excelCell.RowIndex);

            if (row == null) {
                return null;
            }

            /* Obtient la cellule. */
            string cellName = excelCell.Name;
            Cell cell = row.Elements<Cell>()
                           .Where(c => string.Compare(c.CellReference.Value, cellName, true) == 0)
                           .FirstOrDefault();

            // if (cell == null) {
            //    throw new NotSupportedException("Cellule introuvable : " + cellName);
            // }
            return cell;
        }

        /// <summary>
        /// Retourne la ligne d'index donnée.
        /// </summary>
        /// <param name="rowIndex">Index de la ligne.</param>
        /// <returns>Ligne.</returns>
        private Row GetRow(uint rowIndex) {
            return _worksheet.GetFirstChild<SheetData>()
                .Elements<Row>()
                .Where(r => r.RowIndex == rowIndex)
                .FirstOrDefault();

            // if (row == null) {
            //    throw new NotSupportedException("Ligne introuvable : " + rowIndex);
            // }
        }

        /// <summary>
        /// Renvoie l'index d'une string dans la SharedStringTable.
        /// Créé l'item s'il est manquant.
        /// </summary>
        /// <param name="text">Texte à stocker dans la table.</param>
        /// <returns>Index de l'iem dans la SharedStringTable.</returns>
        private int InsertSharedStringItem(string text) {

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in _shareStringPart.SharedStringTable.Elements<SharedStringItem>()) {
                if (item.InnerText == text) {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            _shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new Text(text)));
            _shareStringPart.SharedStringTable.Save();

            return i;
        }

        /// <summary>
        /// Récupère la valeur de la string contenue dans la SharedStringTable à l'index passé en paramètre.
        /// </summary>
        /// <param name="index">L'index dans lequel est stockée la valeur.</param>
        /// <returns>La valeur de la string.</returns>
        private string GetSharedStringValueByIndex(int index) {
            return _shareStringPart.SharedStringTable.Elements<SharedStringItem>().ElementAt(index).InnerText;
        }

        private class BooleanConfig {

            /// <summary>
            /// Identifieur pour "true".
            /// </summary>
            public string True { get; set; }

            /// <summary>
            /// Identifieur pour "false".
            /// </summary>
            public string False { get; set; }
        }
    }
}
