using System;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using D = DocumentFormat.OpenXml.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Validation;
using Resx = Kinetix.Reporting.Test.Resources.Resource;
using System.IO;
using System.Drawing;
using Kinetix.Reporting;

namespace Kinetix.Reporting.Test {
    /// <summary>
    /// Classe de test du helper Powerpoint.
    /// </summary>
    [TestClass]
    public class PowerpointHelperTest {

        private static readonly OpenXmlValidator validator = new OpenXmlValidator();
            
        #region CreatePresentationWithOneEmptySlide

        /// <summary>
        /// Test la création d'une présentation.
        /// </summary>
        [TestMethod]
        public void CreatePresentationWithOneEmptySlide_Valid() {
            PresentationDocument pres = PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFile0.pptx");
            Assert.IsTrue(pres.PresentationPart.SlideParts.Count() > 0);
        }

        /// <summary>
        /// Test la création d'une présentation avec des arguments personnalisés.
        /// </summary>
        [TestMethod]
        public void CreatePresentationWithOneEmptySlide_CustomAttributes_Valid() {
            PresentationDocument pres = PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFile1.pptx", 9989900, 998990);
            Assert.IsTrue(pres.PresentationPart.SlideParts.Count() > 0);
        }

        /// <summary>
        /// Test la création d'une présentation sans fichier.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreatePresentationWithOneEmptySlide_InvalidFile_Throws() {
            PowerpointHelper.CreatePresentationWithOneEmptySlide(null);
        }

