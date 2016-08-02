using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Kinetix.Caching;
using Kinetix.ComponentModel;
using log4net;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Gestionnaire des données de références.
    /// </summary>
    public sealed class ReferenceManager : IReferenceManager {

        /// <summary>
        /// Nom du cache associé aux listes de référence.
        /// </summary>
        private const string ReferenceCache = "ReferenceLists";

        /// <summary>
        /// Nom du cache associé aux listes statiques.
        /// </summary>
        private const string StaticCache = "StaticLists";

        /// <summary>
        /// Instance singleton.
        /// </summary>
        private static readonly ReferenceManager _instance = new ReferenceManager();

        /// <summary>
        /// Méthode chargeant les listes de références.
        /// </summary>
        /// <remarks>
        /// Point d'injection utilisé dans les tests unitaires pour stuber le chargement depuis la base.
        /// </remarks>
        private static ReferenceListLoaderDelegate referenceListLoader = LoadReferenceListWithReferenceAccessor;

        /// <summary>
        /// Crée un nouveau manager.
        /// </summary>
        private ReferenceManager() {
        }

        /// <summary>
        /// Délégué pour la méthode qui charge une liste de référence à partir de son type.
        /// </summary>
        /// <param name="referenceType">Type à charger.</param>
        /// <returns>Liste des éléments de la liste de référence.</returns>
        public delegate ICollection ReferenceListLoaderDelegate(Type referenceType);

        /// <summary>
        /// Retourne une instance du manager.
        /// </summary>
        public static ReferenceManager Instance {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Flush all cache.
        /// </summary>
        public void FlushAll() {
            CacheManager.Instance.GetCache(StaticCache).RemoveAll();
            CacheManager.Instance.GetCache(ReferenceCache).RemoveAll();
        }

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <returns>Liste de référence.</returns>
        public object GetReferenceList(Type referenceType) {
            return this.GetReferenceList(referenceType, null);
        }

        /// <summary>
        /// Retourne la liste de référence du type TReferenceType.
        /// </summary>
        /// <param name="referenceName">Nom de la liste de référence à utiliser.</param>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <returns>Liste de référence.</returns>
        public ICollection<TReferenceType> GetReferenceList<TReferenceType>(string referenceName = null) {
            return (ICollection<TReferenceType>)GetReferenceList(typeof(TReferenceType), referenceName);
        }

        /// <summary>
        /// Retourne les éléments de la liste de référence du type TReference correspondant au prédicat.
        /// </summary>
        /// <typeparam name="TReference">Type de la liste de référence.</typeparam>
        /// <param name="predicate">Prédicat de filtrage.</param>
        /// <param name="referenceName">Nom de la liste de référence.</param>
        /// <returns>Ensemble des éléments.</returns>
        public ICollection<TReference> GetReferenceList<TReference>(Func<TReference, bool> predicate, string referenceName = null) {
            if (predicate == null) {
                throw new ArgumentNullException("predicate");
            }

            IEnumerable<TReference> list = (IEnumerable<TReference>)GetReferenceList<TReference>(referenceName);
            return list.Where(predicate).ToList<TReference>();
        }

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="referenceName">Nom de la liste de référence à utiliser.</param>
        /// <returns>Liste de référence.</returns>
        public object GetReferenceList(Type referenceType, string referenceName) {
            if (referenceType == null) {
                throw new ArgumentNullException("referenceType");
            }

            // Si l'élément n'est pas mis en cache (pas d'attribut Reference de mis sur le type), on appelle directement le service de chargement.
            if (!string.IsNullOrEmpty(referenceName)) {
                return ServiceManager.Instance.InvokeReferenceAccessor(referenceType, referenceName);
            }

            if (!HasCache(referenceType)) {
                return ServiceManager.Instance.InvokeReferenceAccessor(referenceType, null);
            }

            IReferenceEntry entry = GetReferenceEntry(referenceType);
            object cachedList = entry.GetReferenceList(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpperInvariant());
            Type typedList = typeof(List<>).MakeGenericType(referenceType);
            return Activator.CreateInstance(typedList, (IEnumerable)cachedList);
        }

        /// <summary>
        /// Retourne la liste de référence du type TReferenceType à partir d'un objet de critères.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="criteria">Objet contenant les critères.</param>
        /// <returns>Les éléments de la liste qui correspondent aux critères.</returns>
        public ICollection<TReferenceType> GetReferenceListByCriteria<TReferenceType>(TReferenceType criteria) {
            return GetReferenceListByCriteria<TReferenceType, TReferenceType>(criteria);
        }

        /// <summary>
        /// Retourne la liste de référence du type TReferenceType à partir d'un objet de critères.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <typeparam name="TCriteria">Type du critère.</typeparam>
        /// <param name="criteria">Objet contenant les critères.</param>
        /// <returns>Les éléments de la liste qui correspondent aux critères.</returns>
        public ICollection<TReferenceType> GetReferenceListByCriteria<TReferenceType, TCriteria>(TCriteria criteria)
            where TCriteria : TReferenceType {
            ICollection<TReferenceType> beanColl = new List<TReferenceType>();
            IEnumerable<BeanPropertyDescriptor> beanPropertyDescriptorList = BeanDescriptor.GetDefinition(criteria)
                .Properties.Where(property => /* property.PropertyType != typeof(ChangeAction) &&*/ property.GetValue(criteria) != null);

            foreach (TReferenceType bean in GetReferenceList<TReferenceType>()) {
                bool add = true;
                foreach (BeanPropertyDescriptor property in beanPropertyDescriptorList) {
                    if (property.PrimitiveType == null) {
                        continue;
                    }

                    if (!property.GetValue(criteria).Equals(property.GetValue(bean))) {
                        add = false;
                        break;
                    }
                }

                if (add) {
                    beanColl.Add(bean);
                }
            }

            return beanColl;
        }

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <param name="primaryKeyArray">Liste des clés primaires.</param>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <returns>Liste de référence.</returns>
        public ICollection<TReferenceType> GetReferenceListByPrimaryKeyList<TReferenceType>(params object[] primaryKeyArray) {
            if (primaryKeyArray == null) {
                throw new ArgumentNullException("primaryKeyArray");
            }

            return (ICollection<TReferenceType>)GetReferenceListByPrimaryKeyList(typeof(TReferenceType), primaryKeyArray);
        }

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="primaryKeyArray">Liste des clés primaires.</param>
        /// <returns>Liste de référence.</returns>
        public object GetReferenceListByPrimaryKeyList(Type referenceType, params object[] primaryKeyArray) {
            if (referenceType == null) {
                throw new ArgumentNullException("referenceType");
            }

            if (primaryKeyArray == null) {
                throw new ArgumentNullException("primaryKeyArray");
            }

            ArrayList primaryKeyList = new ArrayList(primaryKeyArray);
            BeanDefinition definition = BeanDescriptor.GetDefinition(referenceType);

            IEnumerable initialList = (IEnumerable)GetReferenceList(referenceType);

            Type dictionnaryType = typeof(Dictionary<,>).MakeGenericType(typeof(int), referenceType);
            IDictionary dictionnary = (IDictionary)Activator.CreateInstance(dictionnaryType);
            foreach (object item in initialList) {
                object primaryKey = definition.PrimaryKey.GetValue(item);
                if (primaryKeyList.Contains(primaryKey)) {
                    dictionnary.Add(primaryKeyList.IndexOf(primaryKey), item);
                }
            }

            Type collectionType = typeof(List<>).MakeGenericType(referenceType);
            IList finalList = (IList)Activator.CreateInstance(collectionType);
            for (int index = 0; index < primaryKeyList.Count; ++index) {
                finalList.Add(dictionnary[index]);
            }

            return finalList;
        }

        /// <summary>
        /// Retourne l'élément unique de la liste de référence du type TReference correspondant au prédicat.
        /// </summary>
        /// <typeparam name="TReference">Type de la liste de référence.</typeparam>
        /// <param name="predicate">Prédicat de filtrage.</param>
        /// <param name="referenceName">Nom de la liste de référence.</param>
        /// <returns>Ensemble des éléments.</returns>
        public TReference GetReferenceObject<TReference>(Func<TReference, bool> predicate, string referenceName = null) {
            if (predicate == null) {
                throw new ArgumentNullException("predicate");
            }

            IEnumerable<TReference> list = (IEnumerable<TReference>)GetReferenceList<TReference>(referenceName);
            return list.Single(predicate);
        }

        /// <summary>
        /// Retourne un type de référence à partir de sa clef primaire.
        /// </summary>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <typeparam name="TReferenceType">Type de référence.</typeparam>
        /// <returns>Le type de référence.</returns>
        public TReferenceType GetReferenceObjectByPrimaryKey<TReferenceType>(object primaryKey) {
            if (primaryKey == null) {
                return default(TReferenceType);
            }

            return (TReferenceType)GetReferenceObjectByPrimaryKey(typeof(TReferenceType), primaryKey);
        }

        /// <summary>
        /// Retourne un type de référence à partir de sa clef primaire.
        /// </summary>
        /// <param name="referenceType">Type de référence.</param>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <returns>Le type de référence.</returns>
        public object GetReferenceObjectByPrimaryKey(Type referenceType, object primaryKey) {
            if (referenceType == null) {
                throw new ArgumentNullException("referenceType");
            }

            if (primaryKey == null) {
                return null;
            }

            for (int i = 0; i < 2; ++i) {
                try {
                    IReferenceEntry entry = GetReferenceEntry(referenceType);
                    return entry.GetReferenceValue(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpperInvariant(), primaryKey);
                } catch (KeyNotFoundException e) {
                    if (i == 1) {
                        throw new KeyNotFoundException(e.Message, e);
                    }

                    FlushCache(referenceType);
                }
            }

            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Retourne la valeur d'une liste de référence à partir de son identifiant.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="primaryKey">Clef primaire de l'objet.</param>
        /// <param name="propertySelector">Lambda expression de sélection de la propriété.</param>
        /// <returns>Valeur de la propriété sur le bean.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Nécessite un typage fort.")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Contractualisation forte pour le développeur des types génériques à utiliser.")]
        public string GetReferenceValueByPrimaryKey<TReferenceType>(object primaryKey, Expression<Func<TReferenceType, object>> propertySelector) {
            if (primaryKey == null) {
                throw new ArgumentNullException("primaryKey");
            }

            if (propertySelector == null) {
                throw new ArgumentNullException("propertySelector");
            }

            MemberExpression mb = ResolveMemberExpression(propertySelector.Body);
            return GetReferenceValueByPrimaryKey<TReferenceType>(primaryKey, mb.Member.Name);
        }

        /// <summary>
        /// Retourne la valeur d'une liste de référence à partir
        /// de son identifiant.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="primaryKey">Identifiant de la liste de référence.</param>
        /// <param name="defaultPropertyName">Nom de la propriété par défaut à utiliser. Null pour utiliser la valeur définie au niveau de l'objet.</param>
        /// <returns>Libellé de la liste de référence.</returns>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Pas besoin d'inférence dans ce cas précis.")]
        public string GetReferenceValueByPrimaryKey<TReferenceType>(object primaryKey, string defaultPropertyName = null) {
            return GetReferenceValueByPrimaryKey(typeof(TReferenceType), primaryKey, defaultPropertyName);
        }

        /// <summary>
        /// Retourne la valeur d'une liste de référence à partir
        /// de son identifiant.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="primaryKey">Identifiant de la liste de référence.</param>
        /// <param name="defaultPropertyName">Nom de la propriété par défaut à utiliser. Null pour utiliser la valeur définie au niveau de l'objet.</param>
        /// <returns>Libellé de la liste de référence.</returns>
        public string GetReferenceValueByPrimaryKey(Type referenceType, object primaryKey, string defaultPropertyName = null) {
            object reference = GetReferenceObjectByPrimaryKey(referenceType, primaryKey);
            BeanDefinition definition = BeanDescriptor.GetDefinition(reference);
            BeanPropertyDescriptor property = string.IsNullOrEmpty(defaultPropertyName) ? definition.DefaultProperty : definition.Properties[defaultPropertyName];
            return property.ConvertToString(property.GetValue(reference));
        }

        /// <summary>
        /// Retourne la valeur d'une liste de référence à partir
        /// de son identifiant et d'une datasource.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="primaryKey">Identifiant de la liste de référence.</param>
        /// <param name="dataSource">Datasource.</param>
        /// <param name="defaultPropertyName">Nom de la propriété par défaut à utiliser. Null pour utiliser la valeur définie au niveau de l'objet.</param>
        /// <returns>Libellé de la liste de référence.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Cohérence avec l'API qui publie des méthodes d'instance.")]
        public string GetReferenceValueByPrimaryKey(Type referenceType, object primaryKey, object dataSource, string defaultPropertyName = null) {
            BeanDefinition definition = BeanDescriptor.GetDefinition(referenceType);
            BeanPropertyDescriptor primaryKeyProperty = definition.PrimaryKey;
            BeanPropertyDescriptor valueProperty = string.IsNullOrEmpty(defaultPropertyName) ? definition.DefaultProperty : definition.Properties[defaultPropertyName];
            ICollection dataSourceColl = (ICollection)dataSource;
            if (dataSourceColl == null) {
                throw new NotSupportedException("DataSource must be an ICollection.");
            }

            object candidate = null;
            foreach (object item in dataSourceColl) {
                if (primaryKeyProperty.GetValue(item).Equals(primaryKey)) {
                    candidate = item;
                    break;
                }
            }

            if (candidate == null) {
                throw new NotSupportedException("The datasource does not contain an object with this primary key.");
            }

            return valueProperty.ConvertToString(valueProperty.GetValue(candidate));
        }

        /// <summary>
        /// Remet à jour le cache pour le type spécifié.
        /// </summary>
        /// <param name="referenceType">Type de référence mis en cache.</param>
        void IReferenceManager.FlushCache(Type referenceType) {
            if (referenceType == null) {
                throw new ArgumentNullException("referenceType");
            }

            FlushCache(referenceType);
        }

        /// <summary>
        /// Retourne si le type a été mis en cache.
        /// </summary>
        /// <param name="type">Le type traité.</param>
        /// <returns><code>True</code> si le type a été mis en cache, <code>False</code> sinon.</returns>
        internal static bool HasCache(Type type) {
            object[] attrs = type.GetCustomAttributes(typeof(ReferenceAttribute), false);
            return attrs.Length == 1;
        }

        /// <summary>
        /// Construit l'entrée pour le cache de référence.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <returns>Entrée du cache.</returns>
        private static IReferenceEntry BuildReferenceEntry(Type referenceType) {
            ICollection<ReferenceResource> resourceList = LoadResources(referenceType);
            ICollection referenceList = referenceListLoader(referenceType);
            Type entryType = typeof(ReferenceEntry<>).MakeGenericType(referenceType);
            IReferenceEntry entry = (IReferenceEntry)Activator.CreateInstance(entryType);
            entry.Initialize(referenceList, resourceList);
            return entry;
        }

        /// <summary>
        /// Retourne le cache d'un type de reference.
        /// </summary>
        /// <param name="referenceType">Type de reference traité.</param>
        /// <returns>Le nom de la région du cache associé.</returns>
        private static string GetCacheRegionByType(Type referenceType) {
            object[] attrs = referenceType.GetCustomAttributes(typeof(ReferenceAttribute), false);
            if (attrs.Length == 0) {
                throw new NotSupportedException("Le type " + referenceType + " n'est pas une liste de référence.");
            }

            return ((ReferenceAttribute)attrs[0]).IsStatic ? StaticCache : ReferenceCache;
        }

        /// <summary>
        /// Charge une liste de référence en utilisant le ReferenceAccessor qui charge depuis la base de données.
        /// </summary>
        /// <param name="referenceType">Type de référence à charger.</param>
        /// <returns>Liste des éléments.</returns>
        private static ICollection LoadReferenceListWithReferenceAccessor(Type referenceType) {
            return ServiceManager.Instance.InvokeReferenceAccessor(referenceType, null);
        }

        /// <summary>
        /// Retourne les ressources disponibles pour un type.
        /// </summary>
        /// <param name="referenceType">Type de référence.</param>
        /// <returns>Liste des ressources.</returns>
        private static ICollection<ReferenceResource> LoadResources(Type referenceType) {
            if (ServiceManager.Instance.ContainsLocalService(typeof(IResourceLoader))) {
                using (ServiceChannel<IResourceLoader> channel = new ServiceChannel<IResourceLoader>()) {
                    return channel.Service.LoadReferenceResourceListByReferenceType(referenceType);
                }
            }

            return null;
        }

        /// <summary>
        /// Résout une expression pour en fournir le membre invoqué.
        /// </summary>
        /// <param name="expression">Lambda Expression.</param>
        /// <returns>Le membre invoqué.</returns>
        private static MemberExpression ResolveMemberExpression(Expression expression) {
            MemberExpression mb = expression as MemberExpression;
            if (mb != null && mb.Member != null) {
                return mb;
            }

            UnaryExpression une = expression as UnaryExpression;
            if (une == null) {
                throw new NotSupportedException();
            }

            if (une.Operand == null) {
                throw new NotSupportedException();
            }

            MemberExpression member = une.Operand as MemberExpression;
            if (member == null || member.Member == null) {
                return member;
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Retourne l'entrée du cache pour le type de référence.
        /// </summary>
        /// <param name="referenceType">Type de référence.</param>
        /// <returns>Entrée du cache.</returns>
        private IReferenceEntry GetReferenceEntry(Type referenceType) {
            string region = GetCacheRegionByType(referenceType);
            IReferenceEntry entry = null;
            try {
                Element element = CacheManager.Instance.GetCache(region).Get(referenceType.FullName);
                if (element != null) {
                    entry = element.Value as IReferenceEntry;
                }
            } catch (Exception e) {
                entry = BuildReferenceEntry(referenceType);
                ILog log = LogManager.GetLogger("Kinetix.Application");
                log.Warn("Impossible d'établir une connexion avec le cache, la valeur est cherchée en base", e);
            }

            if (entry == null) {
                entry = BuildReferenceEntry(referenceType);
                CacheManager.Instance.GetCache(region).Put(new Element(referenceType.FullName, entry));
            }

            return (IReferenceEntry)entry;
        }

        /// <summary>
        /// Remet à jour le cache pour le type spécifié.
        /// </summary>
        /// <param name="referenceType">Type de référence mis en cache.</param>
        private void FlushCache(Type referenceType) {
            string region = GetCacheRegionByType(referenceType);
            CacheManager.Instance.GetCache(region).Remove(referenceType.FullName);
        }
    }
}
