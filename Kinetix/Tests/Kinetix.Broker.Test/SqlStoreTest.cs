using System;
using System.Collections.Generic;
using Kinetix.Data.SqlClient;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
#endif

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Classe de test du SqlStore.
    /// </summary>
    [TestFixture]
    public class SqlStoreTest {
        /// <summary>
        /// Initialise le contexte des tests.
        /// </summary>
        [SetUp]
        public void SetUp() {
            BrokerManager.Instance.RegisterStore("name", typeof(TestStore<>));
        }

        /// <summary>
        /// Test le constructeur.
        /// </summary>
        [Test]
        public void TestConstructor() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
        }

        /// <summary>
        /// Test le nom du store.
        /// </summary>
        [Test]
        public void TestName() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            Assert.AreEqual("name", store.Name);
        }

        /// <summary>
        /// Test le constructeur avec un paramètre null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(BrokerException))]
        public void TestConstructorNull() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>(null);
        }

        /// <summary>
        /// Test la méthode load avec une clef primaire nulle.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestLoadNull() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.Load(null, null);
        }

        /// <summary>
        /// Test la méthode Load sur un type non persistant.
        /// </summary>
        [Test]
        [ExpectedException(typeof(BrokerException))]
        public void TestLoadNotPersistentType() {
            SqlTestStore<object> store = new SqlTestStore<object>("name");
        }

        /// <summary>
        /// Test la méthode Load sur un type sans clef primaire.
        /// </summary>
        [Test]
        [ExpectedException(typeof(BrokerException))]
        public void TestLoadBadPrimaryKey() {
            SqlTestStore<BeanBadPrimaryKey> store = new SqlTestStore<BeanBadPrimaryKey>("name");
        }

        /// <summary>
        /// Test la méthode Load avec une clef primaire ne correspondant 
        /// pas au type de la clef de l'objet.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidCastException))]
        public void TestLoadBadPrimaryKeyType() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.Load(null, String.Empty);
        }

        /// <summary>
        /// Test la méthode Load.
        /// </summary>
        [Test]
        public void TestLoad() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            Bean bean = store.Load(null, 1);
        }

        /// <summary>
        /// Test la méthode LoadAll retournant des champs non utilisés.
        /// </summary>
        [Test]
        public void TestLoadAllUnusedField() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            ICollection<Bean> coll = store.LoadAll(null, new QueryParameter(9));
            Assert.AreEqual(9, coll.Count);
        }

        /// <summary>
        /// Test la méthode Load, vérifie les valeurs de retour.
        /// </summary>
        [Test]
        public void TestLoadValues() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            Bean bean = store.Load(null, 1);
            Assert.AreEqual(0, bean.Id);
            Assert.AreEqual("0", bean.Name);
        }

        /// <summary>
        /// Test la méthode Load, vérifie une erreur si pas de ligne de retour.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CollectionBuilderException))]
        public void TestLoadZeroRow() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            Bean bean = store.Load(null, 0);
        }

        /// <summary>
        /// Test la méthode Load, vérifie une erreur si trop de lignes de retour.
        /// </summary>
        [Test]
        [ExpectedException(typeof(CollectionBuilderException))]
        public void TestLoadTooMayRows() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            Bean bean = store.Load(null, 2);
        }

        /// <summary>
        /// Test la méthode LoadAll.
        /// </summary>
        [Test]
        public void TestLoadAll() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            ICollection<Bean> coll = store.LoadAll(null, new QueryParameter(3));
            Assert.AreEqual(3, coll.Count);
            int index = 0;
            foreach (Bean bean in coll) {
                Assert.AreEqual(index, bean.Id);
                Assert.AreEqual(index.ToString(), bean.Name);
                index++;
            }
        }

        /// <summary>
        /// Test la méthode LoadAll retournant trop de lignes.
        /// </summary>
        [Test]
        [ExpectedException(typeof(BrokerException))]
        public void TestLoadAllTooManyRows() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            ICollection<Bean> coll = store.LoadAll(null, new QueryParameter(15));
        }

        /// <summary>
        /// Test la méthode Put avec un paramètre null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestPutNull() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.Put(null);
        }

        /// <summary>
        /// Test la méthode Put.
        /// </summary>
        [Test]
        public void TestPutUpdate() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            Bean original = new Bean();
            original.Id = 1;
            store.Put(original);
        }

        /// <summary>
        /// Test la méthode Put et vérifie les valeurs.
        /// </summary>
        [Test]
        public void TestPutUpdateValues() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            Bean original = new Bean();
            original.Id = 1;

            // On attend la valeur de la clé primaire
            // comme valeur de retour.
            Assert.AreEqual(1, store.Put(original));
        }

        /// <summary>
        /// Test la méthode Put.
        /// </summary>
        [Test]
        public void TestPutInsert() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            Bean original = new Bean();
            store.Put(original);
        }

        /// <summary>
        /// Test la méthode Put et vérifie les valeurs.
        /// </summary>
        [Test]
        public void TestPutInsertValues() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            Bean original = new Bean();
            Assert.AreEqual(0, store.Put(original));
        }

        /// <summary>
        /// Test la méthode Put modifiant plusieurs lignes.
        /// </summary>
        [Test]
        [ExpectedException(typeof(BrokerException))]
        public void TestPutUpdateTooManyRowsAffected() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            Bean original = new Bean();
            original.Id = 7;
            store.Put(original);
        }

        /// <summary>
        /// Test la méthode Put ne modifiant aucune ligne.
        /// </summary>
        [Test]
        [ExpectedException(typeof(BrokerException))]
        public void TestPutUpdateZeroRowsAffected() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            Bean original = new Bean();
            original.Id = 6;
            store.Put(original);
        }

        /// <summary>
        /// Test la méthode RemoveAllLike quand le paramètre est nul.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestRemoveAllByCriteriaNull() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.RemoveAllByCriteria(null);
        }

        /// <summary>
        /// Test la méthode RemoveAllLike.
        /// </summary>
        [Test]
        public void TestRemoveAllByCriteria() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.RemoveAllByCriteria(new FilterCriteria());
        }

        /// <summary>
        /// Test la méthode Remove quand le paramètre est nul.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestRemoveNull() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.Remove(null);
        }

        /// <summary>
        /// Test la méthode Remove.
        /// </summary>
        [Test]
        public void TestRemove() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.Remove(2);
        }

        /// <summary>
        /// Test la méthode Remove avec un id inexistant.
        /// </summary>
        [Test]
        [ExpectedException(typeof(BrokerException))]
        public void TestRemoveZeroRow() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.Remove(10);
        }

        /// <summary>
        /// Test la méthode Remove avec un id multiple.
        /// </summary>
        [Test]
        [ExpectedException(typeof(BrokerException))]
        public void TestRemoveTooManyRows() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.Remove(20);
        }

        /// <summary>
        /// Test la méthode LoadAllLike avec un critère null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestLoadAllByCriteriaNull() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.LoadAllByCriteria(null, null, null);
        }

        /// <summary>
        /// Test la méthode LoadAllLike.
        /// </summary>
        [Test]
        public void TestLoadAllByCriteria() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.LoadAllByCriteria(null, new FilterCriteria().Equals(Bean.Cols.BEA_ID, 1), null);
        }

        /// <summary>
        /// Test la méthode LoadLike avec un critère null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestLoadByCriteriaNull() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.LoadByCriteria(null, null);
        }

        /// <summary>
        /// Test la méthode LoadLike.
        /// </summary>
        [Test]
        public void TestLoadByCriteria() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.LoadByCriteria(null, new FilterCriteria().Equals(Bean.Cols.BEA_ID, 1));
        }

        /// <summary>
        /// Test l'ordre de tri avec un champ "Libelle".
        /// </summary>
        [Test]
        public void TestLoadByCriteriaSortOrderLibelle() {
            SqlTestStore<BeanLibelle> store = new SqlTestStore<BeanLibelle>("name");
            store.LoadAllByCriteria(null, new FilterCriteria().Equals(Bean.Cols.BEA_ID, 1), null);
            Assert.AreEqual("BEA_LIBELLE", store.SortOrder);
        }

        /// <summary>
        /// Test l'ordre de tri avec un champ par défaut.
        /// </summary>
        [Test]
        public void TestLoadByCriteriaSortOrderDefault() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.LoadAllByCriteria(null, new FilterCriteria().Equals(Bean.Cols.BEA_ID, 1), null);
            Assert.AreEqual("BEA_NAME", store.SortOrder);
        }

        /// <summary>
        /// Test l'ordre de croissant tri sur un champ.
        /// </summary>
        [Test]
        public void TestLoadByCriteriaSortOrderManualAsc() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.LoadAllByCriteria(null, new FilterCriteria().Equals(Bean.Cols.BEA_ID, 1), new QueryParameter(Bean.Cols.BEA_ID, SortOrder.Asc));
            Assert.AreEqual("BEA_ID asc", store.SortOrder);
        }

        /// <summary>
        /// Test l'ordre de tri décroissant sur un champ.
        /// </summary>
        [Test]
        public void TestLoadByCriteriaSortOrderManualDesc() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            store.LoadAllByCriteria(null, new FilterCriteria().Equals(Bean.Cols.BEA_ID, 1), new QueryParameter(Bean.Cols.BEA_ID, SortOrder.Desc));
            Assert.AreEqual("BEA_ID desc", store.SortOrder);
        }

        /// <summary>
        /// Test l'ordre de tri sur deux champs.
        /// </summary>
        [Test]
        public void TestLoadByCriteriaSortOrderManualBoth() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            QueryParameter sort = new QueryParameter();
            sort.AddSortParam(Bean.Cols.BEA_ID, SortOrder.Asc);
            sort.AddSortParam(Bean.Cols.BEA_NAME, SortOrder.Desc);
            store.LoadAllByCriteria(null, new FilterCriteria().Equals(Bean.Cols.BEA_ID, 1), sort);
            Assert.AreEqual("BEA_ID asc, BEA_NAME desc", store.SortOrder);
        }

        /// <summary>
        /// Test la méthode LoadLike.
        /// </summary>
        [Test]
        public void TestAddRule() {
            SqlTestStore<Bean> store = new SqlTestStore<Bean>("name");
            IStoreRule rule = new VersionRule("BEA_NAME");
            store.AddRule(rule);
            bool hasRuleBeaName = false;
            foreach (IStoreRule r in store.Rules) {
                if ("BEA_NAME".Equals(r.FieldName)) {
                    hasRuleBeaName = true;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsTrue(hasRuleBeaName);
            Assert.IsNotNull(store.GetRule("BEA_NAME"));
            Assert.IsNull(store.GetRule("BEA_NO_RULE"));
        }
    }
}
