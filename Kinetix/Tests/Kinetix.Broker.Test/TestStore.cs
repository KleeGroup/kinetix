using System;
using System.Collections.Generic;
using System.Transactions;
using Kinetix.Data.SqlClient;

namespace Kinetix.Broker.Test {
    /// <summary>
    /// Store de test du broker.
    /// </summary>
    /// <typeparam name="T">Type du store.</typeparam>
    public class TestStore<T> : IStore<T> where T : new() {

        private static readonly List<T> _putList = new List<T>();
        private static readonly List<object> _removeList = new List<object>();
        private readonly List<IStoreRule> _rules = new List<IStoreRule>();
        private static bool _callWithTransactionScope;
        private static bool _exceptionOnCall;
        private string _name;

        /// <summary>
        /// Crée un nouveau store.
        /// </summary>
        /// <param name="name">Nom de la base de données utilisées.</param>
        public TestStore(string name) {
            this._name = name;
        }

        /// <summary>
        /// Indique si un appel a eu lieu avec un contexte transactionnel.
        /// </summary>
        public static bool IsCallWithTransactionScope {
            get {
                return _callWithTransactionScope;
            }
        }

        /// <summary>
        /// Obtient ou définit si une exception est levée lors de l'appel.
        /// </summary>
        public static bool ExceptionOnCall {
            get {
                return _exceptionOnCall;
            }
            set {
                _exceptionOnCall = value;
            }
        }

        /// <summary>
        /// Retourne la collection des objects mise à jour.
        /// </summary>
        public static ICollection<T> PutList {
            get {
                return _putList;
            }
        }

        /// <summary>
        /// Retourne la collection des clefs primaires des objects supprimés.
        /// </summary>
        public static ICollection<object> RemoveList {
            get {
                return _removeList;
            }
        }

        /// <summary>
        /// Retourne la liste des régles appliquées par le store.
        /// </summary>
        public ICollection<IStoreRule> Rules {
            get {
                return _rules;
            }
        }

        /// <summary>
        /// Ajoute une règle de gestion au store.
        /// </summary>
        /// <param name="rule">Règle de gestion.</param>
        public void AddRule(IStoreRule rule) {
            _rules.Add(rule);
        }

        /// <summary>
        /// Charge un objet à partir de la clef primaire.
        /// </summary>
        /// <param name="destination">Le bean à charger.</param>
        /// <param name="primaryKey">Clef primaire à charger.</param>
        /// <returns>Objet chargé.</returns>
        public T Load(T destination, object primaryKey) {
            _callWithTransactionScope = Transaction.Current != null;
            if (_exceptionOnCall) {
                throw new Exception();
            }
            return new T();
        }

        /// <summary>
        /// Charge une collection d'objet.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection d'objet.</returns>
        public ICollection<T> LoadAll(ICollection<T> collection, QueryParameter queryParameter) {
            _callWithTransactionScope = Transaction.Current != null;
            if (_exceptionOnCall) {
                throw new Exception();
            }
            ICollection<T> list = collection ?? new List<T>();
            if (queryParameter.Limit > 5) {
                for (int i = 0; i < queryParameter.Limit + 1; i++) {
                    list.Add(new T());
                }
            } else {
                for (int i = 0; i < queryParameter.Limit; i++) {
                    list.Add(new T());
                }
            }
            return list;
        }

        /// <summary>
        /// Charge une collection d'objet à partir de
        /// critères de recherche.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="criteria">Critères de recherche.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection d'objet.</returns>
        public ICollection<T> LoadAllByCriteria(ICollection<T> collection, FilterCriteria criteria, QueryParameter queryParameter) {
            return LoadAll(collection, queryParameter);
        }

        /// <summary>
        /// Charge un objet à partir de critère de recherche.
        /// </summary>
        /// <param name="destination">Bean à charger.</param>
        /// <param name="criteria">Les critères de recherche.</param>
        /// <returns>Bean.</returns>
        public T LoadByCriteria(T destination, FilterCriteria criteria) {
            return new T();
        }

        /// <summary>
        /// Enregistre un objet dans le store.
        /// </summary>
        /// <param name="bean">Objet à enregistrer.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
        /// <returns>Clef primaire de l'objet.</returns>
        public object Put(T bean, ColumnSelector columnSelector) {
            _callWithTransactionScope = Transaction.Current != null;
            if (_exceptionOnCall) {
                throw new Exception();
            }
            _putList.Add(bean);
            return 1;
        }

        /// <summary>
        /// Supprime un objet du store.
        /// </summary>
        /// <param name="primaryKey">Clef primaire de l'objet à supprimer.</param>
        public void Remove(object primaryKey) {
            _callWithTransactionScope = Transaction.Current != null;
            if (_exceptionOnCall) {
                throw new Exception();
            }
            _removeList.Add(primaryKey);
        }

        /// <summary>
        /// Supprime tous les objets correspondant aux critères.
        /// </summary>
        /// <param name="criteria">Critères de suppression.</param>
        public void RemoveAllByCriteria(FilterCriteria criteria) {
            _callWithTransactionScope = Transaction.Current != null;
            if (_exceptionOnCall) {
                throw new Exception();
            }
        }

        /// <summary>
        /// Compte le nombre d'occurence de l'objet correspondant
        /// aux critères de recherche.
        /// </summary>
        /// <param name="criteria">Critères de recherche.</param>
        /// <returns>Le nombre d'occurence correspondant aux critères de recherche.</returns>
        public int Count(FilterCriteria criteria) {
            _callWithTransactionScope = Transaction.Current != null;
            if (_exceptionOnCall) {
                throw new Exception();
            }
            return Int32.MaxValue;
        }

        /// <summary>
        /// Insère tous les éléments.
        /// </summary>
        /// <param name="collection">Elements a insérer.</param>
        /// <returns>Liste des éléments insérés.</returns>
        public ICollection<T> PutAll(ICollection<T> collection) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the object is used by another object in the application.
        /// </summary>
        /// <param name="primaryKey">Id of the object.</param>
        /// <param name="tablesToIgnore">A collection of tables to ignore when looking for tables that depend on the object.</param>
        /// <returns>True if the object is used by another object.</returns>
        public bool IsUsed(object primaryKey, ICollection<string> tablesToIgnore = null) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the object is used by another object in the application.
        /// </summary>
        /// <param name="primaryKeys">Id of the object.</param>
        /// <param name="tablesToIgnore">A collection of tables to ignore when looking for tables that depend on the object.</param>
        /// <returns>True if the object is used by another object.</returns>
        public bool AreUsed(ICollection<int> primaryKeys, ICollection<string> tablesToIgnore = null) {
            throw new NotImplementedException();
        }
    }
}
