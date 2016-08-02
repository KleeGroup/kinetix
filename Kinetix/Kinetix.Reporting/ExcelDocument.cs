using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Validation;
using Kinetix.ComponentModel;
using Kinetix.ServiceModel;
using log4net;
using a = DocumentFormat.OpenXml.Drawing;

namespace Kinetix.Reporting {

    /// <summary>
    /// Cette classe représente un document XML.
    /// </summary>
    internal sealed class ExcelDocument : IDisposable {

        private static readonly byte[] Calibri11 = new byte[] {
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            3, 5, 7, 7, 7, 11, 11, 4, 5, 5, 7, 7, 4, 5, 4, 6,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 4, 4, 7, 7, 7, 7,
            13, 9, 8, 8, 9, 7, 7, 10, 9, 4, 5, 8, 6, 13, 10, 10,
            8, 10, 8, 7, 7, 10, 9, 14, 8, 8, 7, 5, 6, 5, 7, 7,
            5, 7, 8, 6, 8, 8, 5, 7, 8, 4, 4, 7, 4, 12, 8, 8,
            8, 8, 5, 6, 5, 8, 7, 11, 7, 7, 6, 5, 7, 5, 7, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            3, 5, 7, 7, 7, 7, 7, 7, 6, 13, 6, 8, 7, 5, 8, 6,
            5, 7, 5, 5, 5, 8, 9, 4, 5, 4, 7, 8, 10, 10, 11, 7,
            9, 9, 9, 9, 9, 9, 12, 8, 7, 7, 7, 7, 4, 4, 4, 4,
            10, 10, 10, 10, 10, 10, 10, 7, 10, 10, 10, 10, 10, 8, 8, 8,
            7, 7, 7, 7, 7, 7, 12, 6, 8, 8, 8, 8, 4, 4, 4, 4,
            8, 8, 8, 8, 8, 8, 8, 7, 8, 8, 8, 8, 8, 7, 8, 7
        };

        private readonly SpreadsheetDocument _excelDocument;
        private readonly SharedStringTable _shareStringTable = new SharedStringTable();
        private readonly string _fileName;
        private int _sharedStringIndex = 0;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="fileName">Nom du document.</param>
        public ExcelDocument(string fileName) {
            if (string.IsNullOrEmpty(fileName)) {
                throw new ArgumentNullException("fileName");
            }

            _fileName = fileName;
            _excelDocument = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);
        }

        /// <summary>
        /// Remplit la feuille 1 du document XML.
        /// </summary>
        /// <param name="exportSheetList">Liste des feuilles.</param>
        public void FillDocument(ICollection<ExportSheet> exportSheetList) {
            if (exportSheetList == null) {
                throw new ArgumentNullException("exportSheetList");
            }

            // ExtendedFilePropertiesPart : non implémenté.
            // CoreFilePropertiesPart : non implémenté.

            // Création du WorkbookPart.
            WorkbookPart workbookPart = _excelDocument.AddWorkbookPart();
            GenerateWorkbookPart(exportSheetList).Save(workbookPart);

            int i = 1;
            foreach (ExportSheet sheet in exportSheetList) {
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>("rId" + i);
                CreateSheet(sheet).Save(worksheetPart);
                ++i;
            }

            // Création des thémes.
            ++i;
            ThemePart themePart = workbookPart.AddNewPart<ThemePart>("rId" + i);
            GenerateThemePart().Save(themePart);

            // Création de la table des styles.
            ++i;
            WorkbookStylesPart workbookStylesPart = workbookPart.AddNewPart<WorkbookStylesPart>("rId" + i);
            GenerateWorkbookStylesPart().Save(workbookStylesPart);

            // Création de la table des SharedString.
            ++i;
            SharedStringTablePart sharedStringTablePart = workbookPart.AddNewPart<SharedStringTablePart>("rId" + i);
            _shareStringTable.Save(sharedStringTablePart);

#if FONTTABLE
            WorksheetPart worksheetPartTest = workbookPart.AddNewPart<WorksheetPart>("rId" + exportSheetList.Count + 4);
            CreateTestSheet().Save(worksheetPartTest);
#endif
            ValidateDocument();
        }

        /// <summary>
        /// Libère les ressources de la classe.
        /// </summary>
        public void Dispose() {
            _excelDocument.Dispose();
        }

