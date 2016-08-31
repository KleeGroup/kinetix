using System;
using System.Collections;
using System.Collections.Generic;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using Kinetix.Caching.Store;

namespace Kinetix.Caching.Test {
    /// <summary>
    /// Classe de test du SequencedDictionary.
    /// </summary>
    [TestFixture]
    public class SequencedDictionaryTest {
        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestConstructeur() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestConstructeurSize() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>(15);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestAdd() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual(1, dic.Count);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestICollectionAdd() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            ((ICollection<KeyValuePair<string, string>>)dic).Add(new KeyValuePair<string, string>("Key1", "Value1"));
            Assert.AreEqual(1, dic.Count);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestIDictionaryAdd() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            ((IDictionary)dic).Add("Key1", "Value1");
            Assert.AreEqual(1, dic.Count);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestClear() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual(1, dic.Count);
            dic.Clear();
            Assert.AreEqual(0, dic.Count);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestContains() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual(1, dic.Count);
            Assert.IsTrue(dic.Contains("Key1"));
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestICollectionContains() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual(1, dic.Count);
            Assert.IsTrue(((ICollection<KeyValuePair<string, string>>)dic).Contains(new KeyValuePair<string, string>("Key1", "Value1")));
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestContainsKey() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual(1, dic.Count);
            Assert.IsTrue(dic.ContainsKey("Key1"));
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestFirstKey() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual("Key1", dic.FirstKey);
            dic.Add("Key2", "Value2");
            Assert.AreEqual("Key1", dic.FirstKey);
            dic.Remove("Key1");
            Assert.AreEqual("Key2", dic.FirstKey);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestLastKey() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual("Key1", dic.LastKey);
            dic.Add("Key2", "Value2");
            Assert.AreEqual("Key2", dic.LastKey);
            dic.Remove("Key1");
            Assert.AreEqual("Key2", dic.LastKey);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestFirstValue() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual("Value1", dic.FirstValue);
            dic.Add("Key2", "Value2");
            Assert.AreEqual("Value1", dic.FirstValue);
            dic.Remove("Key1");
            Assert.AreEqual("Value2", dic.FirstValue);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestLastValue() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual("Value1", dic.LastValue);
            dic.Add("Key2", "Value2");
            Assert.AreEqual("Value2", dic.LastValue);
            dic.Remove("Key1");
            Assert.AreEqual("Value2", dic.LastValue);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestIsFixedSize() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            Assert.IsFalse(dic.IsFixedSize);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestIsReadOnly() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            Assert.IsFalse(dic.IsReadOnly);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestICollectionIsReadOnly() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            Assert.IsFalse(((ICollection<KeyValuePair<string, string>>)dic).IsReadOnly);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestIsSynchronized() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            Assert.IsFalse(dic.IsSynchronized);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestGetItem() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual("Value1", dic["Key1"]);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestIDictionaryGetItem() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual("Value1", ((IDictionary)dic)["Key1"]);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestIDictionarySetItem() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            ((IDictionary)dic)["Key1"] = "Value1";
            Assert.AreEqual("Value1", ((IDictionary)dic)["Key1"]);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestRemove() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual(1, dic.Count);
            Assert.IsTrue(dic.Remove("Key1"));
            Assert.AreEqual(0, dic.Count);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestIDictionaryRemove() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual(1, dic.Count);
            ((IDictionary)dic).Remove("Key1");
            Assert.AreEqual(0, dic.Count);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestICollectionRemove() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            Assert.AreEqual(1, dic.Count);
            Assert.IsTrue(((ICollection<KeyValuePair<string, string>>)dic).Remove(new KeyValuePair<string, string>("Key1", "Value1")));
            Assert.AreEqual(0, dic.Count);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestRemoveMissing() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            Assert.IsFalse(dic.Remove("Key1"));
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestGetItemMissing() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            Assert.IsNull(dic["Key1"]);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestKeys() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");
            ICollection<string> keys = dic.Keys;
            Assert.IsFalse(keys.Contains("Key1"));
            Assert.IsTrue(keys.Contains("Key2"));
            Assert.IsTrue(keys.Contains("Key3"));
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestIDictionaryKeys() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");
            ICollection keys = ((IDictionary)dic).Keys;
            Assert.AreEqual(2, keys.Count);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestICollectionCopyTo() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");
            object[] values = new object[2];
            ((ICollection)dic).CopyTo(values, 0);
            Assert.AreEqual("Value2", values[0]);
            Assert.AreEqual("Value3", values[1]);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestValues() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");

