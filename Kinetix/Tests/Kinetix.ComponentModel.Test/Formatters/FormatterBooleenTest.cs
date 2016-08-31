using System;
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
    /// Test du formatteur booléen.
    /// </summary>
    [TestFixture]
#if NUnit
    [SetCulture("fr-FR")]
    [SetUICulture("fr-FR")]
#endif
    public class FormatterBooleenTest {

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
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertToStringTrueTest() {
            Assert.AreEqual("Oui", new FormatterBooleen().ConvertToString(true));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertToStringFalseTest() {
            Assert.AreEqual("Non", new FormatterBooleen().ConvertToString(false));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertToStringNullTest() {
            Assert.AreEqual("-", new FormatterBooleen().ConvertToString(null));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringNullTest() {
            Assert.IsNull(new FormatterBooleen().ConvertFromString(null));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringEmptyTest() {
            Assert.IsNull(new FormatterBooleen().ConvertFromString(string.Empty));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringTrueTest() {
            Assert.AreEqual(true, new FormatterBooleen().ConvertFromString("True"));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringTrueLowerTest() {
            Assert.AreEqual(true, new FormatterBooleen().ConvertFromString("true"));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringOuiTest() {
            Assert.AreEqual(true, new FormatterBooleen().ConvertFromString("Oui"));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringOuiLowerTest() {
            Assert.AreEqual(true, new FormatterBooleen().ConvertFromString("oui"));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringUnTest() {
            Assert.AreEqual(true, new FormatterBooleen().ConvertFromString("1"));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringFalseTest() {
            Assert.AreEqual(false, new FormatterBooleen().ConvertFromString("False"));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringFalseLowerTest() {
            Assert.AreEqual(false, new FormatterBooleen().ConvertFromString("false"));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringNonTest() {
            Assert.AreEqual(false, new FormatterBooleen().ConvertFromString("Non"));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringNonLowerTest() {
            Assert.AreEqual(false, new FormatterBooleen().ConvertFromString("non"));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        public void CheckConvertFromStringZeroTest() {
            Assert.AreEqual(false, new FormatterBooleen().ConvertFromString("0"));
        }

        /// <summary>
        /// Vérifie la conversion vers chaine.
        /// </summary>
        [Test]
        [ExpectedException(typeof(FormatException))]
        public void CheckConvertFromStringUnknowTest() {
            new FormatterBooleen().ConvertFromString("Je ne sais pas.");
        }

        /// <summary>
        /// Vérification de l'unitée.
        /// </summary>
        [Test]
        public void FormatterUnit() {
            FormatterBooleen formatter = new FormatterBooleen();
            Assert.IsNull(formatter.Unit);
        }
    }
}