        /// <summary>
        /// Crée le théme du document.
        /// </summary>
        /// <returns>Theme.</returns>
        [SuppressMessage("Cpd", "Cpd", Justification = "Issu de l'outil de désassemblage de DOCX.")]
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "Code générés automatiquement par Excel.")]
        [SuppressMessage("Microsoft.StyleCop.CSharp.SpacingRules", "SA1009: ClosingParenthesisMustBeSpacedCorrectly", Justification = "Indentation adaptée.")]
        private static a.Theme GenerateThemePart() {
            return new a.Theme(
                    new a.ThemeElements(
                        new a.ColorScheme(
                            new a.Dark1Color(
                                new a.SystemColor() { Val = a.SystemColorValues.WindowText, LastColor = "000000" }),
                            new a.Light1Color(
                                new a.SystemColor() { Val = a.SystemColorValues.Window, LastColor = "FFFFFF" }),
                            new a.Dark2Color(
                                new a.RgbColorModelHex() { Val = "1F497D" }),
                            new a.Light2Color(
                                new a.RgbColorModelHex() { Val = "EEECE1" }),
                            new a.Accent1Color(
                                new a.RgbColorModelHex() { Val = "4F81BD" }),
                            new a.Accent2Color(
                                new a.RgbColorModelHex() { Val = "C0504D" }),
                            new a.Accent3Color(
                                new a.RgbColorModelHex() { Val = "9BBB59" }),
                            new a.Accent4Color(
                                new a.RgbColorModelHex() { Val = "8064A2" }),
                            new a.Accent5Color(
                                new a.RgbColorModelHex() { Val = "4BACC6" }),
                            new a.Accent6Color(
                                new a.RgbColorModelHex() { Val = "F79646" }),
                            new a.Hyperlink(
                                new a.RgbColorModelHex() { Val = "0000FF" }),
                            new a.FollowedHyperlinkColor(
                                new a.RgbColorModelHex() { Val = "800080" })) { Name = "Office" },
                        new a.FontScheme(
                            new a.MajorFont(
                                new a.LatinFont() { Typeface = "Cambria" },
                                new a.EastAsianFont() { Typeface = string.Empty },
                                new a.ComplexScriptFont() { Typeface = string.Empty },
                                new a.SupplementalFont() { Script = "Jpan", Typeface = "ＭＳ Ｐゴシック" },
                                new a.SupplementalFont() { Script = "Hang", Typeface = "맑은 고딕" },
                                new a.SupplementalFont() { Script = "Hans", Typeface = "宋体" },
                                new a.SupplementalFont() { Script = "Hant", Typeface = "新細明體" },
                                new a.SupplementalFont() { Script = "Arab", Typeface = "Times New Roman" },
                                new a.SupplementalFont() { Script = "Hebr", Typeface = "Times New Roman" },
                                new a.SupplementalFont() { Script = "Thai", Typeface = "Tahoma" },
                                new a.SupplementalFont() { Script = "Ethi", Typeface = "Nyala" },
                                new a.SupplementalFont() { Script = "Beng", Typeface = "Vrinda" },
                                new a.SupplementalFont() { Script = "Gujr", Typeface = "Shruti" },
                                new a.SupplementalFont() { Script = "Khmr", Typeface = "MoolBoran" },
                                new a.SupplementalFont() { Script = "Knda", Typeface = "Tunga" },
                                new a.SupplementalFont() { Script = "Guru", Typeface = "Raavi" },
                                new a.SupplementalFont() { Script = "Cans", Typeface = "Euphemia" },
                                new a.SupplementalFont() { Script = "Cher", Typeface = "Plantagenet Cherokee" },
                                new a.SupplementalFont() { Script = "Yiii", Typeface = "Microsoft Yi Baiti" },
                                new a.SupplementalFont() { Script = "Tibt", Typeface = "Microsoft Himalaya" },
                                new a.SupplementalFont() { Script = "Thaa", Typeface = "MV Boli" },
                                new a.SupplementalFont() { Script = "Deva", Typeface = "Mangal" },
                                new a.SupplementalFont() { Script = "Telu", Typeface = "Gautami" },
                                new a.SupplementalFont() { Script = "Taml", Typeface = "Latha" },
                                new a.SupplementalFont() { Script = "Syrc", Typeface = "Estrangelo Edessa" },
                                new a.SupplementalFont() { Script = "Orya", Typeface = "Kalinga" },
                                new a.SupplementalFont() { Script = "Mlym", Typeface = "Kartika" },
                                new a.SupplementalFont() { Script = "Laoo", Typeface = "DokChampa" },
                                new a.SupplementalFont() { Script = "Sinh", Typeface = "Iskoola Pota" },
                                new a.SupplementalFont() { Script = "Mong", Typeface = "Mongolian Baiti" },
                                new a.SupplementalFont() { Script = "Viet", Typeface = "Times New Roman" },
                                new a.SupplementalFont() { Script = "Uigh", Typeface = "Microsoft Uighur" }),
                            new a.MinorFont(
                                new a.LatinFont() { Typeface = "Calibri" },
                                new a.EastAsianFont() { Typeface = string.Empty },
                                new a.ComplexScriptFont() { Typeface = string.Empty },
                                new a.SupplementalFont() { Script = "Jpan", Typeface = "ＭＳ Ｐゴシック" },
                                new a.SupplementalFont() { Script = "Hang", Typeface = "맑은 고딕" },
                                new a.SupplementalFont() { Script = "Hans", Typeface = "宋体" },
                                new a.SupplementalFont() { Script = "Hant", Typeface = "新細明體" },
                                new a.SupplementalFont() { Script = "Arab", Typeface = "Arial" },
                                new a.SupplementalFont() { Script = "Hebr", Typeface = "Arial" },
                                new a.SupplementalFont() { Script = "Thai", Typeface = "Tahoma" },
                                new a.SupplementalFont() { Script = "Ethi", Typeface = "Nyala" },
                                new a.SupplementalFont() { Script = "Beng", Typeface = "Vrinda" },
                                new a.SupplementalFont() { Script = "Gujr", Typeface = "Shruti" },
                                new a.SupplementalFont() { Script = "Khmr", Typeface = "DaunPenh" },
                                new a.SupplementalFont() { Script = "Knda", Typeface = "Tunga" },
                                new a.SupplementalFont() { Script = "Guru", Typeface = "Raavi" },
                                new a.SupplementalFont() { Script = "Cans", Typeface = "Euphemia" },
                                new a.SupplementalFont() { Script = "Cher", Typeface = "Plantagenet Cherokee" },
                                new a.SupplementalFont() { Script = "Yiii", Typeface = "Microsoft Yi Baiti" },
                                new a.SupplementalFont() { Script = "Tibt", Typeface = "Microsoft Himalaya" },
                                new a.SupplementalFont() { Script = "Thaa", Typeface = "MV Boli" },
                                new a.SupplementalFont() { Script = "Deva", Typeface = "Mangal" },
                                new a.SupplementalFont() { Script = "Telu", Typeface = "Gautami" },
                                new a.SupplementalFont() { Script = "Taml", Typeface = "Latha" },
                                new a.SupplementalFont() { Script = "Syrc", Typeface = "Estrangelo Edessa" },
                                new a.SupplementalFont() { Script = "Orya", Typeface = "Kalinga" },
                                new a.SupplementalFont() { Script = "Mlym", Typeface = "Kartika" },
                                new a.SupplementalFont() { Script = "Laoo", Typeface = "DokChampa" },
                                new a.SupplementalFont() { Script = "Sinh", Typeface = "Iskoola Pota" },
                                new a.SupplementalFont() { Script = "Mong", Typeface = "Mongolian Baiti" },
                                new a.SupplementalFont() { Script = "Viet", Typeface = "Arial" },
                                new a.SupplementalFont() { Script = "Uigh", Typeface = "Microsoft Uighur" })) { Name = "Office" },
                        new a.FormatScheme(
                            new a.FillStyleList(
                                new a.SolidFill(
                                    new a.SchemeColor() { Val = a.SchemeColorValues.PhColor }),
                                new a.GradientFill(
                                    new a.GradientStopList(
                                        new a.GradientStop(
                                            new a.SchemeColor(
                                                new a.Tint() { Val = 50000 },
                                                new a.SaturationModulation() { Val = 300000 }) { Val = a.SchemeColorValues.PhColor }) { Position = 0 },
                                        new a.GradientStop(
                                            new a.SchemeColor(
                                                new a.Tint() { Val = 37000 },
                                                new a.SaturationModulation() { Val = 300000 }) { Val = a.SchemeColorValues.PhColor }) { Position = 35000 },
                                        new a.GradientStop(
                                            new a.SchemeColor(
                                                new a.Tint() { Val = 15000 },
                                                new a.SaturationModulation() { Val = 350000 }) { Val = a.SchemeColorValues.PhColor }) { Position = 100000 }),
                                    new a.LinearGradientFill() { Angle = 16200000, Scaled = true }) { RotateWithShape = true },
                                new a.GradientFill(
                                    new a.GradientStopList(
                                        new a.GradientStop(
                                            new a.SchemeColor(
                                                new a.Shade() { Val = 51000 },
                                                new a.SaturationModulation() { Val = 130000 }) { Val = a.SchemeColorValues.PhColor }) { Position = 0 },
                                        new a.GradientStop(
                                            new a.SchemeColor(
                                                new a.Shade() { Val = 93000 },
                                                new a.SaturationModulation() { Val = 130000 }) { Val = a.SchemeColorValues.PhColor }) { Position = 80000 },
                                        new a.GradientStop(
                                            new a.SchemeColor(
                                                new a.Shade() { Val = 94000 },
                                                new a.SaturationModulation() { Val = 135000 }) { Val = a.SchemeColorValues.PhColor }) { Position = 100000 }),
                                    new a.LinearGradientFill() { Angle = 16200000, Scaled = false }) { RotateWithShape = true }),
                            new a.LineStyleList(
                                new a.Outline(
                                    new a.SolidFill(
                                        new a.SchemeColor(
                                            new a.Shade() { Val = 95000 },
                                            new a.SaturationModulation() { Val = 105000 }) { Val = a.SchemeColorValues.PhColor }),
                                    new a.PresetDash() { Val = a.PresetLineDashValues.Solid }) { Width = 9525, CapType = a.LineCapValues.Flat, CompoundLineType = a.CompoundLineValues.Single, Alignment = a.PenAlignmentValues.Center },
                                new a.Outline(
                                    new a.SolidFill(
                                        new a.SchemeColor() { Val = a.SchemeColorValues.PhColor }),
                                    new a.PresetDash() { Val = a.PresetLineDashValues.Solid }) { Width = 25400, CapType = a.LineCapValues.Flat, CompoundLineType = a.CompoundLineValues.Single, Alignment = a.PenAlignmentValues.Center },
                                new a.Outline(
                                    new a.SolidFill(
                                        new a.SchemeColor() { Val = a.SchemeColorValues.PhColor }),
                                    new a.PresetDash() { Val = a.PresetLineDashValues.Solid }) { Width = 38100, CapType = a.LineCapValues.Flat, CompoundLineType = a.CompoundLineValues.Single, Alignment = a.PenAlignmentValues.Center }),
                            new a.EffectStyleList(
                                new a.EffectStyle(
                                    new a.EffectList(
                                        new a.OuterShadow(
                                            new a.RgbColorModelHex(
                                                new a.Alpha() { Val = 38000 }) { Val = "000000" }) { BlurRadius = 40000L, Distance = 20000L, Direction = 5400000, RotateWithShape = false })),
                                new a.EffectStyle(
                                    new a.EffectList(
                                        new a.OuterShadow(
                                            new a.RgbColorModelHex(
                                                new a.Alpha() { Val = 35000 }) { Val = "000000" }) { BlurRadius = 40000L, Distance = 23000L, Direction = 5400000, RotateWithShape = false })),
                                new a.EffectStyle(
                                    new a.EffectList(
                                        new a.OuterShadow(
                                            new a.RgbColorModelHex(
                                                new a.Alpha() { Val = 35000 }) { Val = "000000" }) { BlurRadius = 40000L, Distance = 23000L, Direction = 5400000, RotateWithShape = false }),
                                    new a.Scene3DType(
                                        new a.Camera(
                                            new a.Rotation() { Latitude = 0, Longitude = 0, Revolution = 0 }) { Preset = a.PresetCameraValues.OrthographicFront },
                                        new a.LightRig(
                                            new a.Rotation() { Latitude = 0, Longitude = 0, Revolution = 1200000 }) { Rig = a.LightRigValues.ThreePoints, Direction = a.LightRigDirectionValues.Top }),
                                    new a.Shape3DType(
                                        new a.BevelTop() { Width = 63500L, Height = 25400L }))),
                            new a.BackgroundFillStyleList(
                                new a.SolidFill(
                                    new a.SchemeColor() { Val = a.SchemeColorValues.PhColor }),
                                new a.GradientFill(
                                    new a.GradientStopList(
                                        new a.GradientStop(
                                            new a.SchemeColor(
                                                new a.Tint() { Val = 40000 },
                                                new a.SaturationModulation() { Val = 350000 }) { Val = a.SchemeColorValues.PhColor }) { Position = 0 },
                                        new a.GradientStop(
                                            new a.SchemeColor(
                                                new a.Tint() { Val = 45000 },
                                                new a.Shade() { Val = 99000 },
                                                new a.SaturationModulation() { Val = 350000 }) { Val = a.SchemeColorValues.PhColor }) { Position = 40000 },
                                        new a.GradientStop(
                                            new a.SchemeColor(
                                                new a.Shade() { Val = 20000 },
                                                new a.SaturationModulation() { Val = 255000 }) { Val = a.SchemeColorValues.PhColor }) { Position = 100000 }),
                                    new a.PathGradientFill(
                                        new a.FillToRectangle() { Left = 50000, Top = -80000, Right = 50000, Bottom = 180000 }) { Path = a.PathShadeValues.Circle }) { RotateWithShape = true },
                                new a.GradientFill(
                                    new a.GradientStopList(
                                        new a.GradientStop(
                                            new a.SchemeColor(
                                                new a.Tint() { Val = 80000 },
                                                new a.SaturationModulation() { Val = 300000 }) { Val = a.SchemeColorValues.PhColor }) { Position = 0 },
                                        new a.GradientStop(
                                            new a.SchemeColor(
                                                new a.Shade() { Val = 30000 },
                                                new a.SaturationModulation() { Val = 200000 }) { Val = a.SchemeColorValues.PhColor }) { Position = 100000 }),
                                    new a.PathGradientFill(
                                        new a.FillToRectangle() { Left = 50000, Top = 50000, Right = 50000, Bottom = 50000 }) { Path = a.PathShadeValues.Circle }) { RotateWithShape = true })) { Name = "Office" }),
                    new a.ObjectDefaults(),
                    new a.ExtraColorSchemeList()) { Name = "Thème Office" };
        }

        /// <summary>
        /// Retourne la table des styles.
        /// </summary>
        /// <returns>Tables des styles.</returns>
        [SuppressMessage("Cpd", "Cpd", Justification = "Issu de l'outil de désassemblage de DOCX.")]
        [SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals", Justification = "Code générés automatiquement par Excel.")]
        private static Stylesheet GenerateWorkbookStylesPart() {
            return new Stylesheet(
                    new Fonts(
                        new Font(
                            new FontSize() { Val = 11D },
                            new Color() { Theme = (UInt32Value)1U },
                            new FontName() { Val = "Calibri" },
                            new FontFamily() { Val = 2 },
                            new FontScheme() { Val = FontSchemeValues.Minor }),
                        new Font(
                            new Bold(),
                            new FontSize() { Val = 11D },
                            new Color() { Theme = (UInt32Value)1U },
                            new FontName() { Val = "Calibri" },
                            new FontFamily() { Val = 2 },
                            new FontScheme() { Val = FontSchemeValues.Minor })) { Count = (UInt32Value)2U },
                    new Fills(
                        new Fill(
                            new PatternFill() { PatternType = PatternValues.None }),
                        new Fill(
                            new PatternFill() { PatternType = PatternValues.Gray125 }),
                        new Fill(
                            new PatternFill(
                                new ForegroundColor() { Theme = (UInt32Value)0U, Tint = new DoubleValue() { InnerText = "-0.14996795556505021" } },
                                new BackgroundColor() { Indexed = (UInt32Value)64U }) { PatternType = PatternValues.Solid }),
                        new Fill(
                            new PatternFill(
                                new ForegroundColor() { Theme = (UInt32Value)0U, Tint = new DoubleValue() { InnerText = "-4.9989318521683403E-2" } },
                                new BackgroundColor() { Indexed = (UInt32Value)64U }) { PatternType = PatternValues.Solid })) { Count = (UInt32Value)4U },
                    new Borders(
                        new Border(
                            new LeftBorder(),
                            new RightBorder(),
                            new TopBorder(),
                            new BottomBorder(),
                            new DiagonalBorder()),
                        new Border(
                            new LeftBorder(
                                new Color() { Theme = (UInt32Value)0U, Tint = new DoubleValue() { InnerText = "-0.14993743705557422" } }) { Style = BorderStyleValues.Thin },
                            new RightBorder(
                                new Color() { Theme = (UInt32Value)0U, Tint = new DoubleValue() { InnerText = "-0.14993743705557422" } }) { Style = BorderStyleValues.Thin },
                            new TopBorder(
                                new Color() { Theme = (UInt32Value)0U, Tint = new DoubleValue() { InnerText = "-0.14993743705557422" } }) { Style = BorderStyleValues.Thin },
                            new BottomBorder(
                                new Color() { Theme = (UInt32Value)0U, Tint = new DoubleValue() { InnerText = "-0.14993743705557422" } }) { Style = BorderStyleValues.Thin },
                            new DiagonalBorder()),
                        new Border(
                            new LeftBorder(
                                new Color() { Theme = (UInt32Value)0U, Tint = new DoubleValue() { InnerText = "-0.14993743705557422" } }) { Style = BorderStyleValues.Thin },
                            new RightBorder(
                                new Color() { Theme = (UInt32Value)0U, Tint = new DoubleValue() { InnerText = "-0.14993743705557422" } }) { Style = BorderStyleValues.Thin },
                            new TopBorder(
                                new Color() { Theme = (UInt32Value)0U, Tint = new DoubleValue() { InnerText = "-0.14993743705557422" } }) { Style = BorderStyleValues.Thin },
                            new BottomBorder(),
                            new DiagonalBorder()),
                        new Border(
                            new LeftBorder(
                                new Color() { Theme = (UInt32Value)0U, Tint = new DoubleValue() { InnerText = "-0.14993743705557422" } }) { Style = BorderStyleValues.Thin },
                            new RightBorder(
                                new Color() { Theme = (UInt32Value)0U, Tint = new DoubleValue() { InnerText = "-0.14993743705557422" } }) { Style = BorderStyleValues.Thin },
                            new TopBorder(),
                            new BottomBorder(),
                            new DiagonalBorder())) { Count = (UInt32Value)4U },
                    new CellStyleFormats(
                        new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U }) { Count = (UInt32Value)1U },
                    new CellFormats(
                        new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U },
                        new CellFormat(
                            new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Indent = (UInt32Value)1U }) { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)1U, FillId = (UInt32Value)2U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyFont = true, ApplyFill = true, ApplyBorder = true, ApplyAlignment = true },
                        new CellFormat(
                            new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Indent = (UInt32Value)1U }) { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)1U, FillId = (UInt32Value)2U, BorderId = (UInt32Value)2U, FormatId = (UInt32Value)0U, ApplyFont = true, ApplyFill = true, ApplyBorder = true, ApplyAlignment = true },
                        new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyBorder = true },
                        new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)2U, FormatId = (UInt32Value)0U, ApplyBorder = true },
                        new CellFormat() { NumberFormatId = (UInt32Value)14U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true, ApplyBorder = true },
                        new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)3U, BorderId = (UInt32Value)2U, FormatId = (UInt32Value)0U, ApplyFill = true, ApplyBorder = true },
                        new CellFormat() { NumberFormatId = (UInt32Value)14U, FontId = (UInt32Value)0U, FillId = (UInt32Value)3U, BorderId = (UInt32Value)2U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true, ApplyFill = true, ApplyBorder = true },
                        new CellFormat() { NumberFormatId = (UInt32Value)14U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)2U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true, ApplyBorder = true },
                        new CellFormat(
                            new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Indent = (UInt32Value)2U }) { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)1U, FillId = (UInt32Value)2U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyFont = true, ApplyFill = true, ApplyBorder = true, ApplyAlignment = true },
                        new CellFormat(
                            new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Indent = (UInt32Value)1U }) { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)3U, FormatId = (UInt32Value)0U, ApplyBorder = true, ApplyAlignment = true },
                        new CellFormat(
                            new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Indent = (UInt32Value)1U }) { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)2U, FormatId = (UInt32Value)0U, ApplyBorder = true, ApplyAlignment = true },
                        new CellFormat(
                            new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Indent = (UInt32Value)1U }) { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyBorder = true, ApplyAlignment = true },
                        new CellFormat(
                            new Alignment() { Horizontal = HorizontalAlignmentValues.Left, Indent = (UInt32Value)1U }) { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyAlignment = true },
                        new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)1U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)3U, FormatId = (UInt32Value)0U, ApplyFont = true, ApplyBorder = true },
                        new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)1U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)2U, FormatId = (UInt32Value)0U, ApplyFont = true, ApplyBorder = true },
                        new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)1U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)1U, FormatId = (UInt32Value)0U, ApplyFont = true, ApplyBorder = true },
                        new CellFormat() { NumberFormatId = (UInt32Value)0U, FontId = (UInt32Value)1U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyFont = true }) { Count = (UInt32Value)18U },
                    new CellStyles(
                        new CellStyle() { Name = "Normal", FormatId = (UInt32Value)0U, BuiltinId = (UInt32Value)0U }) { Count = (UInt32Value)1U },
                    new DifferentialFormats() { Count = (UInt32Value)0U },
                    new TableStyles() { Count = (UInt32Value)0U, DefaultTableStyle = "TableStyleMedium9", DefaultPivotStyle = "PivotStyleLight16" });
        }

        /// <summary>
        /// Retourne le nom de la colonne à partir de son index.
        /// </summary>
        /// <param name="colIndex">Index de la colonne.</param>
        /// <returns>Nom de la colonne.</returns>
        private static string ColumnFromIndex(uint colIndex) {
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
        /// Retourne le nombre de pixel utilisé pour l'affichage du texte.
        /// </summary>
        /// <param name="data">Texte à afficher.</param>
        /// <param name="fontTable">Table des largeurs.</param>
        /// <param name="offset">Offset pour le style.</param>
        /// <returns>Largeur d'affichage en pixels.</returns>
        private static int PixelWidthOf(string data, byte[] fontTable, int offset) {
            int width = offset;
            char[] characters = data.ToCharArray();
            byte maxWidth = fontTable[0];
            foreach (byte b in fontTable) {
                maxWidth = b > maxWidth ? b : maxWidth;
            }

            // Pour les caractères non-ASCII on prend la largeur maximum.
            foreach (char c in characters) {
                width += fontTable.Length > c ? fontTable[c] : maxWidth;
            }

            return width;
        }

        /// <summary>
        /// Retourne les ColWidth associé a un objet.
        /// </summary>
        /// <param name="sheet">Source de données.</param>
        /// <returns>Les largeurs de colonne adaptées.</returns>
        private static int[] ComputeColWidthForSingleObject(ExportSheet sheet) {
            int[] colWidth = new int[] { PixelWidthOf(sheet.Name, Calibri11, 16), 0 };
            foreach (ExportPropertyDefinition property in sheet.DisplayedProperties) {
                int descWidth = PixelWidthOf(property.PropertyLabel, Calibri11, 7);
                if (colWidth[0] < descWidth) {
                    colWidth[0] = descWidth;
                }

                BeanDefinition definition = BeanDescriptor.GetDefinition(sheet.DataSource);
                int valueWidth = PixelWidthOf(GetCellData(sheet.DataSource, property, definition).StringValue, Calibri11, 7);
                if (colWidth[1] < valueWidth) {
                    colWidth[1] = valueWidth;
                }
            }

            return colWidth;
        }

        /// <summary>
        /// Calcule les largeurs de colonne de la feuille Excel.
        /// </summary>
        /// <param name="sheet">Données de l'export.</param>
        /// <param name="definition">Definition.</param>
        /// <returns>Tableau contenant la taille des colonnes.</returns>
        private static int[] ComputeColWidthForCollection(ExportSheet sheet, out BeanDefinition definition) {
            int[] colWidth = new int[sheet.DisplayedProperties.Count];
            BeanPropertyDescriptor[] propDesc = new BeanPropertyDescriptor[sheet.DisplayedProperties.Count];

            uint currentCol = 'A';
            ICollection ds = (ICollection)sheet.DataSource;
            IEnumerator enumerator = ds.GetEnumerator();
            enumerator.MoveNext();
            object firstItem = enumerator.Current;
            definition = BeanDescriptor.GetDefinition(firstItem);

            int i = 0;
            foreach (ExportPropertyDefinition property in sheet.DisplayedProperties) {
                if (property.PropertyPath.IndexOf('.') == -1) {
                    propDesc[i] = GetPropertyDescriptor(firstItem, property.PropertyPath, definition);
                }

                string description;
                if (!string.IsNullOrEmpty(property.PropertyLabel)) {
                    description = property.PropertyLabel;
                } else if (propDesc[i] != null) {
                    description = propDesc[i].Description;
                } else {
                    description = GetPropertyDescriptor(firstItem, property.PropertyPath, definition).Description;
                }

                int width = PixelWidthOf(description, Calibri11, 16);
                if (colWidth[currentCol - 'A'] < width) {
                    colWidth[currentCol - 'A'] = width;
                }

                ++currentCol;
                ++i;
            }

            foreach (object item in (ICollection)sheet.DataSource) {
                currentCol = 'A';
                i = 0;
                foreach (ExportPropertyDefinition propertyDefinition in sheet.DisplayedProperties) {
                    CellData cellData = GetCellData(item, propertyDefinition, definition);
                    string strValue = cellData.StringValue;
                    int width = PixelWidthOf(strValue, Calibri11, 7);
                    if (colWidth[currentCol - 'A'] < width) {
                        colWidth[currentCol - 'A'] = width;
                    }

                    ++currentCol;
                    ++i;
                }
            }

            return colWidth;
        }

        /// <summary>
        /// Crée les colonnes de représentation d'un objet.
        /// </summary>
        /// <param name="colWidth">Largeur des colonnes.</param>
        /// <returns>La définition des colonnes.</returns>
        private static Columns CreateSingleObjectColumns(int[] colWidth) {
            return new Columns(
                            new Column {
                                Min = (UInt32Value)1U,
                                Max = (UInt32Value)1U,
                                Width = new DoubleValue() {
                                    InnerText = "5.7109375"
                                },
                                CustomWidth = true
                            },
                            new Column {
                                Min = (UInt32Value)2U,
                                Max = (UInt32Value)2U,
                                Width = new DoubleValue() {
                                    InnerText = ((double)(colWidth[0] * 256 / 7) / 256).ToString(CultureInfo.InvariantCulture)
                                },
                                Style = (UInt32Value)13U,
                                BestFit = true,
                                CustomWidth = true
                            },
                            new Column {
                                Min = (UInt32Value)3U,
                                Max = (UInt32Value)3U,
                                Width = new DoubleValue() {
                                    InnerText = ((double)(colWidth[1] * 256 / 7) / 256).ToString(CultureInfo.InvariantCulture)
                                },
                                Style = (UInt32Value)17U,
                                BestFit = true,
                                CustomWidth = true
                            });
        }

        /// <summary>
        /// Retourne le descripteur d'une propriété.
        /// </summary>
        /// <param name="item">DataSource.</param>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <param name="definition">Définition du bean.</param>
        /// <returns>Descripteur de la propriété.</returns>
        private static BeanPropertyDescriptor GetPropertyDescriptor(object item, string propertyName, BeanDefinition definition) {
            if (propertyName.IndexOf('.') == -1) {
                return definition.Properties[propertyName];
            }

            string[] splitedPath = propertyName.Split('.');
            if (splitedPath.Length != 2) {
                throw new NotSupportedException("Export only supports one level of composition");
            }

            object composedValue = definition.Properties[splitedPath[0]].GetValue(item);
            BeanDefinition propDefinition = BeanDescriptor.GetDefinition(composedValue);
            return propDefinition.Properties[splitedPath[1]];
        }

        /// <summary>
        /// Obtient les données sur une valeur de cellule.
        /// </summary>
        /// <param name="item">Datasource de la cellule.</param>
        /// <param name="propertyDefinition">Définition de la propriété d'export.</param>
        /// <param name="definition">Définition du bean.</param>
        /// <returns>Données sur la valeur de la cellule.</returns>
        private static CellData GetCellData(object item, ExportPropertyDefinition propertyDefinition, BeanDefinition definition) {
            BeanPropertyDescriptor propDesc;
            string propertyName = propertyDefinition.PropertyPath;
            string defaultPropertyName = propertyDefinition.DefaultPropertyName;
            object dataSource = propertyDefinition.DataSource;
            if (propertyName.IndexOf('.') == -1) {
                propDesc = definition.Properties[propertyName];
                return new CellData {
                    Descriptor = propDesc,
                    Value = propDesc.GetValue(item),
                    StringValue = GetTranslatedString(propDesc, propDesc.GetValue(item), dataSource, defaultPropertyName)
                };
            }

            string[] splitedPath = propertyName.Split('.');
            if (splitedPath.Length != 2) {
                throw new NotSupportedException("Export only supports one level of composition");
            }

            object composedValue = definition.Properties[splitedPath[0]].GetValue(item);
            BeanDefinition propDefinition = BeanDescriptor.GetDefinition(composedValue);
            propDesc = propDefinition.Properties[splitedPath[1]];
            return new CellData {
                Descriptor = propDesc,
                Value = propDesc.GetValue(composedValue),
                StringValue = GetTranslatedString(propDesc, propDesc.GetValue(composedValue), dataSource, defaultPropertyName)
            };
        }

        /// <summary>
        /// Retourne la chaine de caractères correspondant à l'évaluation d'une propriété.
        /// </summary>
        /// <param name="property">Propriété utilisée.</param>
        /// <param name="value">Valeur de la propriété.</param>
        /// <param name="dataSource">Datasource facultative pour une propriété de type référence.</param>
        /// <param name="defaultPropertyName">Nom de la propriété par défaut invoquée si la propriété est une association.</param>
        /// <returns>Chaine de caractères.</returns>
        /// <todo who="SEY" type="IGNORE">Corriger la gestion des propriétés de type IList[string].</todo>
        private static string GetTranslatedString(BeanPropertyDescriptor property, object value, object dataSource, string defaultPropertyName = null) {
            if (property.ReferenceType != null && value != null) {
                if (dataSource != null) {
                    return ReferenceManager.Instance.GetReferenceValueByPrimaryKey(property.ReferenceType, value, dataSource, defaultPropertyName);
                }

                return ServiceManager.Instance.GetDefaultValueByPrimaryKey(property.ReferenceType, value, defaultPropertyName);
            }

            if (property.PrimitiveType != null) {
                if (property.DomainName == "DO_MOIS" && value != null) {
                    return Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetMonthName((short)value);
                }

                if (property.DomainName == "DO_DATE_TIME" && value != null) {
                    return ((DateTime)value).ToString(Thread.CurrentThread.CurrentCulture);
                }

                return property.ConvertToString(value) ?? string.Empty;
            }

            if (property.PropertyType == typeof(IList<string>)) {
                return string.Join(", ", (IList<string>)value);
            }

            if (property.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)) {
                return GetConcatenationValue((ICollection)value);
            }

            return string.Empty;
        }

        /// <summary>
        /// Renvoie la liste concatanée des valeurs par défauts des objets sélectionnés dans une liste d'objets.
        /// </summary>
        /// <param name="selectedCollection">Liste des objets sélectionnés.</param>
        /// <returns>Chaîne de caractères représentant la liste.</returns>
        /// <todo who="SEY" type="IGNORE">A mutualiser avec le CheckBoxListAdapter.</todo>
        private static string GetConcatenationValue(ICollection selectedCollection) {
            ICollection dataSource = selectedCollection;
            BeanDefinition refDefinition = BeanDescriptor.GetCollectionDefinition(dataSource);
            Hashtable index = CreateSelectedIndex(selectedCollection);
            BeanPropertyDescriptor primaryKey = refDefinition.PrimaryKey;
            BeanPropertyDescriptor defaultProperty = refDefinition.DefaultProperty;
            List<string> valueList = new List<string>();
            foreach (object item in dataSource) {
                object pk = primaryKey.GetValue(item);
                if (index.ContainsKey(pk)) {
                    valueList.Add(defaultProperty.ConvertToString(defaultProperty.GetValue(item)));
                }
            }

            return string.Join(", ", valueList);
        }

        /// <summary>
        /// Création d'un index des valeurs sélectionnées.
        /// </summary>
        /// <param name="selectedCollection">Liste des éléments sélectionnés.</param>
        /// <returns>Liste des élements sélectionnés.</returns>
        private static Hashtable CreateSelectedIndex(ICollection selectedCollection) {
            Hashtable index = new Hashtable();
            BeanPropertyDescriptor selectedPrimaryKey = BeanDescriptor.GetCollectionDefinition(selectedCollection).PrimaryKey;
            foreach (object selected in selectedCollection) {
                IBeanState beanState = selected as IBeanState;
                if (beanState != null && beanState.State == ChangeAction.Delete) {
                    continue;
                }

                object pk = selectedPrimaryKey.GetValue(selected);
                index.Add(pk, pk);
            }

            return index;
        }

        /// <summary>
        /// Crée le workbook du document.
        /// </summary>
        /// <param name="exportSheetList">Liste des feuilles de données.</param>
        /// <returns>Workbook.</returns>
        private static Workbook GenerateWorkbookPart(ICollection<ExportSheet> exportSheetList) {
            Workbook workbook = new Workbook();

            workbook.AppendChild<FileVersion>(new FileVersion { ApplicationName = "xl", LastEdited = "4", LowestEdited = "4", BuildVersion = "4506" });
            workbook.AppendChild<WorkbookProperties>(new WorkbookProperties { DefaultThemeVersion = (UInt32Value)124226U });
            workbook.AppendChild<BookViews>(new BookViews(
                        new WorkbookView { XWindow = 120, YWindow = 60, WindowWidth = (UInt32Value)15120U, WindowHeight = (UInt32Value)11070U }));

            Sheets sheets = new Sheets();
            uint i = 1;
            foreach (ExportSheet sheet in exportSheetList) {
                sheets.AppendChild<Sheet>(new Sheet { Name = sheet.Name, SheetId = UInt32Value.FromUInt32(i), Id = "rId" + i });
                ++i;
            }

#if FONTTABLE
            ++i;
            sheets.AppendChild<Sheet>(new Sheet { Name = "test", SheetId = UInt32Value.FromUInt32(i), Id = "rId" + i });
#endif
            workbook.AppendChild<Sheets>(sheets);
            workbook.AppendChild<CalculationProperties>(new CalculationProperties { CalculationId = (UInt32Value)125725U });

            return workbook;
        }

        /// <summary>
        /// Crée la feuille de données dans le classeur.
        /// </summary>
        /// <param name="sheet">Définition de la feuille de données.</param>
        /// <returns>La feuille de données instanciée.</returns>
        private Worksheet CreateSheet(ExportSheet sheet) {
            return sheet.IsCollection ? CreateCollectionSheet(sheet) : CreateObjectSheet(sheet);
        }

        /// <summary>
        /// Créé une feuille contenant les données de l'export.
        /// </summary>
        /// <param name="sheet">Les données de l'export.</param>
        /// <returns>Feuille des données.</returns>
        private Worksheet CreateCollectionSheet(ExportSheet sheet) {
            Worksheet worksheet = new Worksheet();
            worksheet.AppendChild<SheetViews>(new SheetViews(new SheetView { ShowGridLines = false, TabSelected = true, WorkbookViewId = (UInt32Value)0U }));
            worksheet.AppendChild<SheetFormatProperties>(new SheetFormatProperties { BaseColumnWidth = (UInt32Value)10U, DefaultColumnWidth = new DoubleValue() { InnerText = "9.140625" }, DefaultRowHeight = 15D });
            SheetData sheetData = new SheetData();

            BeanDefinition definition;
            int[] colWidth = ComputeColWidthForCollection(sheet, out definition);

            worksheet.InsertAt<SheetDimension>(new SheetDimension { Reference = "A1:" + ColumnFromIndex('A' + (uint)sheet.DisplayedProperties.Count - 1) + ((ICollection)sheet.DataSource).Count }, 0);
            worksheet.AppendChild<SheetData>(sheetData);
            worksheet.AppendChild<PageMargins>(new PageMargins { Left = new DoubleValue() { InnerText = "0.7" }, Right = new DoubleValue() { InnerText = "0.7" }, Top = new DoubleValue() { InnerText = "0.75" }, Bottom = new DoubleValue() { InnerText = "0.75" }, Header = new DoubleValue() { InnerText = "0.3" }, Footer = new DoubleValue() { InnerText = "0.3" } });

            Tuple<Columns, Row> headerDefinition = CreateCollectionHeaderDefinition(sheet, colWidth, definition);
            worksheet.InsertAt<Columns>(headerDefinition.Item1, 3);
            sheetData.InsertAt<Row>(headerDefinition.Item2, 0);

            AddItems(sheet, sheetData, definition);
            return worksheet;
        }

        /// <summary>
        /// Ajoute les éléments de la collection dans la feuille de données.
        /// </summary>
        /// <param name="sheet">Données exportées.</param>
        /// <param name="sheetData">Feuille de données.</param>
        /// <param name="definition">Définition du bean.</param>
        private void AddItems(ExportSheet sheet, SheetData sheetData, BeanDefinition definition) {
            BeanPropertyDescriptor[] propDesc = new BeanPropertyDescriptor[sheet.DisplayedProperties.Count];
            for (int i = 0; i < propDesc.Length; ++i) {
                if (sheet.DisplayedProperties[i].PropertyPath.IndexOf('.') == -1) {
                    propDesc[i] = BeanDescriptor.GetCollectionDefinition(sheet.DataSource).Properties[sheet.DisplayedProperties[i].PropertyPath];
                }
            }

            uint currentRow = 2;
            foreach (object item in (ICollection)sheet.DataSource) {
                Row row = new Row {
                    RowIndex = (UInt32Value)currentRow,
                    Spans = new ListValue<StringValue> {
                        InnerText = "1:" + sheet.DisplayedProperties.Count
                    }
                };
                uint currentCol = 'A';
                int i = 0;
                foreach (ExportPropertyDefinition propertyDefinition in sheet.DisplayedProperties) {
                    CellData cellData = GetCellData(item, propertyDefinition, definition);
                    row.AppendChild<Cell>(this.CreateCell(currentCol, currentRow, cellData, (currentRow % 2 == 0) ? 4U : 6U));
                    ++currentCol;
                    ++i;
                }

                sheetData.AppendChild<Row>(row);
                ++currentRow;
            }
        }

        /// <summary>
        /// Renvoie la définition de la ligne d'entête du tableau.
        /// </summary>
        /// <param name="sheet">Données de l'export.</param>
        /// <param name="colWidth">Largeur précaclulées des colonnes.</param>
        /// <param name="definition">Définition du bean.</param>
        /// <returns>Un tuple contenant colonnes à injecter et ligne.</returns>
        private Tuple<Columns, Row> CreateCollectionHeaderDefinition(ExportSheet sheet, int[] colWidth, BeanDefinition definition) {
            Columns columns = new Columns();
            Row headerRow = new Row {
                RowIndex = (UInt32Value)1,
                Spans = new ListValue<StringValue> {
                    InnerText = "1:" + sheet.DisplayedProperties.Count
                }
            };
            uint currentCol = 'A';
            ICollection ds = (ICollection)sheet.DataSource;
            IEnumerator enumerator = ds.GetEnumerator();
            enumerator.MoveNext();
            object firstItem = enumerator.Current;

            foreach (ExportPropertyDefinition propertyDefinition in sheet.DisplayedProperties) {
                string columnName;
                if (!string.IsNullOrEmpty(propertyDefinition.PropertyLabel)) {
                    columnName = propertyDefinition.PropertyLabel;
                } else {
                    columnName = GetPropertyDescriptor(firstItem, propertyDefinition.PropertyPath, definition).Description;
                }

                columns.AppendChild<Column>(new Column {
                    Min = (UInt32Value)(currentCol - 'A' + 1),
                    Max = (UInt32Value)(currentCol - 'A' + 1),
                    Width = new DoubleValue() { InnerText = ((double)(colWidth[currentCol - 'A'] * 256 / 7) / 256).ToString(CultureInfo.InvariantCulture) },
                    Style = (UInt32Value)13U,
                    BestFit = true,
                    CustomWidth = true
                });
                headerRow.AppendChild<Cell>(this.CreateCell(currentCol, 1, columnName, 2));
                ++currentCol;
            }

            return new Tuple<Columns, Row>(columns, headerRow);
        }

        /// <summary>
        /// Créé une feuille de données contenant un objet.
        /// </summary>
        /// <param name="sheet">Données de la feuille.</param>
        /// <returns>Feuille de données instanciée.</returns>
        private Worksheet CreateObjectSheet(ExportSheet sheet) {
            Worksheet worksheet = new Worksheet();
            worksheet.AppendChild<SheetViews>(new SheetViews(new SheetView() { ShowGridLines = false, WorkbookViewId = (UInt32Value)0U }));
            worksheet.AppendChild<SheetFormatProperties>(new SheetFormatProperties() { BaseColumnWidth = (UInt32Value)10U, DefaultColumnWidth = new DoubleValue() { InnerText = "9.140625" }, DefaultRowHeight = 15D });
            SheetData sheetData = new SheetData();
            sheetData.AppendChild<Row>(new Row { RowIndex = (UInt32Value)1U, Spans = new ListValue<StringValue>() { InnerText = "2:3" }, Height = 15D, CustomHeight = true });
            sheetData.AppendChild<Row>(new Row(this.CreateCell('B', 2, sheet.Name, 1), new Cell { CellReference = "C2", StyleIndex = (UInt32Value)9U }) { RowIndex = (UInt32Value)2U, Spans = new ListValue<StringValue>() { InnerText = "2:3" } });

            worksheet.InsertAt<SheetDimension>(new SheetDimension { Reference = "B1:C" + (sheet.DisplayedProperties.Count + 3) }, 0);
            worksheet.AppendChild<SheetData>(sheetData);
            worksheet.AppendChild<PageMargins>(new PageMargins { Left = new DoubleValue { InnerText = "0.7" }, Right = new DoubleValue { InnerText = "0.7" }, Top = new DoubleValue { InnerText = "0.75" }, Bottom = new DoubleValue { InnerText = "0.75" }, Header = new DoubleValue { InnerText = "0.3" }, Footer = new DoubleValue { InnerText = "0.3" } });

            int[] colWidth = ComputeColWidthForSingleObject(sheet);
            worksheet.InsertAt<Columns>(CreateSingleObjectColumns(colWidth), 3);

            uint currentRow = 3;
            BeanDefinition definition = BeanDescriptor.GetDefinition(sheet.DataSource);
            foreach (ExportPropertyDefinition propertyDefinition in sheet.DisplayedProperties) {
                Row row = new Row() { RowIndex = (UInt32Value)currentRow, Spans = new ListValue<StringValue>() { InnerText = "2:3" } };
                CellData cellData = GetCellData(sheet.DataSource, propertyDefinition, definition);
                string description = propertyDefinition.PropertyLabel;
                if (string.IsNullOrEmpty(description)) {
                    description = cellData.Descriptor.Description;
                }

                row.AppendChild<Cell>(this.CreateCell('B', currentRow, description, 11));
                row.AppendChild<Cell>(this.CreateCell('C', currentRow, cellData, 15));
                sheetData.AppendChild<Row>(row);
                ++currentRow;
            }

            return worksheet;
        }

