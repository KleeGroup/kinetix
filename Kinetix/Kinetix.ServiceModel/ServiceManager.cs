using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using System.Web;
using System.Web.Configuration;
using Kinetix.ComponentModel;
using Kinetix.Monitoring.Html;
using Kinetix.Monitoring.Manager;
using Kinetix.ServiceModel.Unity;
using log4net;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Kinetix.ServiceModel {
    /// <summary>
    /// Gestionnaire de d'accès à la couche service.
    /// </summary>
    public sealed class ServiceManager : IManager, IManagerDescription {

        /// <summary>
        /// Compteur d'appels aus services.
        /// </summary>
        public const string CounterTotalServiceCallCount = "TOTAL_SERVICE_CALL_COUNT";

        /// <summary>
        /// Compteur d'évènement d'erreur.
        /// </summary>
        public const string CounterTotalServiceErrorCount = "TOTAL_SERVICE_ERROR_COUNT";

        /// <summary>
        /// Hypercube database name.
        /// </summary>
        public const string ServiceHyperCube = "SERVICEDB";

        private static readonly ServiceManager _instance = new ServiceManager();
        private readonly Dictionary<string, Accessor> _referenceAccessors = new Dictionary<string, Accessor>();
        private readonly Dictionary<string, Accessor> _primaryKeyAccessors = new Dictionary<string, Accessor>();
        private readonly Dictionary<string, ICollection<Accessor>> _autoCompleteAccessors = new Dictionary<string, ICollection<Accessor>>();
        private readonly Dictionary<string, AutoCompleteAccessorAttribute> _autoCompleteAttributes = new Dictionary<string, AutoCompleteAccessorAttribute>();
        private readonly Dictionary<string, Accessor> _fileAccessors = new Dictionary<string, Accessor>();
        private readonly List<Type> _localServices = new List<Type>();
        private readonly List<ServiceHost> _hostList = new List<ServiceHost>();
        private DbContext _context;
        private IUnityContainer _container;
        private ServiceModelSectionGroup _serviceModelSection;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        private ServiceManager() {
            _serviceModelSection = null;
            System.Configuration.Configuration configuration;
            try {
                if (HttpContext.Current != null) {
                    configuration = WebConfigurationManager.OpenWebConfiguration(
                            HttpContext.Current.Request.ApplicationPath);
                } else {
                    configuration = ConfigurationManager.OpenExeConfiguration(
                            ConfigurationUserLevel.None);
                }

                _serviceModelSection = ServiceModelSectionGroup.GetSectionGroup(configuration);
            } catch (Exception ex) {
                ILog log = LogManager.GetLogger("Chaine.Application");
                log.Error("Erreur à la lecture de la section de configuration : ", ex);
            }
        }

        /// <summary>
        /// Evenement permettant de modifier le comportement du pipeline d'interception durant l'enregistrement des services.
        /// </summary>
        public event EventHandler<InterceptionPipelineEventArgs> RegisteringInterceptors;

        /// <summary>
        /// Retourne une instance du manager.
        /// </summary>
        public static ServiceManager Instance {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Retourne le conteneur d'injection de dépendances.
        /// </summary>
        public IUnityContainer Container {
            get {
                return _container;
            }

            set {
                _container = value;
                _container.AddNewExtension<Interception>();
            }
        }

        /// <summary>
        /// Désactive la publication de tous les services WCF locaux.
        /// </summary>
        /// <remarks>Utilisable pour lancer des traitements batchs en ligne de commande
        /// sans publier les service.</remarks>
        public bool DisableServiceHosts {
            get;
            set;
        }

        /// <summary>
        /// Retourne la liste des attributs des autocomplete à partir du nom de l'autocomplete s'il est définit, à partir du type de l'objet retourné s'il n'est pas définit.
        /// </summary>
        public Dictionary<string, AutoCompleteAccessorAttribute> AutoCompleteAttributes {
            get {
                return _autoCompleteAttributes;
            }
        }

        /// <summary>
        /// Retourne un objet décrivant le service.
        /// </summary>
        IManagerDescription IManager.Description {
            get {
                return this;
            }
        }

        /// <summary>
        /// Retourne le nom.
        /// </summary>
        string IManagerDescription.Name {
            get { return "Services"; }
        }

        /// <summary>
        /// Image du manager.
        /// </summary>
        string IManagerDescription.Image {
            get { return "query.png"; }
        }

        /// <summary>
        /// Type mime de l'image.
        /// </summary>
        string IManagerDescription.ImageMimeType {
            get { return "image/png"; }
        }

        /// <summary>
        /// Image.
        /// </summary>
        byte[] IManagerDescription.ImageData {
            get { return IR.Query_png; }
        }

        /// <summary>
        /// Priorité d'affichage du manager.
        /// </summary>
        int IManagerDescription.Priority {
            get { return 20; }
        }

        /// <summary>
        /// Methode de chargement de fichier à partir du FileAccessor.
        /// </summary>
        /// <param name="fileAccessorName">Nom du FileAccessor.</param>
        /// <param name="pk">Clef primaire de l'objet pointant vers le fichier.</param>
        /// <returns>Le contenu du fichier.</returns>
        public DownloadedFile GetFile(string fileAccessorName, object pk) {
            if (fileAccessorName == null) {
                throw new ArgumentNullException("fileAccessorName");
            }

            if (pk == null) {
                throw new ArgumentNullException("pk");
            }

            Accessor accessor = _fileAccessors[fileAccessorName];

            object obj;
            ChannelFactory factory;
            object service = GetService(accessor.ContractType, out factory);
            try {
                obj = accessor.Method.Invoke(service, new object[] { pk });
            } finally {
                if (factory != null) {
                    factory.Close();
                }
            }

            DownloadedFile file = obj as DownloadedFile;
            if (file == null) {
                throw new FileNotFoundException();
            }

            return file;
        }

        /// <summary>
        /// Retourne la valeur par défaut d'une objet à partir de sa clef primaire.
        /// </summary>
        /// <param name="type">Type de l'objet.</param>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <param name="defaultPropertyName">Nom de la propriété évaluée.</param>
        /// <returns>Valeur de la propriété par défaut de l'objet.</returns>
        public string GetDefaultValueByPrimaryKey(Type type, object primaryKey, string defaultPropertyName = null) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            if (primaryKey == null) {
                throw new ArgumentNullException("primaryKey");
            }

            return ReferenceManager.HasCache(type) ? ReferenceManager.Instance.GetReferenceValueByPrimaryKey(type, primaryKey, defaultPropertyName) : InvokePrimaryKeyAccessor(type, primaryKey, defaultPropertyName);
        }

        /// <summary>
        /// Retourne la liste des valeurs d'un objet correspondant à un préfix.
        /// </summary>
        /// <param name="type">Libellé complet du type de l'objet.</param>
        /// <param name="like">Préfix.</param>
        /// <returns>Liste des valeurs par défaut.</returns>
        public ICollection GetAutoCompleteList(string type, string like) {
            ICollection<Accessor> accessorList = _autoCompleteAccessors[type];
            if (accessorList.Count == 0) {
                throw new NotSupportedException();
            }

            IEnumerator<Accessor> enumerator = accessorList.GetEnumerator();
            if (!enumerator.MoveNext()) {
                throw new NotSupportedException();
            }

            Accessor accessor = enumerator.Current;
            object list;
            ChannelFactory factory;
            object service = GetService(accessor.ContractType, out factory);
            try {
                list = accessor.Method.Invoke(service, new object[] { like });
            } catch (ArgumentException e) {
                throw new ArgumentException("Méthode " + accessor.ContractType + "." + accessor.Method.Name, e);
            } finally {
                if (factory != null) {
                    factory.Close();
                }
            }

            ICollection coll = list as ICollection;
            if (coll == null) {
                throw new NotImplementedException(list.GetType().Name);
            }

            return coll;
        }

        /// <summary>
        /// Indique si l'autocompletion peut être utilisée pour un type.
        /// </summary>
        /// <param name="type">Type d'objet.</param>
        /// <param name="autoCompleteName">Nom de la liste spécifique à utiliser. Peut être nul.</param>
        /// <returns>True si l'autocompletion peut être utilisé.</returns>
        public bool CanUseAutoComplete(Type type, string autoCompleteName = null) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            BeanDefinition definition;
            if (_primaryKeyAccessors.ContainsKey(type.FullName)) {
                Accessor primaryKeyAccessor = _primaryKeyAccessors[type.FullName];
                definition = BeanDescriptor.GetDefinition(primaryKeyAccessor.ReturnType, true);
            } else if (_referenceAccessors.ContainsKey(type.FullName)) {
                Accessor referenceAccessor = _referenceAccessors[type.FullName];
                definition = BeanDescriptor.GetDefinition(referenceAccessor.ReturnType, true);
            } else {
                return false;
            }

            return definition.DefaultProperty != null && _autoCompleteAccessors.ContainsKey(string.IsNullOrEmpty(autoCompleteName) ? type.FullName : autoCompleteName);
        }

        /// <summary>
        /// Indique si le manager contient une référence locale au contrat contractType.
        /// </summary>
        /// <param name="contractType">Interface du contrat.</param>
        /// <returns>True si le contrat est connu.</returns>
        public bool ContainsLocalService(Type contractType) {
            if (contractType == null) {
                throw new ArgumentNullException("contractType");
            }

            return _localServices.Contains(contractType);
        }

        /// <summary>
        /// Enregistre tous les services d'une assembly.
        /// </summary>
        /// <param name="assembly">Assembly.</param>
        public void RegisterAllService(Assembly assembly) {
            if (assembly == null) {
                throw new ArgumentNullException("assembly");
            }

            foreach (Module module in assembly.GetModules()) {
                foreach (Type type in module.GetTypes()) {
                    if (type.GetCustomAttributes(typeof(ServiceBehaviorAttribute), false).Length > 0) {
                        foreach (Type interfaceType in type.GetInterfaces()) {
                            if (interfaceType.GetCustomAttributes(typeof(ServiceContractAttribute), false).Length > 0) {
                                if (!this.ContainsLocalService(interfaceType)) {
                                    this.RegisterLocalService(interfaceType, type);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enregistre tous les contrats d'une assembly comme service distant.
        /// </summary>
        /// <param name="assembly">Assembly.</param>
        public void RegisterAllRemoteService(Assembly assembly) {
            if (assembly == null) {
                throw new ArgumentNullException("assembly");
            }

            foreach (Module module in assembly.GetModules()) {
                foreach (Type interfaceType in module.GetTypes()) {
                    if (interfaceType.IsInterface && interfaceType.GetCustomAttributes(typeof(ServiceContractAttribute), false).Length > 0) {
                        this.RegisterRemoteService(interfaceType);
                    }
                }
            }
        }

        /// <summary>
        /// Enregistre une instance de service.
        /// </summary>
        /// <param name="contractType">Type de l'interface représentant le contrat.</param>
        /// <param name="serviceType">Type du service.</param>
        public void RegisterLocalService(Type contractType, Type serviceType) {
            if (contractType == null) {
                throw new ArgumentNullException("contractType");
            }

            if (serviceType == null) {
                throw new ArgumentNullException("serviceType");
            }

            if (!contractType.IsInterface) {
                throw new ArgumentException("contractType " + contractType.FullName + "must define an interface.");
            }

            if (!contractType.IsAssignableFrom(serviceType)) {
                throw new ArgumentException("Invalid serviceType " + serviceType.FullName);
            }

            if (_localServices.Contains(contractType)) {
                throw new NotSupportedException("Contract already registered for contractType " + contractType.FullName);
            }

            ILog log = LogManager.GetLogger("Kinetix.Application");
            if (log.IsDebugEnabled) {
                log.Debug("Enregistrement du service " + contractType.FullName);
            }

            List<Accessor> referenceAccessors = new List<Accessor>();
            List<Accessor> primaryKeyAccessors = new List<Accessor>();
            List<Accessor> autoCompleteAccessors = new List<Accessor>();
            List<Accessor> fileAccessors = new List<Accessor>();
            List<string> disableLogList = new List<string>();
            ParseAccessors(contractType, referenceAccessors, primaryKeyAccessors, autoCompleteAccessors, fileAccessors, disableLogList);

            if (!HasWcfClientEndPoint(contractType) || HttpContext.Current == null) {
                ServiceEndpointElement sep = HasWcfServerEndPoint(serviceType, contractType);
                if (sep != null && HttpContext.Current == null && !DisableServiceHosts) {
                    if (log.IsInfoEnabled) {
                        log.Info("Inscription du service " + sep.Contract + " à l'adresse " + sep.Address + ".");
                    }

                    _container.RegisterType(
                        serviceType,
                        new Interceptor<InterfaceInterceptor>(),
                        new InterceptionBehavior<LogInterceptionBehavior>());
                    UnityServiceHost host = new UnityServiceHost(serviceType);
                    host.Container = _container;
                    host.Open();
                    _hostList.Add(host);
                } else {
                    InterceptionPipelineEventArgs args = new InterceptionPipelineEventArgs(contractType, new List<InjectionMember> {
                                                                new Interceptor<InterfaceInterceptor>(),
                        //// new InterceptionBehavior<HighAvailabilityInterceptionBehavior>(),
                                                                new InterceptionBehavior<LogInterceptionBehavior>(),
                                                                new InterceptionBehavior<TransactionInterceptionBehavior>(),
                                                                new InterceptionBehavior<AnalyticsInterceptionBehavior>() });
                    if (this.RegisteringInterceptors != null) {
                        this.RegisteringInterceptors(this, args);
                    }

                    _container.RegisterType(
                        contractType,
                        serviceType,
                        args.Interceptors.ToArray());
                    _localServices.Add(contractType);
                }
            }

            this.RegisterReferenceAccessors(referenceAccessors);
            this.RegisterPrimaryKeyAccessors(primaryKeyAccessors);
            this.RegisterAutoCompleteAccessors(autoCompleteAccessors);
            this.RegisterFileAccessors(fileAccessors);
        }

        /// <summary>
        /// Enregistre un service distant.
        /// </summary>
        /// <param name="contractType">Type de l'interface représentant le contrat.</param>
        public void RegisterRemoteService(Type contractType) {
            if (contractType == null) {
                throw new ArgumentNullException("contractType");
            }

            if (!contractType.IsInterface) {
                throw new ArgumentException("contractType " + contractType.FullName + "must define an interface.");
            }

            ILog log = LogManager.GetLogger("Kinetix.Application");
            if (log.IsDebugEnabled) {
                log.Debug("Enregistrement du service distant " + contractType.FullName);
            }

            /* REMARQUE : on ne peut pas appeler HasWcfClientEndPoint à l'enregistrement car web.config n'est pas accessible. */
            InterceptionPipelineEventArgs args = new InterceptionPipelineEventArgs(contractType, new List<InjectionMember> {
                                                                new Interceptor<InterfaceInterceptor>(),
                                                                new InterceptionBehavior<LogInterceptionBehavior>(),
                                                                new InterceptionBehavior<AnalyticsInterceptionBehavior>() });
            if (this.RegisteringInterceptors != null) {
                this.RegisteringInterceptors(this, args);
            }

            var remoteServiceFactory = new InjectionFactory(container => {
                /* TODO VERSION TEMPORAIRE : il faut faire un dispose sur la factory quand le service n'est plus utilisé. */
                try {

                    ChannelFactory factory;
                    var instance = GetRemoteService(contractType, out factory);
                    return instance;
                } catch (Exception ex) {
                    log.Error($"Erreur à l'instancation du service WCF distant {contractType}", ex);
                    throw ex;
                }
            });
            _container.RegisterType(contractType, remoteServiceFactory);
        }

        /// <summary>
        /// Libération des ressources consommées par le manager lors du undeploy.
        /// Exemples : connexions, thread, flux.
        /// </summary>
        public void Close() {
            foreach (ServiceHost host in _hostList) {
                host.Close();
            }
        }

        /// <summary>
        /// Enregistre un accésseur avec un nom particulier.
        /// </summary>
        /// <param name="name">Nom de l'accesseur.</param>
        /// <param name="accessor">Accesseur.</param>
        public void RegisterAutoCompleteAccessor(string name, Accessor accessor) {
            ICollection<Accessor> list = _autoCompleteAccessors.ContainsKey(name) ? _autoCompleteAccessors[name] : new Collection<Accessor>();
            list.Add(accessor);
            _autoCompleteAccessors[name] = list;
        }

        /// <summary>
        /// Enregistre un accésseur avec un nom particulier.
        /// </summary>
        /// <param name="name">Nom de l'accesseur.</param>
        /// <param name="accessor">Accesseur.</param>
        public void RegisterReferenceAccessor(string name, Accessor accessor) {
            if (_referenceAccessors.ContainsKey(name)) {
                throw new NotSupportedException();
            }

            _referenceAccessors.Add(name, accessor);
        }

        /// <summary>
        /// Enregistrement d'un contexte Entity-Framework.
        /// </summary>
        /// <param name="context">Contexte Entity Framework.</param>
        public void RegisterDbContext(DbContext context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            _context = context;
        }

        /// <summary>
        /// Retourne un bean a partir de sa clef primaire.
        /// </summary>
        /// <param name="type">Le type recherché.</param>
        /// <param name="primaryKey">La clef primaire du bean.</param>
        /// <returns>Le bean.</returns>
        public object GetBeanByPrimaryKey(Type type, object primaryKey) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            if (!_primaryKeyAccessors.ContainsKey(type.FullName)) {
                throw new ArgumentException("Pas de PrimaryKeyAccessor disponible pour le type " + type.FullName, "type");
            }

            Accessor accessor = _primaryKeyAccessors[type.FullName];
            ChannelFactory factory;
            object o;
            object service = GetService(accessor.ContractType, out factory);
            try {
                o = accessor.Method.Invoke(service, new object[] { primaryKey });
            } finally {
                if (factory != null) {
                    factory.Close();
                }
            }

            return o;
        }

        /// <summary>
        /// Extension de la méthode toString().
        /// Permet à chaque Manager de présenter son propre état.
        /// </summary>
        /// <param name="writer">Writer HTML.</param>
        void IManagerDescription.ToHtml(System.Web.UI.HtmlTextWriter writer) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            HtmlPageRenderer.ToHtml(ServiceHyperCube, writer);
        }

        /// <summary>
        /// Retourne une instance du contrat.
        /// </summary>
        /// <param name="contractType">Interface du contrat.</param>
        /// <param name="factory">Factory du canal de communication, si appel distant.</param>
        /// <returns>Instance du contrat ou null.</returns>
        internal object GetService(Type contractType, out ChannelFactory factory) {
            if (_localServices.Contains(contractType)) {
                factory = null;
                return _container.Resolve(contractType);
            }

            if (HasWcfClientEndPoint(contractType)) {
                return GetRemoteService(contractType, out factory);
            }

            throw new NotSupportedException("Check if the attribute [ServiceContract] is present on contract " + contractType.Name + " and if [ServiceBehavior(...)] is present on its implementation.");
        }

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="referenceName">Nom de la liste à utiliser.</param>
        /// <returns>Liste de référence.</returns>
        internal ICollection InvokeReferenceAccessor(Type referenceType, string referenceName) {
            if (_context != null && string.IsNullOrEmpty(referenceName)) {
                DbSet set = _context.Set(referenceType);
                set.Load();
                return set.Local;
            }

            if (!_referenceAccessors.ContainsKey(referenceType.FullName)) {
                throw new ArgumentException("Pas d'accesseur disponible pour le type " + referenceType.Name, "referenceType");
            }

            Accessor accessor = _referenceAccessors[referenceName ?? referenceType.FullName];
            ChannelFactory factory;
            object list;
            object service = GetService(accessor.ContractType, out factory);
            try {
                list = accessor.Method.Invoke(service, null);
            } finally {
                if (factory != null) {
                    factory.Close();
                }
            }

            ICollection coll = list as ICollection;
            if (coll == null) {
                throw new NotImplementedException(list.GetType().Name);
            }

            return coll;
        }

        /// <summary>
        /// Vérifie que la méthode retourne une collection d'un type non générique.
        /// </summary>
        /// <param name="method">Méthode.</param>
        /// <param name="returnType">Type retourné par la méthode.</param>
        /// <param name="parameterCount">Nombre de paramètres nécessaire pour la méthode.</param>
        private static void CheckGenericType(MethodInfo method, Type returnType, int parameterCount) {
            if (!returnType.IsGenericType || (
                    !typeof(ICollection<>).Equals(returnType.GetGenericTypeDefinition()) &&
                    returnType.GetGenericTypeDefinition().GetInterface(typeof(ICollection<>).Name) == null)) {
                throw new NotSupportedException(SR.ExceptionAccessorMustReturnCollection + returnType.Name);
            }

            if (method.GetParameters().Length != parameterCount) {
                throw new NotSupportedException(SR.ExceptionAccessorWithParameters);
            }

            if (returnType.GetGenericArguments().Length > 1) {
                throw new NotSupportedException(SR.ExceptionAccessorWithTooManyGenericArgs);
            }
        }

        /// <summary>
        /// Instancie un service à partir de son type.
        /// </summary>
        /// <param name="serviceType">Type du service.</param>
        /// <returns>Instance du service.</returns>
        private static object ActivateService(Type serviceType) {
            Type[] constructorParams = new Type[] { typeof(string) };
            object[] dasAttributes = serviceType.GetCustomAttributes(typeof(DataAccessServiceAttribute), false);
            try {
                if (dasAttributes.Length > 0 && serviceType.GetConstructor(constructorParams) != null) {
                    return Activator.CreateInstance(serviceType, ((DataAccessServiceAttribute)dasAttributes[0]).DataSourceName);
                }

                return Activator.CreateInstance(serviceType);
            } catch (MissingMethodException mme) {
                throw new NotImplementedException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionMissingConstructor,
                        serviceType.FullName),
                    mme);
            } catch (TargetInvocationException te) {
                throw new TargetInvocationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        SR.ExceptionServiceInstanciation,
                        serviceType.FullName,
                        te.InnerException.Message),
                    te);
            }
        }

        /// <summary>
        /// Retourne une instance du contrat distant.
        /// </summary>
        /// <param name="contractType">Interface du contrat.</param>
        /// <param name="factory">Factory du canal de communication.</param>
        /// <returns>Instance du contrat ou null.</returns>
        private static object GetRemoteService(Type contractType, out ChannelFactory factory) {
            Type factoryType = typeof(ChannelFactory<>).MakeGenericType(contractType);
            factory = (ChannelFactory)Activator.CreateInstance(factoryType, new object[] { string.Empty });
            factory.Open();
            MethodInfo method = factoryType.GetMethod("CreateChannel", new Type[0]);
            return method.Invoke(factory, new object[0]);
        }

        /// <summary>
        /// Parse les différents accésseurs fournis par le type.
        /// </summary>
        /// <param name="contractType">Contrat.</param>
        /// <param name="referenceAccessors">Accésseurs à des listes de références.</param>
        /// <param name="primaryKeyAccessors">Accésseurs via clef primaire.</param>
        /// <param name="autoCompleteAccessors">Accésseurs via like sur la propriété par défaut.</param>
        /// <param name="fileAccessors">Accésseurs sur les fichiers.</param>
        /// <param name="disableLogList">Liste des methodes sur lewquelles le log est desactivé.</param>
        private void ParseAccessors(Type contractType, ICollection<Accessor> referenceAccessors, ICollection<Accessor> primaryKeyAccessors, ICollection<Accessor> autoCompleteAccessors, ICollection<Accessor> fileAccessors, List<string> disableLogList) {
            MethodInfo[] contractMethods = contractType.GetMethods();
            for (int i = 0; i < contractMethods.Length; i++) {
                MethodInfo method = contractMethods[i];
                Type returnType = method.ReturnType;

                object[] referenceArray = method.GetCustomAttributes(typeof(ReferenceAccessorAttribute), true);
                if (referenceArray.Length > 0) {
                    ReferenceAccessorAttribute attribute = (ReferenceAccessorAttribute)referenceArray[0];
                    CheckGenericType(method, returnType, 0);
                    referenceAccessors.Add(new Accessor(contractType, method, returnType.GetGenericArguments()[0], attribute.Name));
                }

                object[] primaryKeyArray = method.GetCustomAttributes(typeof(PrimaryKeyAccessorAttribute), true);
                if (primaryKeyArray.Length > 0) {
                    PrimaryKeyAccessorAttribute attribute = (PrimaryKeyAccessorAttribute)primaryKeyArray[0];
                    primaryKeyAccessors.Add(attribute.TargetType == null
                                                ? new Accessor(contractType, method, returnType)
                                                : new Accessor(contractType, method, attribute.TargetType, returnType));
                }

                object[] autoCompleteArray = method.GetCustomAttributes(typeof(AutoCompleteAccessorAttribute), true);
                if (autoCompleteArray.Length > 0) {
                    AutoCompleteAccessorAttribute attribute = (AutoCompleteAccessorAttribute)autoCompleteArray[0];
                    CheckGenericType(method, returnType, 1);
                    if (!typeof(string).Equals(method.GetParameters()[0].ParameterType)) {
                        throw new NotSupportedException("L'autocomplete accessor " + contractType.FullName + "." +
                            method.Name + " doit avoir un seul paramètre de type String.");
                    }

                    if (attribute.TargetType == null) {
                        autoCompleteAccessors.Add(new Accessor(contractType, method, returnType.GetGenericArguments()[0], attribute.Name));
                    } else {
                        autoCompleteAccessors.Add(new Accessor(contractType, method, attribute.TargetType, returnType.GetGenericArguments()[0], attribute.Name));
                    }

                    _autoCompleteAttributes.Add(attribute.Name ?? returnType.GetGenericArguments()[0].FullName, attribute);
                }

                object[] fileArray = method.GetCustomAttributes(typeof(FileAccessorAttribute), true);
                if (fileArray.Length == 0) {
                    continue;
                }

                if (!typeof(DownloadedFile).Equals(returnType)) {
                    throw new NotSupportedException("Le FileAccessor " + contractType.FullName + "." + method.Name
                                                    + " doit retourner un objet de type Kinetix.ComponentModel.DownloadedFile.");
                }

                fileAccessors.Add(new Accessor(contractType, method, returnType, method.Name));
            }
        }

        /// <summary>
        /// Appelle le PrimaryKeyAccessor pour le type donné avec la valeur de PK donnée.
        /// </summary>
        /// <param name="type">Type concerné.</param>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <param name="propertyName">Propriété évaluée.</param>
        /// <returns>Libellé par défaut.</returns>
        private string InvokePrimaryKeyAccessor(Type type, object primaryKey, string propertyName = null) {
            object o = GetBeanByPrimaryKey(type, primaryKey);
            Accessor accessor = _primaryKeyAccessors[type.FullName];
            BeanDefinition definition = BeanDescriptor.GetDefinition(accessor.ReturnType);
            BeanPropertyDescriptor property = string.IsNullOrEmpty(propertyName) ? definition.DefaultProperty : definition.Properties[propertyName];
            return property.ConvertToString(property.GetValue(o));
        }

        /// <summary>
        /// Enregistre une liste d'accesseur à des listes de référence.
        /// </summary>
        /// <param name="accessors">Liste d'accesseurs.</param>
        private void RegisterReferenceAccessors(IEnumerable<Accessor> accessors) {
            lock (this) {
                foreach (Accessor accessor in accessors) {
                    string name = accessor.Name ?? accessor.ReferenceType.FullName;

                    if (_referenceAccessors.ContainsKey(name)) {
                        Accessor acc = _referenceAccessors[name];
                        throw new NotSupportedException("Impossible d'enregistrer plusieurs fois le ReferenceAccessor: " + name +
                            "\n\tPremier enregistrement dans     " + acc.Method.DeclaringType + "." + acc.Method.Name +
                            "\n\tTentative d'enregistrement dans " + accessor.Method.DeclaringType + "." + accessor.Method.Name);
                    }

                    _referenceAccessors.Add(name, accessor);
                    object[] items = accessor.ReferenceType.GetCustomAttributes(typeof(ReferenceAttribute), false);
                    if (items.Length == 0) {
                        throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, SR.WarningReferenceListNotMarkedAsReference, name, accessor.ContractType, accessor.Method.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Enregistre une liste d'accésseur à des objets par la clef primaire.
        /// </summary>
        /// <param name="accessors">Liste d'accesseurs.</param>
        private void RegisterPrimaryKeyAccessors(IEnumerable<Accessor> accessors) {
            lock (this) {
                foreach (Accessor accessor in accessors) {
                    if (!_primaryKeyAccessors.ContainsKey(accessor.ReferenceType.FullName)) {
                        _primaryKeyAccessors.Add(accessor.ReferenceType.FullName, accessor);
                    } else {
                        Accessor pkAccessor = _primaryKeyAccessors[accessor.ReferenceType.FullName];
                        throw new NotSupportedException("Un accesseur de clef primaire existe déjà pour le type " +
                                                        accessor.ReferenceType.FullName + " pour la méthode " +
                                                        pkAccessor.ContractType.FullName + "." + pkAccessor.Method.Name +
                                                        " : impossible d'utiliser " + accessor.ContractType.FullName +
                                                        "." + accessor.Method.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Enregistre une liste d'accésseur à des fichiers.
        /// </summary>
        /// <param name="accessors">Liste d'accesseurs.</param>
        private void RegisterFileAccessors(IEnumerable<Accessor> accessors) {
            lock (this) {
                foreach (Accessor accessor in accessors) {
                    _fileAccessors.Add(accessor.Name, accessor);
                }
            }
        }

        /// <summary>
        /// Enregistre une liste d'accésseur à des listes d'objet commençant par une valeur.
        /// </summary>
        /// <param name="accessors">Liste d'accesseurs.</param>
        private void RegisterAutoCompleteAccessors(IEnumerable<Accessor> accessors) {
            lock (this) {
                foreach (Accessor accessor in accessors) {
                    string name = accessor.Name ?? accessor.ReferenceType.FullName;
                    ICollection<Accessor> list = _autoCompleteAccessors.ContainsKey(name) ? _autoCompleteAccessors[name] : new Collection<Accessor>();
                    list.Add(accessor);
                    _autoCompleteAccessors[name] = list;
                }
            }
        }

        /// <summary>
        /// Recherche un point d'accès client contractType dans la
        /// configuration WCF.
        /// </summary>
        /// <param name="contractType">Type du contrat recherché.</param>
        /// <returns>True si il existe un point d'accès.</returns>
        private bool HasWcfClientEndPoint(Type contractType) {
            System.Configuration.Configuration configuration;
            if (HttpContext.Current != null) {
                configuration = WebConfigurationManager.OpenWebConfiguration(
                        HttpContext.Current.Request.ApplicationPath);
                _serviceModelSection = ServiceModelSectionGroup.GetSectionGroup(configuration);
            } else {
                // configuration = ConfigurationManager.OpenExeConfiguration(
                //        ConfigurationUserLevel.None);
            }

            return _serviceModelSection != null && _serviceModelSection.Client.Endpoints.Cast<ChannelEndpointElement>().Any(cep => cep.Contract.Equals(contractType.FullName));
        }

        /// <summary>
        /// Recherche un point d'accès serveur dans la configuration WCF.
        /// </summary>
        /// <param name="serviceType">Type du service.</param>
        /// <param name="contractType">Type du contrat.</param>
        /// <returns>Retourne le point d'accès.</returns>
        private ServiceEndpointElement HasWcfServerEndPoint(Type serviceType, Type contractType) {
            if (_serviceModelSection == null) {
                return null;
            }

            ServiceElementCollection coll = _serviceModelSection.Services.Services;
            if (coll.ContainsKey(serviceType.FullName)) {
                ServiceElement element = coll[serviceType.FullName];
                return element.Endpoints.Cast<ServiceEndpointElement>().FirstOrDefault(sep => sep.Contract.Equals(contractType.FullName));
            }

            return null;
        }

        /// <summary>
        /// Gère le cas d'erreur sur un ServiceHost.
        /// </summary>
        /// <param name="sender">Emtteur de l'évènement.</param>
        /// <param name="e">Argument de l'évènement.</param>
        private void Host_Faulted(object sender, EventArgs e) {
            ServiceHost host = (ServiceHost)sender;
            ILog log = LogManager.GetLogger("Kinetix.Application");
            if (log.IsErrorEnabled) {
                log.Error("ServiceHost is in faulted state for service " + host.Description);
            }
        }
    }
}
