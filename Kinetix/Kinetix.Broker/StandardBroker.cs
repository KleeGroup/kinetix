using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Transactions;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;
using Kinetix.ServiceModel;

namespace Kinetix.Broker {
    /// <summary>
    /// Broker par défaut.
    /// La gestion des transactions est prise en charge par ce broker.
    /// </summary>
    /// <typeparam name="T">Type du bean.</typeparam>
    public class StandardBroker<T> : IBroker<T>
        where T : class, new() {

        /// <summary>
        /// Champs obligatoire lié à l'attribut Optimistic locking : Date de modification.
        /// </summary>
        private const string DateModif = "ModificationDate";

        /// <summary>
        /// Champs obligatoire lié à l'attribut Optimistic locking : Date de création.
        /// </summary>
        private const string DateCreation = "CreationDate";

        /// <summary>
        /// Champ obligatoire: Utilisateur modification.
        /// </summary>
        private const string UserModif = "UserIdModification";

        /// <summary>
        /// Champ obligatoire: Utilisateur création.
        /// </summary>
        private const string UserCreation = "UserIdCreation";

        private readonly IStore<T> _store;
        private readonly bool _hasCache;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="dataSourceName">Nom de la source de données.</param>
        public StandardBroker(string dataSourceName) {
            if (dataSourceName == null) {
                throw new ArgumentNullException("dataSourceName");
            }

            object[] attr = typeof(T).GetCustomAttributes(typeof(ReferenceAttribute), false);
            ReferenceAttribute[] referenceAttr = (ReferenceAttribute[])attr;
            _hasCache = referenceAttr != null && referenceAttr.Length == 1;

            _store = CreateStore(dataSourceName);

            this.AddRule(new CreationDateRule(DateCreation));
            this.AddRule(new ModificationDateRule(DateModif));
            this.AddRule(new CreationUserRule(UserCreation));
            this.AddRule(new ModificationUserRule(UserModif));
        }

        /// <summary>
        /// Retourne la liste des régles appliquées par le store.
        /// </summary>
        public ICollection<IStoreRule> StoreRules {
            get {
                return _store.Rules;
            }
        }

        /// <summary>
        /// Supprime un bean à partir de sa clef primaire.
        /// </summary>
        /// <param name="primaryKey">Clef primaire de l'objet.</param>
        public virtual void Delete(object primaryKey) {
            if (primaryKey == null) {
                throw new ArgumentNullException("primaryKey");
            }

            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                _store.Remove(primaryKey);
                tx.Complete();
            }

            if (_hasCache) {
                FlushCache();
            }
        }

        /// <summary>
        /// Supprime plusieurs beans à partir de leur clé primaire.
        /// </summary>
        /// <param name="primaryKeys">Clef primaires des objets.</param>
        public virtual void DeleteCollection(ICollection<int> primaryKeys) {
            if (primaryKeys == null) {
                throw new ArgumentNullException("primaryKeys");
            }

            foreach (object primaryKey in primaryKeys) {
                Delete(primaryKey);
            }
        }

        /// <summary>
        /// Supprimé tous les objets correspondant aux critères.
        /// </summary>
        /// <param name="bean">Critères de suppression.</param>
        public void DeleteAllByCriteria(T bean) {
            DeleteAllByCriteria(new FilterCriteria(bean));
        }

        /// <summary>
        /// Supprimé tous les objets correspondant aux critères.
        /// </summary>
        /// <param name="criteria">Critères de suppression.</param>
        public virtual void DeleteAllByCriteria(FilterCriteria criteria) {
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                _store.RemoveAllByCriteria(criteria);
                tx.Complete();
            }