#if FONTTABLE
        /// <summary>
        /// Créé une feuille contenant les critères de l'export.
        /// </summary>
        /// <returns>Feuille des critères</returns>
        private Worksheet CreateTestSheet() {
            Worksheet worksheet = new Worksheet();
            worksheet.AppendChild<SheetViews>(new SheetViews(new SheetView() { ShowGridLines = false, WorkbookViewId = (UInt32Value)0U }));
            worksheet.AppendChild<SheetFormatProperties>(new SheetFormatProperties() { BaseColumnWidth = (UInt32Value)10U, DefaultColumnWidth = new DoubleValue() { InnerText = "9.140625" }, DefaultRowHeight = 15D });

            SheetData sheetData = new SheetData();

            StringBuilder reference = new StringBuilder("A1:" + ColumnFromIndex(255) + "1");

            Row row = new Row() { RowIndex = (UInt32Value)1U, Spans = new ListValue<StringValue>() { InnerText = "2:3" } };

            uint currentCell = 'A';

            for (int i = 1; i < 256; ++i) {
                if (i >= 32) {
                    row.AppendChild<Cell>(this.CreateCell(currentCell, 1, new string((char)i, 2), 2));
                } else {
                    row.AppendChild<Cell>(this.CreateCell(currentCell, 1, String.Empty, 2));
                }
                ++currentCell;
            }
            sheetData.AppendChild<Row>(row);

            worksheet.InsertAt<SheetDimension>(new SheetDimension() { Reference = reference.ToString() }, 0);
            worksheet.AppendChild<SheetData>(sheetData);
            worksheet.AppendChild<PageMargins>(new PageMargins() { Left = new DoubleValue() { InnerText = "0.7" }, Right = new DoubleValue() { InnerText = "0.7" }, Top = new DoubleValue() { InnerText = "0.75" }, Bottom = new DoubleValue() { InnerText = "0.75" }, Header = new DoubleValue() { InnerText = "0.3" }, Footer = new DoubleValue() { InnerText = "0.3" } });

            return worksheet;
        }
