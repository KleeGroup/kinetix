using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using Kinetix.ComponentModel;
using Kinetix.ServiceModel;

namespace Kinetix.Broker {

    /// <summary>
    /// Broker spécifique pour les listes de référence, permettant de gérer les traductions.
    /// </summary>
    /// <typeparam name="T">Type du bean.</typeparam>
    public class ReferenceBroker<T> : StandardBroker<T>
        where T : class, new() {

        /// <summary>
        /// Service de lecture des ressources.
        /// </summary>
        private readonly IResourceLoader _resourceLoader;

        /// <summary>
        /// Service de sauvegarde des ressources.
        /// </summary>
        private readonly IResourceWriter _resourceWriter;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="dataSourceName">Nom de la source de données.</param>
        /// <param name="resourceLoader">Service de chargement des ressources.</param>
        /// <param name="resourceWriter">Service d'écriture des ressources.</param>
        public ReferenceBroker(string dataSourceName, IResourceLoader resourceLoader, IResourceWriter resourceWriter)
            : base(dataSourceName) {
            if (resourceLoader == null) {
                throw new ArgumentNullException("resourceLoader");
            }

            if (resourceWriter == null) {
                throw new ArgumentNullException("resourceWriter");
            }

            _resourceLoader = resourceLoader;
            _resourceWriter = resourceWriter;
        }

        /// <summary>
        /// Suppression d'un bean.
        /// </summary>
        /// <param name="primaryKey">Clé de l'objet à supprimer.</param>
        public override void Delete(object primaryKey) {
            _resourceWriter.DeleteTraductionReferenceByReferenceAndPrimaryKey(typeof(T), primaryKey);
            base.Delete(primaryKey);
        }

        /// <summary>
        /// Sauvegarde d'un bean.
        /// </summary>
        /// <param name="bean">Bean à sauvegarder.</param>
        /// <param name="columnSelector">Column Selector.</param>
        /// <returns>Clé primaire de l'objet.</returns>
        public override object Save(T bean, ColumnSelector columnSelector) {
            BeanDefinition definition = BeanDescriptor.GetDefinition(typeof(T));
            ICollection<BeanPropertyDescriptor> propList = definition.Properties.Where(x => !x.PropertyType.Name.Equals(typeof(ChangeAction).Name)
                && x.MemberName != null).ToList();
            T beanToSave = new T();

            /* Mise à jour du bean : on ne sauvegarde que les labels en anglais ou les champs hors chaînes de caractères */
            string defaultLanguage = _resourceLoader.LoadLangueCodeDefaut();
            string lanCode = _resourceLoader.LoadCurrentLangueCode();

            if (definition.PrimaryKey.GetValue(bean) != null && !lanCode.Equals(defaultLanguage) && definition.IsTranslatable) {
                T beanOld = ReferenceManager.Instance.GetReferenceObjectByPrimaryKey<T>(definition.PrimaryKey.GetValue(bean));
                foreach (BeanPropertyDescriptor property in propList) {
                    if (property.IsTranslatable) {
                        /* la propriété est traduisible : on garde la valeur courante sur la table du bean
                         * et sauvegardera la valeur dans la table Traductionreference. */
                        property.SetValue(beanToSave, property.GetValue(beanOld));
                    } else {
                        /* La propriété est non traduisible : on sauvegarde la valeur normalement sur la table du bean. */
                        property.SetValue(beanToSave, property.GetValue(bean));
                    }
                }
            } else {
                beanToSave = bean;
            }

            object o = base.Save(beanToSave, columnSelector);
            definition.PrimaryKey.SetValue(bean, o);

            _resourceWriter.SaveTraductionReference(typeof(T), bean, lanCode);

            return o;
        }

        /// <summary>
        ///  Sauvegarde un ensemble de beans.
        /// </summary>
        /// <param name="values">Beans à sauvegarder.</param>
        /// <param name="columnSelector">Column Selector.</param>
        public override void SaveAll(ICollection<T> values, ColumnSelector columnSelector) {
            if (values == null) {
                throw new ArgumentNullException("values");
            }

            foreach (T val in values) {
                this.Save(val, columnSelector);
            }
        }

        /// <summary>
        /// Retourne le store à utiliser.
        /// </summary>
        /// <param name="dataSourceName">Source de données.</param>
        /// <returns>Store.</returns>
        protected override IStore<T> CreateStore(string dataSourceName) {
            Type storeType = typeof(ReferenceSqlServerStore<>);
            Type realStoreType = storeType.MakeGenericType(typeof(T));
            return (IStore<T>)Activator.CreateInstance(realStoreType, dataSourceName);
        }
    }
}
