using System;
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
    /// Classe de tests unitaires pour le formatteur 
    /// de chaînes en majuscules.
    /// </summary>
    [TestFixture]
    public class FormatterUpperCaseTest {
        /// <summary>
        /// Vérifie le constructeur.
        /// </summary>
        public FormatterUpperCaseTest() {
            new FormatterUpperCase();
        }

        /// <summary>
        /// Vérifie la conversion saisie vers chaîne en majuscule.
        /// </summary>
        [Test]
        public void CheckConvertToStringTest() {
            FormatterUpperCase formatter = new FormatterUpperCase();
            string expected = @"AB-CD@ÉF.G";
            string source = @"ab-cd@éf.g";
            string actual = formatter.ConvertToString(source);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Vérifie la conversion persistence vers majuscule.
        /// </summary>
        [Test]
        public void CheckConvertFromStringTest() {
            FormatterUpperCase formatter = new FormatterUpperCase();
            string expected = @"AB-CD@ÉF.G";
            string source = @"ab-cd@éf.g";
            string actual = ((IFormatter<string>)formatter).ConvertFromString(source);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Vérifie la conversion persistence vers majuscule.
        /// </summary>
        [Test]
        public void CheckConvertFromStringNullTest() {
            FormatterUpperCase formatter = new FormatterUpperCase();
            Assert.IsNull(formatter.ConvertFromString(null));
        }

        /// <summary>
        /// Vérifie la conversion persistence vers majuscule.
        /// </summary>
        [Test]
        public void CheckConvertFromStringEmptyTest() {
            FormatterUpperCase formatter = new FormatterUpperCase();
            Assert.IsNull(formatter.ConvertFromString(String.Empty));
        }

        /// <summary>
        /// Vérification de l'unitée.
        /// </summary>
        [Test]
        public void FormatterUnit() {
            FormatterUpperCase formatter = new FormatterUpperCase();
            Assert.IsNull(formatter.Unit);
        }
    }
}
