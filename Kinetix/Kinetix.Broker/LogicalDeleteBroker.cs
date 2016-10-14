using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using Kinetix.ComponentModel;
using Kinetix.Data.SqlClient;

namespace Kinetix.Broker {

    /// <summary>
    /// Broker prenant en charge la suppression logique.
    /// Se base sur la propriété booléenne IsActif pour effectuer la mise à jour.
    /// </summary>
    /// <typeparam name="T">Bean.</typeparam>
    public class LogicalDeleteBroker<T> : StandardBroker<T>
        where T : class, new() {

        private readonly string _propertyName;
        private readonly string _pkName;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="dataSourceName">Nom de la source de données.</param>
        public LogicalDeleteBroker(string dataSourceName)
            : base(dataSourceName) {
            BeanDefinition definition = BeanDescriptor.GetDefinition(typeof(T));
            if (definition.PrimaryKey == null) {
                throw new NotSupportedException("Pas de primary key");
            }

            _pkName = definition.PrimaryKey.MemberName;
            if (!definition.Properties.Contains("IsActif")) {
                throw new NotSupportedException("Aucune propriété 'IsActif' trouvée");
            }

            _propertyName = definition.Properties["IsActif"].MemberName;
        }

        /// <summary>
        /// Supprime logiquement l'élément correspondant à la clef primaire.
        /// </summary>
        /// <param name="primaryKey">Clef primaire.</param>
        public override void Delete(object primaryKey) {
            if (primaryKey == null) {
                throw new ArgumentNullException("primaryKey");
            }

            T bean = this.Get(primaryKey);
            TypeDescriptor.GetProperties(typeof(T))["IsActif"].SetValue(bean, false);
            this.Save(bean, null);
        }

        /// <summary>
        /// Supprime logiquement l'ensemble des éléments correspondant au critère de recherche.
        /// </summary>
        /// <param name="criteria">Critère de recherche.</param>
        public override void DeleteAllByCriteria(FilterCriteria criteria) {
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            bool implementsIBeanState = typeof(IBeanState).IsAssignableFrom(typeof(T));
            ICollection<T> list = this.GetAllByCriteria(criteria, null);
            foreach (T bean in list) {
                TypeDescriptor.GetProperties(typeof(T))["IsActif"].SetValue(bean, false);
                if (implementsIBeanState) {
                    ((IBeanState)bean).State = ChangeAction.Update;
                }
            }

            SaveAll(list);
        }

        /// <summary>
        /// Retourne un bean à partir de sa clef primaire.
        /// </summary>
        /// <param name="primaryKey">Valeur de la clef primaire.</param>
        /// <returns>Bean.</returns>
        public override T Get(object primaryKey) {
            if (primaryKey == null) {
                throw new ArgumentNullException("primaryKey");
            }

            FilterCriteria criteria = new FilterCriteria();
            criteria.AddCriteria(_pkName, Expression.Equals, primaryKey);
            criteria.AddCriteria(_propertyName, Expression.Equals, true);
            return this.GetByCriteria(criteria);
        }

        /// <summary>
        /// Retourne un bean à partir d'un critère de recherche.
        /// </summary>
        /// <param name="criteria">Le critère de recherche.</param>
        /// <returns>Bean.</returns>
        /// <exception cref="CollectionBuilderException">Si la recherche renvoie plus d'un élément.</exception>
        /// <exception cref="CollectionBuilderException">Si la recherche ne renvoit pas d'élément.</exception>
        public override T GetByCriteria(FilterCriteria criteria) {
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            return base.GetByCriteria(criteria);
        }

        /// <summary>
        /// Retourne un bean à partir d'un critère de recherche.
        /// </summary>
        /// <param name="criteria">Le critère de recherche.</param>
        /// <returns>Bean ou null si l'élément n'a pas été trouvé.</returns>
        /// <exception cref="CollectionBuilderException">Si la recherche renvoie plus d'un élément.</exception>
        public override T FindByCriteria(FilterCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria");
            }

            return base.FindByCriteria(criteria);
        }

        /// <summary>
        /// Retourne la liste triée d'éléments T actifs.
        /// </summary>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection.</returns>
        public override ICollection<T> GetAll(QueryParameter queryParameter = null) {
            FilterCriteria criteria = new FilterCriteria();
            criteria.AddCriteria(_propertyName, Expression.Equals, true);
            return this.GetAllByCriteria(criteria, queryParameter);
        }

        /// <summary>
        /// Retourne tous les beans pour un type suivant
        /// une liste de critères donnés.
        /// </summary>
        /// <param name="criteria">Critère.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        /// <returns>Collection.</returns>
        public override ICollection<T> GetAllByCriteria(FilterCriteria criteria, QueryParameter queryParameter = null) {
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            criteria.AddCriteria(_propertyName, Expression.Equals, true);
            return base.GetAllByCriteria(criteria, queryParameter);
        }

        /// <summary>
        /// Charge dans un objet le bean à partir de sa clef primaire.
        /// </summary>
        /// <param name="destination">Objet à charger.</param>
        /// <param name="primaryKey">Valeur de la clef primaire.</param>
        public override void Load(T destination, object primaryKey) {
            if (primaryKey == null) {
                throw new ArgumentNullException("primaryKey");
            }

            FilterCriteria criteria = new FilterCriteria();
            criteria.AddCriteria(_pkName, Expression.Equals, primaryKey);
            criteria.AddCriteria(_propertyName, Expression.Equals, true);
            T newObject = GetByCriteria(criteria);

            BeanFactory<T> factory = new BeanFactory<T>();
            factory.CloneBean(newObject, destination);
        }

        /// <summary>
        /// Charge dans une collection tous les beans actifs pour un type.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        public override void LoadAll(ICollection<T> collection, QueryParameter queryParameter = null) {
            FilterCriteria criteria = new FilterCriteria();
            criteria.AddCriteria(_propertyName, Expression.Equals, true);
            this.LoadAllByCriteria(collection, criteria, queryParameter);
        }

        /// <summary>
        /// Charge dans une collection tous les beans pour un type suivant
        /// une liste de critères donnés.
        /// </summary>
        /// <param name="collection">Collection à charger.</param>
        /// <param name="criteria">Critère.</param>
        /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
        public override void LoadAllByCriteria(ICollection<T> collection, FilterCriteria criteria, QueryParameter queryParameter = null) {
            if (criteria == null) {
                throw new ArgumentNullException("criteria");
            }

            criteria.AddCriteria(_propertyName, Expression.Equals, true);
            base.LoadAllByCriteria(collection, criteria, queryParameter);
        }
    }
}
