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
    /// This is a test class for FormatterDateTest and is intended
    /// to contain all FormatterDateTest Unit Tests.
    /// </summary>
    [TestFixture]
    public class FormatterDateTest {

        /// <summary>
        /// A test for ConvertToString.
        /// </summary>
        /// <todo who="FCN" type="IGNORE">Initialize to an appropriate value.</todo>
        [Test]
        public void ConvertToStringTest() {
            FormatterDate target = new FormatterDate();
            target.FormatString = "dd/MM/yyyy";
            DateTime val = new DateTime(0);
            val = val.AddYears(2007 - 1);
            val = val.AddMonths(1 - 1);
            val = val.AddDays(13 - 1);
            string expected = "13/01/2007";
            string actual;
            actual = target.ConvertToString(val);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ConvertToString avec une valeur nulle.
        /// </summary>
        [Test]
        public void ConvertToStringNullTest() {
            FormatterDate target = new FormatterDate();
            Assert.IsNull(target.ConvertToString(null));
        }

        /// <summary>
        /// A test for ConvertFromString.
        /// </summary>
        [Test]
        public void ConvertFromStringTest() {
            FormatterDate target = new FormatterDate();
            string s = "02/01/2007";
            DateTime expected = new DateTime(0);
            expected = expected.AddYears(2007 - 1);
            expected = expected.AddMonths(1 - 1);
            expected = expected.AddDays(02 - 1);
            DateTime? actual;
            actual = ((IFormatter<DateTime?>)target).ConvertFromString(s);
            Assert.AreEqual(expected, actual.Value);
        }

        /// <summary>
        /// A test for ConvertFromString avec une valeur nulle.
        /// </summary>
        [Test]
        public void ConvertFromStringNullTest() {
            FormatterDate target = new FormatterDate();
            Assert.IsNull(target.ConvertFromString(null));
        }

        /// <summary>
        /// A test for ConvertFromString avec une valeur vide.
        /// </summary>
        [Test]
        public void ConvertFromStringEmptyTest() {
            FormatterDate target = new FormatterDate();
            Assert.IsNull(target.ConvertFromString(String.Empty));
        }

        /// <summary>
        /// A test for ConvertFromString avec une valeur vide.
        /// </summary>
        [Test]
        [ExpectedException(typeof(FormatException))]
        public void ConvertFromStringBadFormatTest() {
            FormatterDate target = new FormatterDate();
            target.ConvertFromString("azerty");
        }

        /// <summary>
        /// A test for FormatterDate Constructor.
        /// </summary>
        [Test]
        public void FormatterDateConstructorTest() {
            FormatterDate target = new FormatterDate();
        }

        /// <summary>
        /// Vérification de l'unitée.
        /// </summary>
        [Test]
        public void FormatterUnit() {
            FormatterDate formatter = new FormatterDate();
            Assert.IsNull(formatter.Unit);
        }
    }
}