#endif

        /// <summary>
        /// Crée une cellule de type sharedString.
        /// </summary>
        /// <param name="col">Colonne.</param>
        /// <param name="row">Ligne.</param>
        /// <param name="cellData">Données sur la valeur de la cellule.</param>
        /// <param name="styleIndex">Index du style à utiliser.</param>
        /// <returns>Cellule.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "A un sens ici.")]
        private Cell CreateCell(uint col, uint row, CellData cellData, uint styleIndex) {
            BeanPropertyDescriptor property = cellData.Descriptor;
            if (property.ReferenceType == null) {
                if (property.PrimitiveType == typeof(int) || property.PrimitiveType == typeof(decimal) || property.PrimitiveType == typeof(float)) {
                    string numericValue = cellData.StringValue.Replace(" ", string.Empty).Replace(",", ".");
                    return new Cell(new CellValue(numericValue)) {
                        CellReference = ColumnFromIndex(col) + row.ToString(CultureInfo.InvariantCulture),
                        StyleIndex = (UInt32Value)styleIndex
                    };
                }
            }

            if (property.PrimitiveType == typeof(DateTime) && property.DomainName != "DO_DATE_TIME") {
                string d = string.Empty;
                if (cellData.Value != null) {
                    DateTime date = (DateTime)cellData.Value;
                    d = date.Subtract(new DateTime(1899, 12, 30)).Days.ToString(CultureInfo.InvariantCulture);
                }

                return new Cell(new CellValue(d)) {
                    CellReference = ColumnFromIndex(col) + row.ToString(CultureInfo.InvariantCulture),
                    StyleIndex = (UInt32Value)((styleIndex == 6) ? 7U : 8U)
                };
            }

            return CreateCell(col, row, cellData.StringValue, styleIndex);
        }

        /// <summary>
        /// Crée une cellule de type sharedString.
        /// </summary>
        /// <param name="col">Colonne.</param>
        /// <param name="row">Ligne.</param>
        /// <param name="shareStringValue">Valeur de la string.</param>
        /// <param name="styleIndex">Index du style à utiliser.</param>
        /// <returns>Cellule.</returns>
        private Cell CreateCell(uint col, uint row, string shareStringValue, uint styleIndex) {
            string cellReference = ColumnFromIndex(col) + row.ToString(CultureInfo.InvariantCulture);
            string shareStringIndex = _sharedStringIndex.ToString(CultureInfo.InvariantCulture);
            ++_sharedStringIndex;
            _shareStringTable.AppendChild<SharedStringItem>(new SharedStringItem(new Text(shareStringValue)));
            return new Cell(new CellValue(shareStringIndex)) {
                CellReference = cellReference,
                StyleIndex = (UInt32Value)styleIndex,
                DataType = CellValues.SharedString
            };
        }

        /// <summary>
        /// Valide le document Excel.
        /// </summary>
        private void ValidateDocument() {
            OpenXmlValidator validator = new OpenXmlValidator(FileFormatVersions.Office2007);
            IEnumerable<ValidationErrorInfo> errors = validator.Validate(_excelDocument);
            ILog log = LogManager.GetLogger("Service");
            foreach (ValidationErrorInfo error in errors) {
                log.Error("Erreur lors dans un document Excel généré : " + error.Description);
            }
        }

        /// <summary>
        /// Encapsule une valeur de cellule, avec son descripteur et sa conversion en chaîne de caractère.
        /// </summary>
        private class CellData {

            /// <summary>
            /// Descripteur de la propriété correspondant à la valeur de la cellule.
            /// </summary>
            public BeanPropertyDescriptor Descriptor {
                get;
                set;
            }

            /// <summary>
            /// Valeur de la cellule, sans conversion.
            /// </summary>
            public object Value {
                get;
                set;
            }

            /// <summary>
            /// Valeur de la cellule convertie en chaîne de caractères.
            /// </summary>
            public string StringValue {
                get;
                set;
            }
        }
    }
}
