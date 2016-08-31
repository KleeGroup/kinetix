using System;
using System.Globalization;
using System.Threading;
using Kinetix.ComponentModel.Formatters;
#if NUnit
    using NUnit.Framework; 
#else
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
    using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#endif

namespace Kinetix.ComponentModel.Test.Formatters {
    /// <summary>
    /// Test du formateur pourcentage.
    /// </summary>
    [TestFixture]
#if NUnit
    [SetCulture("fr-FR")]
    [SetUICulture("fr-FR")]
#endif
    public class FormatterPercentTest {

#if !NUnit
        /// <summary>
        /// Fixe la culture courante.
        /// </summary>
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestInitialize]
        public void Initialize() {
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fr-FR");
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr-FR");
        }
#endif
        /// <summary>
        /// Test du formateur pourcentage.
        /// </summary>
        [Test]
        public void TestConstructeur() {
            FormatterPercent formatter = new FormatterPercent();
            Assert.AreEqual("%", formatter.Unit);
        }

        /// <summary>
        /// Test du formateur pourcentage.
        /// </summary>
        [Test]
        public void TestConvertToString() {
            FormatterPercent formatter = new FormatterPercent();
            Assert.AreEqual("50", formatter.ConvertToString(50m));
        }

        /// <summary>
        /// Test du formateur pourcentage.
        /// </summary>
        [Test]
        public void TestConvertToString2() {
            FormatterPercent formatter = new FormatterPercent();
            Assert.AreEqual("50,01", formatter.ConvertToString(50.01m));
        }

        /// <summary>
        /// Test du formateur pourcentage.
        /// </summary>
        [Test]
        public void TestConvertToStringNull() {
            FormatterPercent formatter = new FormatterPercent();
            Assert.IsNull(formatter.ConvertToString(null));
        }

        /// <summary>
        /// Test du formateur pourcentage.
        /// </summary>
        [Test]
        public void TestConvertFromStringNull() {
            FormatterPercent formatter = new FormatterPercent();
            Assert.IsNull(formatter.ConvertFromString(null));
        }

        /// <summary>
        /// Test du formateur pourcentage.
        /// </summary>
        [Test]
        public void TestConvertFromStringComa() {
            FormatterPercent formatter = new FormatterPercent();
            Assert.AreEqual(51.45m, formatter.ConvertFromString("51,45"));
        }

        /// <summary>
        /// Test du formateur pourcentage.
        /// </summary>
        [Test]
        public void TestConvertFromStringPercentWithSpace() {
            FormatterPercent formatter = new FormatterPercent();
            Assert.AreEqual(51.45m, formatter.ConvertFromString("51,45 %"));
        }

        /// <summary>
        /// Test du formateur pourcentage.
        /// </summary>
        [Test]
        public void TestConvertFromStringPercent() {
            FormatterPercent formatter = new FormatterPercent();
            Assert.AreEqual(51.45m, formatter.ConvertFromString("51,45%"));
        }

        /// <summary>
        /// Test du formateur pourcentage.
        /// </summary>
        [Test]
        public void TestConvertFromStringPercentEmpty() {
            FormatterPercent formatter = new FormatterPercent();
            Assert.IsNull(formatter.ConvertFromString("%"));
        }

        /// <summary>
        /// Test du formateur pourcentage.
        /// </summary>
        [Test]
        public void TestConvertFromStringDot() {
            CultureInfo previousCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            FormatterPercent formatter = new FormatterPercent();
            Assert.AreEqual(51.45m, formatter.ConvertFromString("51.45"));
            Thread.CurrentThread.CurrentCulture = previousCulture;
        }

        /// <summary>
        /// Test du formateur pourcentage.
        /// </summary>
        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestConvertFromStringBadFormat() {
            FormatterPercent formatter = new FormatterPercent();
            formatter.ConvertFromString("azerty");
        }
    }
}
