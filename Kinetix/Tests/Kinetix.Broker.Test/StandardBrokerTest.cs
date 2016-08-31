using System;
using System.Collections.Generic;
using System.Transactions;
using System.Data.Linq;
#if NUnit
    using NUnit.Framework; 
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using SetUpAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute;
using Kinetix.Data.SqlClient;
#endif

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Classe de test du broker standard.
    /// </summary>
    [TestFixture]
    public class StandardBrokerTest {
        /// <summary>
        /// Initialise le contexte des tests.
        /// </summary>
        [SetUp]
        public void SetUp() {
            BrokerManager.Instance.RegisterStore("name", typeof(TestStore<>));
        }

        /// <summary>
        /// Test le fonctionnement du constructeur.
        /// </summary>
        [Test]
        public void TestConstructor() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
        }

        /// <summary>
        /// Test l'appel au constructeur avec un paramètre null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructorNull() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>(null);
        }

        /// <summary>
        /// Test le bon fonctionnement du delete.
        /// </summary>
        [Test]
        public void TestDelete() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            broker.Delete(0);
        }

        /// <summary>
        /// Test le bon fonctionnement du DeleteAllLike.
        /// </summary>
        [Test]
        public void TestDeleteAllLike() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            broker.DeleteAllByCriteria(new FilterCriteria().Equals(Bean.Cols.BEA_ID, 3));
        }

        /// <summary>
        /// Test le bon fonctionnement du delete avec une valeur nulle.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestDeleteNull() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            broker.DeleteCollection(null);
        }

        /// <summary>
        /// Test le bon fonctionnement du DeleteAllLike avec une valeur nulle.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestDeleteAllLikeNull() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            broker.DeleteAllByCriteria(null);
        }

        /// <summary>
        /// Test la présence d'un contexte transactionnel pour Delete.
        /// </summary>
        [Test]
        public void TestDeleteTransactionScope() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            broker.Delete(0);
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test la présence d'un contexte transactionnel pour DeleteAllLike.
        /// </summary>
        [Test]
        public void TestDeleteAllLikeTransactionScope() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            broker.DeleteAllByCriteria(new FilterCriteria().Equals(Bean.Cols.BEA_ID, 3));
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le complete du contexte transactionnel de delete.
        /// </summary>
        [Test]
        public void TestDeleteTransactionComplete() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                broker.Delete((int?)0);
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le complete du contexte transactionnel de DeleteAllLike.
        /// </summary>
        [Test]
        public void TestDeleteAllLikeTransactionComplete() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                broker.DeleteAllByCriteria(new FilterCriteria().Equals(Bean.Cols.BEA_ID, 0));
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le rollback du contexte transactionnel de delete.
        /// </summary>
        [Test]
        [ExpectedException(typeof(TransactionAbortedException))]
        public void TestDeleteTransactionRollback() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = true;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                try {
                    broker.Delete(0);
                } catch {
                    // On intercepte l'exception.
                }
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le rollback du contexte transactionnel de DeleteAllLike.
        /// </summary>
        [Test]
        [ExpectedException(typeof(TransactionAbortedException))]
        public void TestDeleteAllLikeTransactionRollback() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = true;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                try {
                    broker.DeleteAllByCriteria(new FilterCriteria());
                } catch {
                    // On intercepte l'exception.
                }
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le bon fonctionnement du Get.
        /// </summary>
        [Test]
        public void TestGet() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            Assert.IsNotNull(broker.Get(new object()));
        }

        /// <summary>
        /// Test la présence d'un contexte transactionnel pour Get.
        /// </summary>
        [Test]
        public void TestGetTransactionScope() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            Assert.IsNotNull(broker.Get(new object()));
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le complete du contexte transactionnel de Get.
        /// </summary>
        [Test]
        public void TestGetTransactionComplete() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                Assert.IsNotNull(broker.Get(new object()));
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le rollback du contexte transactionnel de Get.
        /// </summary>
        [Test]
        [ExpectedException(typeof(TransactionAbortedException))]
        public void TestGetTransactionRollback() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = true;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                try {
                    broker.Get(new object());
                } catch {
                    // On intercepte l'exception.
                }
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test l'appel à la méthode Get avec un paramètre null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGetNull() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            broker.Get(null);
        }

        /// <summary>
        /// Test le bon fonctionnement du GetAll.
        /// </summary>
        [Test]
        public void TestGetAll() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            Assert.IsNotNull(broker.GetAll());
        }

        /// <summary>
        /// Test le bon fonctionnement du GetAllLike.
        /// </summary>
        [Test]
        public void TestGetLike() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            Assert.IsNotNull(broker.GetByCriteria(new FilterCriteria()));
        }

        /// <summary>
        /// Test le bon fonctionnement du GetAllLike.
        /// </summary>
        [Test]
        public void TestGetAllLike() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            Assert.IsNotNull(broker.GetAllByCriteria(new FilterCriteria(), new QueryParameter(3)));
        }

        /// <summary>
        /// Test la présence d'un contexte transactionnel pour GetAll.
        /// </summary>
        [Test]
        public void TestGetAllTransactionScope() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            Assert.IsNotNull(broker.GetAll());
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test la présence d'un contexte transactionnel pour GetAllLike.
        /// </summary>
        [Test]
        public void TestGetAllLikeTransactionScope() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            Assert.IsNotNull(broker.GetAllByCriteria(new FilterCriteria()));
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le complete du contexte transactionnel de GetAll.
        /// </summary>
        [Test]
        public void TestGetAllTransactionComplete() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                Assert.IsNotNull(broker.GetAll());
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le complete du contexte transactionnel de GetAllLike.
        /// </summary>
        [Test]
        public void TestGetAllLikeTransactionComplete() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                Assert.IsNotNull(broker.GetAllByCriteria(new FilterCriteria()));
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le rollback du contexte transactionnel de GetAll.
        /// </summary>
        [Test]
        [ExpectedException(typeof(TransactionAbortedException))]
        public void TestGetAllTransactionRollback() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = true;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                try {
                    broker.GetAll();
                } catch {
                    // On intercepte l'exception.
                }
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le rollback du contexte transactionnel de GetAllLike.
        /// </summary>
        [Test]
        [ExpectedException(typeof(TransactionAbortedException))]
        public void TestGetAllLikeTransactionRollback() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = true;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                try {
                    broker.GetAllByCriteria(new FilterCriteria());
                } catch {
                    // On intercepte l'exception.
                }
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test l'appel à la méthode GetAll retournant plusieurs lignes.
        /// </summary>
        [Test]
        public void TestGetAllManyRows() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            ICollection<Bean> coll = broker.GetAll(new QueryParameter(5));
            Assert.AreEqual(5, coll.Count);
        }

        /// <summary>
        /// Test l'appel à la méthode GetAllLike retournant plusieurs lignes.
        /// </summary>
        [Test]
        public void TestGetAllLikeManyRows() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            ICollection<Bean> coll = broker.GetAllByCriteria(new FilterCriteria(), new QueryParameter(5));
            Assert.AreEqual(5, coll.Count);
        }

        /// <summary>
        /// Test le bon fonctionnement du Save.
        /// </summary>
        [Test]
        public void TestSave() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            Assert.IsNotNull(broker.Save(new Bean(), null));
        }

        /// <summary>
        /// Test la présence d'un contexte transactionnel pour Save.
        /// </summary>
        [Test]
        public void TestSaveTransactionScope() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            Assert.IsNotNull(broker.Save(new Bean(), null));
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le complete du contexte transactionnel de Save.
        /// </summary>
        [Test]
        public void TestSaveTransactionComplete() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                Assert.IsNotNull(broker.Save(new Bean(), null));
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test le rollback du contexte transactionnel de Save.
        /// </summary>
        [Test]
        [ExpectedException(typeof(TransactionAbortedException))]
        public void TestSaveTransactionRollback() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = true;
            using (TransactionScope tx = new TransactionScope(TransactionScopeOption.Required)) {
                try {
                    broker.Save(new Bean(), null);
                } catch {
                    // On intercepte l'exception.
                }
                tx.Complete();
            }
            Assert.IsTrue(TestStore<Bean>.IsCallWithTransactionScope);
        }

        /// <summary>
        /// Test l'appel à la méthode Save avec un paramètre null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSaveNull() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            broker.Save(null, null);
        }

        /// <summary>
        /// Test l'appel à la méthode SaveAll avec un paramètre null.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestSaveAllNull() {
            StandardBroker<Bean> broker = new StandardBroker<Bean>("name");
            TestStore<Bean>.ExceptionOnCall = false;
            broker.SaveAll(null);
        }

        /// <summary>
        /// Test l'appel à la méthode SaveAll.
        /// </summary>
        [Test]
        public void TestSaveAll() {
            StandardBroker<StateBean> broker = new StandardBroker<StateBean>("name");
            TestStore<StateBean>.ExceptionOnCall = false;
            TestStore<StateBean>.PutList.Clear();
            TestStore<StateBean>.RemoveList.Clear();

            List<StateBean> list = new List<StateBean>();
            StateBean bean = new StateBean();
            bean.Id = 1;
            bean.Name = "1";
            bean.State = ChangeAction.Insert;
            list.Add(bean);

            bean = new StateBean();
            bean.Id = 2;
            bean.Name = "2";
            bean.State = ChangeAction.Delete;
            list.Add(bean);

            bean = new StateBean();
            bean.Id = 3;
            bean.Name = "3";
            bean.State = ChangeAction.Update;
            list.Add(bean);

            bean = new StateBean();
            bean.Id = 4;
            bean.Name = "4";
            bean.State = ChangeAction.None;
            list.Add(bean);

            broker.SaveAll(list);

            bool isAddedPut = false;
            bool isModifiedPut = false;
            foreach (StateBean b in TestStore<StateBean>.PutList) {
                if (b.State == ChangeAction.Insert) {
                    if (isAddedPut) {
                        Assert.Fail();
                    }
                    isAddedPut = true;
                } else if (b.State == ChangeAction.Update) {
                    if (isModifiedPut) {
                        Assert.Fail();
                    }
                    isModifiedPut = true;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsTrue(isAddedPut);
            Assert.IsTrue(isModifiedPut);

            bool isDeletedRemove = false;
            foreach (object pk in TestStore<StateBean>.RemoveList) {
                if ((int)pk == 2) {
                    if (isDeletedRemove) {
                        Assert.Fail();
                    }
                    isDeletedRemove = true;
                } else {
                    Assert.Fail();
                }
            }
            Assert.IsTrue(isDeletedRemove);
        }
    }
}
