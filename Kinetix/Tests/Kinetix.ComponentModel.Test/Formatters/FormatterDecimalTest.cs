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
    /// Test du formateur décimal.
    /// </summary>
    [TestFixture]
#if NUnit
    [SetCulture("fr-FR")]
    [SetUICulture("fr-FR")]
#endif
    public class FormatterDecimalTest {

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
        /// Test du formateur.
        /// </summary>
        [Test]
        public void TestConstructeur() {
            FormatterDecimal formatter = new FormatterDecimal();
            Assert.IsNull(formatter.Unit);
        }

        /// <summary>
        /// Test du formateur.
        /// </summary>
        [Test]
        public void TestConvertToString() {
            FormatterDecimal formatter = new FormatterDecimal();
            Assert.AreEqual("2,2", formatter.ConvertToString(2.2m));
        }

        /// <summary>
        /// Test du formateur.
        /// </summary>
        [Test]
        public void TestConvertToStringNull() {
            FormatterDecimal formatter = new FormatterDecimal();
            Assert.IsNull(formatter.ConvertToString(null));
        }

        /// <summary>
        /// Test du formateur.
        /// </summary>
        [Test]
        public void TestConvertFromStringNull() {
            FormatterDecimal formatter = new FormatterDecimal();
            Assert.IsNull(formatter.ConvertFromString(null));
        }
    }
}