            if (_hasCache) {
                FlushCache();
            }
        }

        /// <summary>
        /// Retourne un bean à partir de sa clef primaire.
        /// </summary>
        /// <param name="primaryKey">Valeur de la clef primaire.</param>
        /// <returns>Bean.</returns>
        public virtual T Get(object primaryKey) {
            T bean = new T();
            Load(bean, primaryKey);
            return bean;
        }

        /// <summary>
        /// Retourne tous les beans pour un type.
        /// </summary>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection.</returns>
        public virtual ICollection<T> GetAll(QueryParameter queryParameter = null) {
            ICollection<T> coll;
            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                coll = _store.LoadAll(null, queryParameter);
                tx.Complete();
            }

            return coll;
        }

        /// <summary>
        /// Charge dans un objet le bean à partir de sa clef primaire.
        /// </summary>
        /// <param name="destination">Objet à charger.</param>
        /// <param name="primaryKey">Valeur de la clef primaire.</param>
        public virtual void Load(T destination, object primaryKey) {
            if (primaryKey == null) {
                throw new ArgumentNullException("primaryKey");
            }

            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                _store.Load(destination, primaryKey);
                tx.Complete();
            }
        }

        /// <summary>
        /// Charge dans une collection tous les beans pour un type.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        public virtual void LoadAll(ICollection<T> collection, QueryParameter queryParameter = null) {
            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                _store.LoadAll(collection, queryParameter);
                tx.Complete();
            }
        }

        /// <summary>
        /// Retourne tous les beans pour un type suivant
        /// une liste de critères donnés.
        /// </summary>
        /// <param name="criteria">Critères de sélection.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection.</returns>
        public virtual ICollection<T> GetAllByCriteria(FilterCriteria criteria, QueryParameter queryParameter = null) {
            ICollection<T> coll;
            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                coll = _store.LoadAllByCriteria(null, criteria, queryParameter);
                tx.Complete();
            }

            return coll;
        }

        /// <inheritdoc cref="IBroker{T}.GetAllByCriteria(T, QueryParameter)" />
        public ICollection<T> GetAllByCriteria(T bean, QueryParameter queryParameter = null) {
            return GetAllByCriteria(new FilterCriteria(bean), queryParameter);
        }

        /// <summary>
        /// Charge dans une collection tous les beans pour un type suivant
        /// une liste de critères donnés.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="criteria">Liste des critères.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        public virtual void LoadAllByCriteria(ICollection<T> collection, FilterCriteria criteria, QueryParameter queryParameter = null) {
            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                _store.LoadAllByCriteria(collection, criteria, queryParameter);
                tx.Complete();
            }
        }

        /// <summary>
        /// Retourne un bean à partir d'un critère de recherche.
        /// </summary>
        /// <param name="criteria">Le critère de recherche.</param>
        /// <returns>Bean.</returns>
        /// <exception cref="CollectionBuilderException">Si la recherche renvoie plus d'un élément.</exception>
        /// <exception cref="CollectionBuilderException">Si la recherche ne renvoit pas d'élément.</exception>
        public virtual T GetByCriteria(FilterCriteria criteria) {
            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                T value = _store.LoadByCriteria(null, criteria);
                tx.Complete();
                return value;
            }
        }

        /// <summary>
        /// Retourne un bean à partir d'un critère de recherche.
        /// </summary>
        /// <param name="criteria">Le critère de recherche.</param>
        /// <returns>Bean ou null si l'élément n'a pas été trouvé.</returns>
        /// <exception cref="CollectionBuilderException">Si la recherche renvoie plus d'un élément.</exception>
        public virtual T FindByCriteria(FilterCriteria criteria)
        {
            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required))
            {
                T value = _store.LoadByCriteria(null, criteria, true);
                tx.Complete();
                return value;
            }
        }

        /// <summary>
        /// Retourne un bean à partir d'un critère de recherche.
        /// </summary>
        /// <param name="destination">Bean à charger.</param>
        /// <param name="criteria">Le critère de recherche.</param>
        /// <exception cref="NotSupportedException">Si la recherche renvoie plus d'un élément.</exception>
        public virtual void LoadByCriteria(T destination, FilterCriteria criteria) {
            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                T value = _store.LoadByCriteria(destination, criteria);
                tx.Complete();
            }
        }

        /// <summary>
        /// Sauvegarde un bean.
        /// </summary>
        /// <param name="bean">Bean à enregistrer.</param>
        /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou ignorer.</param>
        /// <returns>Clef primaire.</returns>
        public virtual object Save(T bean, ColumnSelector columnSelector) {
            if (bean == null) {
                throw new ArgumentNullException("bean");
            }

            object primaryKey;
            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                primaryKey = _store.Put(bean, columnSelector);
                tx.Complete();
            }

            if (_hasCache) {
                FlushCache();
            }

            return primaryKey;
        }

        /// <summary>
        /// Sauvegarde l'ensemble des éléments d'une association n-n.
        /// </summary>
        /// <param name="values">Les valeurs à ajouter via associations.</param>
        /// <param name="columnSelector">Sélecteur de colonnes à mettre à jour.</param>
        /// <exception cref="ArgumentException">Si la collection n'est pas composée d'objets implémentant l'interface IBeanState.</exception>
        public virtual void SaveAll(ICollection<T> values, ColumnSelector columnSelector = null) {
            if (values == null) {
                throw new ArgumentNullException("values");
            }

            BeanPropertyDescriptor primaryKey = BeanDescriptor.GetDefinition(typeof(T), true).PrimaryKey;
            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                if (typeof(IBeanState).IsAssignableFrom(typeof(T))) {
                    foreach (T value in values) {
                        switch (((IBeanState)value).State) {
                            case ChangeAction.Insert:
                            case ChangeAction.Update:
                                _store.Put(value, columnSelector);
                                break;
                            case ChangeAction.Delete:
                                _store.Remove(primaryKey.GetValue(value));
                                break;
                            default:
                                break;
                        }
                    }
                } else {
                    foreach (T value in values) {
                        _store.Put(value, columnSelector);
                    }
                }

                tx.Complete();
            }

            if (_hasCache) {
                FlushCache();
            }
        }

        /// <summary>
        /// Insére l'ensemble des éléments.
        /// </summary>
        /// <param name="values">Valeurs à insérer.</param>
        /// <returns>Valeurs insérées.</returns>
        public ICollection<T> InsertAll(ICollection<T> values) {
            if (values == null) {
                throw new ArgumentNullException("values");
            }

            using (ServiceScope tx = new ServiceScope(TransactionScopeOption.Required)) {
                ICollection<T> result = _store.PutAll(values);
                tx.Complete();
                return result;
            }
        }

        /// <summary>
        /// Vérifie si l'objet est utilisé.
        /// </summary>
        /// <param name="primaryKey">Clé primaire de l'objet à vérifier.</param>
        /// <param name="tablesToIgnore">Tables dépendantes à ignorer</param>
        /// <returns>True si l'objet est utilisé.</returns>
        public bool IsUsed(object primaryKey, ICollection<string> tablesToIgnore = null) {
            return _store.IsUsed(primaryKey, tablesToIgnore);
        }

        /// <summary>
        /// Vérifie si au moins un objet dans la collection est utilisé.
        /// </summary>
        /// <param name="primaryKeys">Clés primaires des objets à vérifier.</param>
        /// <param name="tablesToIgnore">Tables dépendantes à ignorer</param>
        /// <returns>True si au moins un objet est utilisé.</returns>
        public bool AreUsed(ICollection<int> primaryKeys, ICollection<string> tablesToIgnore = null) {
            return _store.AreUsed(primaryKeys, tablesToIgnore);
        }

        /// <summary>
        /// Ajoute une règle de gestion au store.
        /// </summary>
        /// <param name="rule">Règle de gestion.</param>
        public void AddRule(IStoreRule rule) {
            _store.AddRule(rule);
        }

        /// <summary>
        /// Retourne le store à utiliser.
        /// </summary>
        /// <param name="dataSourceName">Source de données.</param>
        /// <returns>Store.</returns>
        protected virtual IStore<T> CreateStore(string dataSourceName) {
            Type storeType = BrokerManager.Instance.GetStoreType(dataSourceName);
            Type realStoreType = storeType.MakeGenericType(typeof(T));
            return (IStore<T>)Activator.CreateInstance(realStoreType, dataSourceName);
        }

        /// <summary>
        /// Flush du cache.
        /// </summary>
        private static void FlushCache() {
            ((IReferenceManager)ReferenceManager.Instance).FlushCache(typeof(T));
        }
    }
}