        /// <summary>
        /// Test la création d'une présentation avec une largeur incorrecte.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreatePresentationWithOneEmptySlide_NegativeWidth_Throws() {
            PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFileP1.pptx", -9989900, 9989900);
        }

        /// <summary>
        /// Test la création d'une présentation avec une largeur incorrecte.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreatePresentationWithOneEmptySlide_LowWidth_Throws() {
            PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFileP2.pptx", 900, 9989900);
        }

        /// <summary>
        /// Test la création d'une présentation avec une hauteur incorrecte.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreatePresentationWithOneEmptySlide_NegativeHeight_Throws() {
            PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFileP3.pptx", 9989900, -9989900);
        }

        /// <summary>
        /// Test la création d'une présentation avec une hauteur incorrecte.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreatePresentationWithOneEmptySlide_LowHeight_Throws() {
            PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFileP4.pptx", 9989900, 900);
        }

        #endregion

        #region GetShapeTreeOfFirstSlide

        /// <summary>
        /// Test l'obtention des éléments de la 1ère slide d'une présentation.
        /// </summary>
        [TestMethod]
        public void GetShapeTreeOfFirstSlide_Valid() {
            PresentationDocument pres = PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFile2.pptx");

            ShapeTree shapeTree = PowerpointHelper.GetShapeTreeOfFirstSlide(pres);
        }

        #endregion

        #region CreatePolygon

        /// <summary>
        /// Test la création d'un polygon.
        /// </summary>
        [TestMethod]
        public void CreatePolygon_Valid() {
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, 2 });
            list.Add(new int[] { 3, 4 });
            list.Add(new int[] { 5, 6 });

            Shape polygon = PowerpointHelper.CreatePolygon(list);
        }

        /// <summary>
        /// Test la création d'un polygon avec des arguments custom.
        /// </summary>
        [TestMethod]
        public void CreatePolygon_CustomAttributes_Valid() {
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, 2 });
            list.Add(new int[] { 3, 4 });
            list.Add(new int[] { 5, 6 });

            Shape polygon = PowerpointHelper.CreatePolygon(list, "AFAFAF");
        }

        /// <summary>
        /// Test la création d'un polygon avec une couleur non valide.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreatePolygon_InvalidColor_Throws() {
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, 2 });
            list.Add(new int[] { 3, 4 });
            list.Add(new int[] { 5, 6 });

            Shape polygon = PowerpointHelper.CreatePolygon(list, "invalid");
        }

        /// <summary>
        /// Test la création d'un polygon avec une liste de points vide.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreatePolygon_EmptyList_Throws() {
            List<int[]> list = new List<int[]>();

            Shape polygon = PowerpointHelper.CreatePolygon(list);
        }

        /// <summary>
        /// Test la création d'un polygon avec une liste non valide.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreatePolygon_InvalidList_Throws() {
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, 2 });
            list.Add(new int[] { 3, 4, 7 });
            list.Add(new int[] { 5, 6 });

            Shape polygon = PowerpointHelper.CreatePolygon(list);
        }

        #endregion
        
        #region AddElement

        /// <summary>
        /// Test l'ajout d'un élément à un autre élément.
        /// </summary>
        [TestMethod]
        public void AddElement_Valid() {
            PresentationDocument pres = PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFile3.pptx");
            ShapeTree shapeTree = PowerpointHelper.GetShapeTreeOfFirstSlide(pres);
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, 2 });
            list.Add(new int[] { 3, 4 });
            list.Add(new int[] { 5, 6 });
            Shape polygon = PowerpointHelper.CreatePolygon(list);

            PowerpointHelper.AddElement(shapeTree, polygon);

            var errors = validator.Validate(pres);
            Assert.IsTrue(errors.Count() == 0);
            Assert.IsTrue(shapeTree.Count() > 0);
        }

        #endregion

        #region AddParagraph

        /// <summary>
        /// Test l'ajout d'un paragraphe à un autre élément.
        /// </summary>
        [TestMethod]
        public void AddParagraph_Valid() {
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, 2 });
            list.Add(new int[] { 3, 4 });
            list.Add(new int[] { 5, 6 });
            Shape polygon = PowerpointHelper.CreatePolygon(list);

            int oldNbParagraph = polygon.Descendants<D.Paragraph>().Count();
            PowerpointHelper.AddParagraph(polygon, "text");

            var errors = validator.Validate(polygon);
            Assert.IsTrue(errors.Count() == 0);
            Assert.IsTrue(polygon.Descendants<D.Paragraph>().Count() - oldNbParagraph == 1);
        }

        /// <summary>
        /// Test l'ajout d'un paragraphe avec des arguments custom.
        /// </summary>
        [TestMethod]
        public void AddParagraph_CustomAttributes_Valid() {
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, 2 });
            list.Add(new int[] { 3, 4 });
            list.Add(new int[] { 5, 6 });
            Shape polygon = PowerpointHelper.CreatePolygon(list);

            int oldNbParagraph = polygon.Descendants<D.Paragraph>().Count();
            PowerpointHelper.AddParagraph(polygon, "text", 400, "ABABAB", true, D.TextAlignmentTypeValues.Right);

            var errors = validator.Validate(polygon);
            Assert.IsTrue(errors.Count() == 0);
            Assert.IsTrue(polygon.Descendants<D.Paragraph>().Count() - oldNbParagraph == 1);
        }

        /// <summary>
        /// Test l'ajout d'un paragraphe avec une font size nulle.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddParagraph_ZeroFontSize_Throws() {
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, 2 });
            list.Add(new int[] { 3, 4 });
            list.Add(new int[] { 5, 6 });
            Shape polygon = PowerpointHelper.CreatePolygon(list);

            int oldNbParagraph = polygon.Descendants<D.Paragraph>().Count();
            PowerpointHelper.AddParagraph(polygon, "text", 0);
        }

        /// <summary>
        /// Test l'ajout d'un paragraphe avec une font size négative.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddParagraph_NegativeFontSize_Throws() {
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, 2 });
            list.Add(new int[] { 3, 4 });
            list.Add(new int[] { 5, 6 });
            Shape polygon = PowerpointHelper.CreatePolygon(list);

            int oldNbParagraph = polygon.Descendants<D.Paragraph>().Count();
            PowerpointHelper.AddParagraph(polygon, "text", -10);
        }

        /// <summary>
        /// Test l'ajout d'un paragraphe avec une couleur non valide.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddParagraph_InvalidColor_Throws() {
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, 2 });
            list.Add(new int[] { 3, 4 });
            list.Add(new int[] { 5, 6 });
            Shape polygon = PowerpointHelper.CreatePolygon(list);

            int oldNbParagraph = polygon.Descendants<D.Paragraph>().Count();
            PowerpointHelper.AddParagraph(polygon, "text", 10, "invalid");
        }

        #endregion
        
        #region AddCenteredParagraph
        /// <summary>
        /// Test l'ajout d'un paragraphe centré à un autre élément.
        /// </summary>
        [TestMethod]
        public void AddCenteredParagraph_Valid() {
            List<int[]> list = new List<int[]>();
            list.Add(new int[] { 1, 2 });
            list.Add(new int[] { 3, 4 });
            list.Add(new int[] { 5, 6 });
            Shape polygon = PowerpointHelper.CreatePolygon(list);

            int oldNbParagraph = polygon.Descendants<D.Paragraph>().Count();
            PowerpointHelper.AddCenteredParagraph(polygon, "text");

            var errors = validator.Validate(polygon);
            Assert.IsTrue(errors.Count() == 0);
            Assert.IsTrue(polygon.Descendants<D.Paragraph>().Count() - oldNbParagraph == 1);
        }

        #endregion

        #region CreateRectangleShape

        /// <summary>
        /// Test la création d'un rectangle.
        /// </summary>
        [TestMethod]
        public void CreateRectangleShape_Valid() {
            Shape rectangle = PowerpointHelper.CreateRectangleShape(50,50,50,50);
            
            PowerpointHelper.AddParagraph(rectangle, "text");
            
            var errors = validator.Validate(rectangle);
            Assert.IsTrue(errors.Count() == 0);
        }

        /// <summary>
        /// Test la création d'un rectangle avec des arguments custom.
        /// </summary>
        [TestMethod]
        public void CreateRectangleShape_CustomAttributes_Valid() {
            Shape rectangle = PowerpointHelper.CreateRectangleShape(50,50,50,50, "AFAFAF", D.TextAnchoringTypeValues.Bottom, D.TextWrappingValues.None);
            
            PowerpointHelper.AddParagraph(rectangle, "text");
            
            var errors = validator.Validate(rectangle);
            Assert.IsTrue(errors.Count() == 0);
        }

        /// <summary>
        /// Test la création d'un rectangle avec des valeurs négatives et nulles.
        /// </summary>
        [TestMethod]
        public void CreateRectangle_ZerosValues_Valid() {
            Shape rectangle = PowerpointHelper.CreateRectangleShape(-10, -10, 0, 0);

            PowerpointHelper.AddParagraph(rectangle, "text");

            var errors = validator.Validate(rectangle);
            Assert.IsTrue(errors.Count() == 0);
        }

        /// <summary>
        /// Test la création d'un rectangle avec une couleur non valide.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateRectangle_InvalidColor_Throws() {
            PowerpointHelper.CreateRectangleShape(50, 50, 50, 50, "invalid");
        }

        /// <summary>
        /// Test la création d'un rectangle avec une largeur négative.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreateRectangle_NegativeWidth_Throws() {
            PowerpointHelper.CreateRectangleShape(50, 50, -100, 50);
        }

        /// <summary>
        /// Test la création d'un rectangle avec une hauteur négative.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreateRectangle_NegativeHeight_Throws() {
            PowerpointHelper.CreateRectangleShape(50, 50, 50, -100);
        }

        #endregion

        #region AddTextRun

        /// <summary>
        /// Test l'ajout d'un morceau de texte à un paragraphe.
        /// </summary>
        [TestMethod]
        public void AddTextRun_Valid() {
            Shape rectangle = PowerpointHelper.CreateRectangleShape(50,50,50,50);
            D.Paragraph paragraph = PowerpointHelper.AddParagraph(rectangle, "text");

            int oldNbTextRun = paragraph.Descendants<D.Run>().Count();

            PowerpointHelper.AddTextRun(paragraph, "text");

            var errors = validator.Validate(paragraph);
            Assert.IsTrue(errors.Count() == 0);
            Assert.IsTrue(paragraph.Descendants<D.Run>().Count() - oldNbTextRun == 1);
        }

        /// <summary>
        /// Test l'ajout d'un morceau de texte avec des arguments custom.
        /// </summary>
        [TestMethod]
        public void AddTextRun_CustomAttributes_Valid() {
            Shape rectangle = PowerpointHelper.CreateRectangleShape(50, 50, 50, 50);
            D.Paragraph paragraph = PowerpointHelper.AddParagraph(rectangle, "text");

            int oldNbTextRun = paragraph.Descendants<D.Run>().Count();

            PowerpointHelper.AddTextRun(paragraph, "text", 400, "487315", true);

            var errors = validator.Validate(paragraph);
            Assert.IsTrue(errors.Count() == 0);
            Assert.IsTrue(paragraph.Descendants<D.Run>().Count() - oldNbTextRun == 1);
        }

        /// <summary>
        /// Test l'ajout d'un morceau de texte avec une font size nulle.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddTextRun_ZeroFontSize_Throws() {
            Shape rectangle = PowerpointHelper.CreateRectangleShape(50, 50, 50, 50);
            D.Paragraph paragraph = PowerpointHelper.AddParagraph(rectangle, "text");

            int oldNbTextRun = paragraph.Descendants<D.Run>().Count();

            PowerpointHelper.AddTextRun(paragraph, "text", 0);
        }

        /// <summary>
        /// Test l'ajout d'un morceau de texte avec une font size négative.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddTextRun_NegativeFontSize_Throws() {
            Shape rectangle = PowerpointHelper.CreateRectangleShape(50, 50, 50, 50);
            D.Paragraph paragraph = PowerpointHelper.AddParagraph(rectangle, "text");

            int oldNbTextRun = paragraph.Descendants<D.Run>().Count();

            PowerpointHelper.AddTextRun(paragraph, "text", -100);
        }

        /// <summary>
        /// Test l'ajout d'un morceau de texte avec une couleur non valide.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddTextRun_InvalidColor_Throws() {
            Shape rectangle = PowerpointHelper.CreateRectangleShape(50, 50, 50, 50);
            D.Paragraph paragraph = PowerpointHelper.AddParagraph(rectangle, "text");

            int oldNbTextRun = paragraph.Descendants<D.Run>().Count();

            PowerpointHelper.AddTextRun(paragraph, "text", 100, "invalid");
        }

        #endregion

        #region CreateGroupShape

        /// <summary>
        /// Test la création d'un groupe.
        /// </summary>
        [TestMethod]
        public void CreateGroupShape_Valid() {
            GroupShape groupShape = PowerpointHelper.CreateGroupShape(50, 50, 50, 50);
        }

        /// <summary>
        /// Test la création d'un groupe avec des valeurs négatives et nulles.
        /// </summary>
        [TestMethod]
        public void CreateGroupShape_ZerosValues_Valid() {
            GroupShape groupShape = PowerpointHelper.CreateGroupShape(-10, -10, 0, 0);
        }

        /// <summary>
        /// Test la création d'un groupe avec une largeur négative.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreateGroupShape_NegativeWidth_Throws() {
            PowerpointHelper.CreateGroupShape(50, 50, -100, 50);
        }

        /// <summary>
        /// Test la création d'un groupe avec une hauteur négative.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CreateGroupShape_NegativeHeight_Throws() {
            PowerpointHelper.CreateGroupShape(50, 50, 50, -100);
        }

        #endregion

        #region ResizeGroup

        /// <summary>
        /// Test le redimensionnement d'un groupe.
        /// </summary>
        [TestMethod]
        public void ResizeGroup_Valid() {
            GroupShape groupShape = PowerpointHelper.CreateGroupShape(50, 50, 50, 50);

            PowerpointHelper.ResizeGroup(groupShape, 100, 10);
        }

        /// <summary>
        /// Test le redimensionnement d'un groupe avec des valeurs négatives et nulles.
        /// </summary>
        [TestMethod]
        public void ResizeGroup_ZerosValues_Valid() {
            GroupShape groupShape = PowerpointHelper.CreateGroupShape(50, 50, 50, 50);

            PowerpointHelper.ResizeGroup(groupShape, 0, 0);
        }

        /// <summary>
        /// Test le redimensionnement d'un groupe avec une largeur négative.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ResizeGroup_NegativeWidth_Throws() {
            GroupShape groupShape = PowerpointHelper.CreateGroupShape(50, 50, 50, 50);

            PowerpointHelper.ResizeGroup(groupShape, -100, 10);
        }

        /// <summary>
        /// Test le redimensionnement d'un groupe avec une hauteur négative.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ResizeGroup_NegativeHeight_Throws() {
            GroupShape groupShape = PowerpointHelper.CreateGroupShape(50, 50, 50, 50);

            PowerpointHelper.ResizeGroup(groupShape, 10, -100);
        }

        #endregion

        #region CreateConnectionShape

        /// <summary>
        /// Test la création d'un Connection Shape.
        /// </summary>
        [TestMethod]
        public void CreateConnectionShape_Valid() {
            Shape rectangle1 = PowerpointHelper.CreateRectangleShape(50, 10, 10, 20);
            Shape rectangle2 = PowerpointHelper.CreateRectangleShape(10, 20, 30, 40);

            PowerpointHelper.CreateConnectionShape(rectangle1, rectangle2, new int[] { 1, 2 }, new int[] { 3, 4 });
        }

        /// <summary>
        /// Test la création d'un polygon avec une 1ère coordonnée non valide.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateConnectionShape_InvalidStartPosition_Throws() {
            Shape rectangle1 = PowerpointHelper.CreateRectangleShape(50, 10, 10, 20);
            Shape rectangle2 = PowerpointHelper.CreateRectangleShape(10, 20, 30, 40);

            PowerpointHelper.CreateConnectionShape(rectangle1, rectangle2, new int[1], new int[] { 3, 4 });
        }

        /// <summary>
        /// Test la création d'un polygon avec une 2ème coordonnée non valide.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateConnectionShape_InvalidEndPosition_Throws() {
            Shape rectangle1 = PowerpointHelper.CreateRectangleShape(50, 10, 10, 20);
            Shape rectangle2 = PowerpointHelper.CreateRectangleShape(10, 20, 30, 40);

            PowerpointHelper.CreateConnectionShape(rectangle1, rectangle2, new int[] { 1, 2 }, new int[] { 3, 4, 5 });
        }

        #endregion

        #region AddImage

        /// <summary>
        /// Test l'ajout d'une image à un autre élément.
        /// </summary>
        [TestMethod]
        public void AddImage_Valid() {
            PresentationDocument pres = PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFileI.pptx");
            ShapeTree shapeTree = PowerpointHelper.GetShapeTreeOfFirstSlide(pres);
            GroupShape groupShape = PowerpointHelper.CreateGroupShape(50,50,50,50);
            PowerpointHelper.AddElement(shapeTree, groupShape);
            Bitmap logo = Resx.klee;

            int oldNbPicture = groupShape.Descendants<Picture>().Count();

            using (MemoryStream ms = new MemoryStream()) {
                logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                PowerpointHelper.AddImage(groupShape, ms, 50, 50, 50, 50);
            }

            Assert.IsTrue(groupShape.Descendants<Picture>().Count() - oldNbPicture == 1);
        }

        /// <summary>
        /// Test l'ajout d'une image avec des arguments négatifs et nuls.
        /// </summary>
        [TestMethod]
        public void AddImage_ZerosAttributes_Valid() {
            PresentationDocument pres = PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFileI1.pptx");
            ShapeTree shapeTree = PowerpointHelper.GetShapeTreeOfFirstSlide(pres);
            GroupShape groupShape = PowerpointHelper.CreateGroupShape(50, 50, 50, 50);
            PowerpointHelper.AddElement(shapeTree, groupShape);

            int oldNbPicture = groupShape.Descendants<Picture>().Count();

            Bitmap logo = Resx.klee;
            using (MemoryStream ms = new MemoryStream()) {
                logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                PowerpointHelper.AddImage(groupShape, ms, -50, -50, 0, 0);
            }

            Assert.IsTrue(groupShape.Descendants<Picture>().Count() - oldNbPicture == 1);
        }

        /// <summary>
        /// Test l'ajout d'une image avec une largeur négative.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddImage_NegativeWidth_Throws() {
            PresentationDocument pres = PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFileI2.pptx");
            ShapeTree shapeTree = PowerpointHelper.GetShapeTreeOfFirstSlide(pres);
            GroupShape groupShape = PowerpointHelper.CreateGroupShape(50, 50, 50, 50);
            PowerpointHelper.AddElement(shapeTree, groupShape);

            Bitmap logo = Resx.klee;
            using (MemoryStream ms = new MemoryStream()) {
                logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                PowerpointHelper.AddImage(groupShape, ms, 50, 50, -50, 50);
            }
        }

        /// <summary>
        /// Test l'ajout d'une image avec une hauteur négative.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddImage_NegativeHeight_Throws() {
            PresentationDocument pres = PowerpointHelper.CreatePresentationWithOneEmptySlide("./testFileI3.pptx");
            ShapeTree shapeTree = PowerpointHelper.GetShapeTreeOfFirstSlide(pres);
            GroupShape groupShape = PowerpointHelper.CreateGroupShape(50, 50, 50, 50);
            PowerpointHelper.AddElement(shapeTree, groupShape);

            Bitmap logo = Resx.klee;
            using (MemoryStream ms = new MemoryStream()) {
                logo.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                PowerpointHelper.AddImage(groupShape, ms, 50, 50, 50, -50);
            }
        }

        #endregion
    }
}
