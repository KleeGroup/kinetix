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
    /// Classe de test pour le formatteur de mise  
    /// en capitale des mots d'une chaîne.
    /// </summary>
    [TestFixture]
    public class FormatterCapitalizeTest {
        /// <summary>
        /// Vérifie le constructeur.
        /// </summary>
        public FormatterCapitalizeTest() {
            new FormatterCapitalize();
        }

        /// <summary>
        /// Vérifie la conversion saisie vers chaîne en majuscule.
        /// </summary>
        [Test]
        public void CheckConvertToStringTest() {
            FormatterCapitalize formatter = new FormatterCapitalize();
            string expected = @"Jean-Sébastien Bach";
            string source = @"jEan-séBastien bACH";
            string actual = formatter.ConvertToString(source);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Vérifie la conversion persistence vers majuscule.
        /// </summary>
        [Test]
        public void CheckConvertFromStringTest() {
            FormatterCapitalize formatter = new FormatterCapitalize();
            string expected = @"Jean-Sébastien Bach";
            string source = @"jEan-séBastien bACH";
            string actual = ((IFormatter<string>)formatter).ConvertFromString(source);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Vérifie la conversion persistence vers majuscule.
        /// </summary>
        [Test]
        public void CheckConvertFromStringNullTest() {
            FormatterCapitalize formatter = new FormatterCapitalize();
            Assert.IsNull(formatter.ConvertFromString(null));
        }

        /// <summary>
        /// Vérifie la conversion persistence vers majuscule.
        /// </summary>
        [Test]
        public void CheckConvertFromStringEmptyTest() {
            FormatterCapitalize formatter = new FormatterCapitalize();
            Assert.IsNull(formatter.ConvertFromString(String.Empty));
        }

        /// <summary>
        /// Vérification de l'unitée.
        /// </summary>
        [Test]
        public void FormatterUnit() {
            FormatterCapitalize formatter = new FormatterCapitalize();
            Assert.IsNull(formatter.Unit);
        }
    }
}