            bool containsV2 = false;
            bool containsV3 = false;
            foreach (object o in dic.Values) {
                if ("Value2".Equals(o)) {
                    containsV2 = true;
                } else if ("Value3".Equals(o)) {
                    containsV3 = true;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsTrue(containsV2);
            Assert.IsTrue(containsV3);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestIDictionaryValues() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");

            bool containsV2 = false;
            bool containsV3 = false;
            foreach (string s in ((IDictionary<string, string>)dic).Values) {
                if ("Value2".Equals(s)) {
                    containsV2 = true;
                } else if ("Value3".Equals(s)) {
                    containsV3 = true;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsTrue(containsV2);
            Assert.IsTrue(containsV3);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestGetEnumerator() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");

            bool containsV2 = false;
            bool containsV3 = false;
            IEnumerator<KeyValuePair<string, string>> enumerator = dic.GetEnumerator();
            while (enumerator.MoveNext()) {
                string s = enumerator.Current.Value;
                if ("Value2".Equals(s)) {
                    containsV2 = true;
                } else if ("Value3".Equals(s)) {
                    containsV3 = true;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsTrue(containsV2);
            Assert.IsTrue(containsV3);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestIEnumerableGetEnumerator() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");

            bool containsV2 = false;
            bool containsV3 = false;
            IEnumerator enumerator = ((IEnumerable)dic).GetEnumerator();
            while (enumerator.MoveNext()) {
                string s = (string)enumerator.Current;
                if ("Value2".Equals(s)) {
                    containsV2 = true;
                } else if ("Value3".Equals(s)) {
                    containsV3 = true;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsTrue(containsV2);
            Assert.IsTrue(containsV3);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestIDictionaryGetEnumerator() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");

            bool containsV2 = false;
            bool containsV3 = false;
            foreach (object o in (IDictionary)dic) {
                if ("Value2".Equals(((DictionaryEntry)o).Value)) {
                    containsV2 = true;
                } else if ("Value3".Equals(((DictionaryEntry)o).Value)) {
                    containsV3 = true;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsTrue(containsV2);
            Assert.IsTrue(containsV3);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestSequencedEnumerator() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");

            IDictionaryEnumerator enumerator = ((IDictionary)dic).GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("Key2", enumerator.Key);
            Assert.AreEqual("Value2", enumerator.Value);
            Assert.IsNotNull(enumerator.Entry);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("Key3", enumerator.Key);
            Assert.AreEqual("Value3", enumerator.Value);
            Assert.IsNotNull(enumerator.Entry);
            Assert.IsFalse(enumerator.MoveNext());
            enumerator.Reset();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual("Key2", enumerator.Key);
            Assert.AreEqual("Value2", enumerator.Value);
            Assert.IsNotNull(enumerator.Entry);
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSequencedEnumeratorCurrent() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");

            IDictionaryEnumerator enumerator = ((IDictionary)dic).GetEnumerator();
            object o = enumerator.Current;
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSequencedEnumeratorKey() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");

            IDictionaryEnumerator enumerator = ((IDictionary)dic).GetEnumerator();
            object o = enumerator.Key;
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSequencedEnumeratorValue() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");

            IDictionaryEnumerator enumerator = ((IDictionary)dic).GetEnumerator();
            object o = enumerator.Value;
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestSequencedEnumeratorEntry() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            dic.Add("Key1", "Value1");
            dic.Add("Key2", "Value2");
            dic.Add("Key3", "Value3");
            dic.Remove("Key1");

            IDictionaryEnumerator enumerator = ((IDictionary)dic).GetEnumerator();
            object o = enumerator.Entry;
        }

        /// <summary>
        /// Test SequencedDictionary.
        /// </summary>
        [Test]
        public void TestSyncRoot() {
            SequencedDictionary<string, string> dic = new SequencedDictionary<string, string>();
            Assert.IsNotNull(dic.SyncRoot);
        }
    }
}
