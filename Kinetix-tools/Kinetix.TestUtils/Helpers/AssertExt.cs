using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kinetix.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Kinetix.TestUtils.Helpers {

    /// <summary>
    /// Extensions de l'API d'assertions.
    /// </summary>
    public static class AssertExt {

        /// <summary>
        /// Vérifie que deux ont les mêmes propriétés.
        /// </summary>
        /// <typeparam name="T">Type des éléments. Doit être un type avec des propriétés.</typeparam>
        /// <param name="expected">Bean attendu.</param>
        /// <param name="actual">Bean constaté.</param>
        public static void AreBeanEqual<T>(T expected, T actual) {
            AreBeanEqualCore(expected, actual);
        }

        /// <summary>
        /// Vérifie que deux collections ont les mêmes éléments avec les mêmes propriétés dans le même ordre.
        /// </summary>
        /// <typeparam name="T">Type des éléments. Doit être un type avec des propriétés.</typeparam>
        /// <param name="expected">Collection attendue.</param>
        /// <param name="actual">Collection constatée.</param>
        public static void AreCollectionPropertiesEqual<T>(ICollection<T> expected, ICollection<T> actual) {

            if (expected.Count != actual.Count) {
                throw new ArgumentException("Les deux collections n'ont pas le même nombre d'éléments.");
            }

            if (expected.Count != 0) {
                var beanDef = BeanDescriptor.GetDefinition(expected.First());

                var expectedValue = expected.GetEnumerator();
                var actualValue = actual.GetEnumerator();
                for (int i = 0; i < expected.Count; i++) {
                    expectedValue.MoveNext();
                    actualValue.MoveNext();
                    foreach (var prop in beanDef.Properties) {
                        Assert.AreEqual(prop.GetValue(expectedValue.Current), prop.GetValue(actualValue.Current), "Index : " + i + ". Propriété : " + prop.PropertyName);
                    }
                }
            }
        }

        /// <summary>
        /// Vérifie que deux collections ont les mêmes éléments dans le même ordre.
        /// </summary>
        /// <typeparam name="T">Type des éléments.</typeparam>
        /// <param name="expected">Collection attendue.</param>
        /// <param name="actual">Collection constatée.</param>
        public static void AreEqual<T>(ICollection<T> expected, ICollection<T> actual) {
            CollectionAssert.AreEqual((ICollection)expected, (ICollection)actual);
        }

        /// <summary>
        /// Vérifie que deux collections ont les mêmes éléments dans le même ordre.
        /// </summary>
        /// <typeparam name="T">Type des éléments.</typeparam>
        /// <param name="expected">Collection attendue.</param>
        /// <param name="actual">Collection constatée.</param>
        public static void AreEqual<T>(ICollection<T> expected, params T[] actual) {
            CollectionAssert.AreEqual((ICollection)expected, (ICollection)actual);
        }

        /// <summary>
        /// Vérifie qu'un argument est plus grand qu'un autre ou égal.
        /// </summary>
        /// <typeparam name="T">Type des arguments.</typeparam>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        public static void GreaterOrEqualThan<T>(T arg1, T arg2)
            where T : IComparable<T> {
            if (arg1.CompareTo(arg2) >= 0) {
                return;
            }

            Assert.Fail("Il était attendu que {0} >= {1}", arg1, arg2);
        }

        /// <summary>
        /// Vérifie qu'un argument est plus grand qu'un autre.
        /// </summary>
        /// <typeparam name="T">Type des arguments.</typeparam>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        public static void GreaterThan<T>(T arg1, T arg2)
            where T : IComparable<T> {
            if (arg1.CompareTo(arg2) > 0) {
                return;
            }

            Assert.Fail("Il était attendu que {0} > {1}", arg1, arg2);
        }

        /// <summary>
        /// Vérifie qu'un argument est plus petit qu'un autre.
        /// </summary>
        /// <typeparam name="T">Type des arguments.</typeparam>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        public static void LesserOrEqualThan<T>(T arg1, T arg2)
            where T : IComparable<T> {
            if (arg1.CompareTo(arg2) <= 0) {
                return;
            }

            Assert.Fail("Il était attendu que {0} <= {1}", arg1, arg2);
        }

        /// <summary>
        /// Vérifie qu'un argument est plus petit qu'un autre.
        /// </summary>
        /// <typeparam name="T">Type des arguments.</typeparam>
        /// <param name="arg1">Argument 1.</param>
        /// <param name="arg2">Argument 2.</param>
        public static void LesserThan<T>(T arg1, T arg2)
            where T : IComparable<T> {
            if (arg1.CompareTo(arg2) < 0) {
                return;
            }

            Assert.Fail("Il était attendu que {0} < {1}", arg1, arg2);
        }

        /// <summary>
        /// Teste qu'une action donnée renvoie une exception d'un type donné.
        /// Si l'exception attendue est levée, on la renvoie.
        /// Si l'exception est d'un autre type, on met le test courant en erreur.
        /// Si l'exception n'est pas levé, on met le test courant en erreur.
        /// </summary>
        /// <typeparam name="TExpected">Type de l'exception attendue.</typeparam>
        /// <param name="action">Action à tester.</param>
        /// <returns>Instance de l'exception.</returns>
        public static TExpected Throws<TExpected>(Action action)
            where TExpected : Exception {
            try {
                action();
            } catch (TExpected ex) {
                return ex;
            } catch (Exception ex) {
                Assert.Fail("Attendu : exception de type {0} ; constaté : {1}", typeof(TExpected).FullName, ex.GetType());
                return null;
            }

            Assert.Fail("Attendu : exception de type {0} ; constaté : pas d'exception", typeof(TExpected).FullName);
            return null;
        }

        /// <summary>
        /// Vérifie que deux ont les mêmes propriétés.
        /// </summary>
        /// <param name="expected">Bean attendu.</param>
        /// <param name="actual">Bean constaté.</param>
        private static void AreBeanEqualCore(object expected, object actual) {
            if (expected == null && actual == null) {
                return;
            }

            if (expected == null || actual == null) {
                Assert.Fail("Un des beans est null.");
            }

            var beanDef = BeanDescriptor.GetDefinition(expected);

            foreach (var prop in beanDef.Properties) {
                if (prop.PrimitiveType != null) {
                    Assert.AreEqual(prop.GetValue(expected), prop.GetValue(actual), $"Propriété {prop.PropertyName}");
                } else {
                    AreBeanEqualCore(prop.GetValue(expected), prop.GetValue(actual));
                }
            }
        }
    }
}
