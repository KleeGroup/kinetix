using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using log4net;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Classe pour la gestion des domaines.
    /// </summary>
    public class DomainManager {

        /// <summary>
        /// Domaine de champs alias.
        /// </summary>
        public const string AliasDomain = "DO_ALIAS";
        private const string DomainPrefix = "__";

        /// <summary>
        /// Singleton.
        /// </summary>
        private static readonly DomainManager _instance = new DomainManager();

        /// <summary>
        /// Dictionnaire des domaines.
        /// </summary>
        private readonly Dictionary<string, IDomainChecker> _domainDictionary;

        /// <summary>
        /// Crée une nouvelle instance.
        /// Initialise les domaines par défaut pour les types primitifs.
        /// </summary>
        private DomainManager() {
            _domainDictionary = new Dictionary<string, IDomainChecker>();
            RegisterDomain(new Domain<bool?>(DomainPrefix + typeof(bool).Name, null, null));
            RegisterDomain(new Domain<byte?>(DomainPrefix + typeof(byte).Name, null, null));
            RegisterDomain(new Domain<DateTime?>(DomainPrefix + typeof(DateTime).Name, null, null));
            RegisterDomain(new Domain<decimal?>(DomainPrefix + typeof(decimal).Name, null, null));
            RegisterDomain(new Domain<double?>(DomainPrefix + typeof(double).Name, null, null));
            RegisterDomain(new Domain<float?>(DomainPrefix + typeof(float).Name, null, null));
            RegisterDomain(new Domain<int?>(DomainPrefix + typeof(int).Name, null, null));
            RegisterDomain(new Domain<long?>(DomainPrefix + typeof(long).Name, null, null));
            RegisterDomain(new Domain<sbyte?>(DomainPrefix + typeof(sbyte).Name, null, null));
            RegisterDomain(new Domain<short?>(DomainPrefix + typeof(short).Name, null, null));
            RegisterDomain(new Domain<string>(DomainPrefix + typeof(string).Name, null, null));
            RegisterDomain(new Domain<uint?>(DomainPrefix + typeof(uint).Name, null, null));
            RegisterDomain(new Domain<ushort?>(DomainPrefix + typeof(ushort).Name, null, null));
            RegisterDomain(new Domain<ulong?>(DomainPrefix + typeof(ulong).Name, null, null));
            RegisterDomain(new Domain<byte[]>(DomainPrefix + typeof(byte[]).Name, null, null));
            RegisterDomain(new Domain<Guid?>(DomainPrefix + typeof(Guid).Name, null, null));
            RegisterDomain(new Domain<char?>(DomainPrefix + typeof(char).Name, null, null));
            RegisterDomain(new Domain<TimeSpan?>(DomainPrefix + typeof(TimeSpan).Name, null, null));
        }

        /// <summary>
        /// Retourne une instance de DomainManager.
        /// </summary>
        public static DomainManager Instance {
            get {
                return _instance;
            }
        }

        /// <summary>
        /// Retourne la description d'un domaine.
        /// </summary>
        /// <param name="domainName">Nom du domaine.</param>
        /// <returns>Domaine.</returns>
        /// <exception cref="NotSupportedException">Si le domaine n'est pas connu.</exception>
        public IDomain GetDomain(string domainName) {
            return GetDomainInternal(domainName);
        }

        /// <summary>
        /// Enregistre les domaines.
        /// </summary>
        /// <param name="domainMetadataType">Type de la classe portant les métadonnées des domaines.</param>
        /// <returns>Liste des domaines créés.</returns>
        public ICollection<IDomain> RegisterDomainMetadataType(Type domainMetadataType) {
            if (domainMetadataType == null) {
                throw new ArgumentNullException("domainMetadataType");
            }

            List<IDomain> list = new List<IDomain>();
            foreach (PropertyInfo property in domainMetadataType.GetProperties()) {
                object[] attrDomainArray = property.GetCustomAttributes(typeof(DomainAttribute), false);
                if (attrDomainArray.Length > 0) {
                    DomainAttribute domainAttr = (DomainAttribute)attrDomainArray[0];

                    List<ValidationAttribute> validationAttributes = new List<ValidationAttribute>();
                    object[] attrValidationArray = property.GetCustomAttributes(typeof(ValidationAttribute), false);
                    foreach (ValidationAttribute validationAttribute in attrValidationArray) {
                        validationAttributes.Add(validationAttribute);
                        RequiredAttribute requiredAttr = validationAttribute as RequiredAttribute;
                        StringLengthAttribute strLenAttr = validationAttribute as StringLengthAttribute;
                        RangeAttribute rangeAttr = validationAttribute as RangeAttribute;
                        if (requiredAttr != null) {
                            requiredAttr.ErrorMessageResourceName = "ConstraintNotNull";
                            requiredAttr.ErrorMessageResourceType = typeof(SR);
                        } else if (strLenAttr != null) {
                            strLenAttr.ErrorMessageResourceName = "ErrorConstraintStringLength";
                            strLenAttr.ErrorMessageResourceType = typeof(SR);
                        } else if (rangeAttr != null) {
                            rangeAttr.ErrorMessageResourceName = "ConstraintIntervalBornes";
                            rangeAttr.ErrorMessageResourceType = typeof(SR);
                        }
                    }

                    TypeConverter formatter = null;
                    object[] attrConverterArray = property.GetCustomAttributes(typeof(CustomTypeConverterAttribute), false);
                    if (attrConverterArray.Length > 0) {
                        CustomTypeConverterAttribute converterAttribute = (CustomTypeConverterAttribute)attrConverterArray[0];
                        Type converterType = Type.GetType(converterAttribute.ConverterTypeName, false);
                        if (converterType == null) {
                            string simpleTypeName = converterAttribute.ConverterTypeName.Split(',').First();
                            converterType = domainMetadataType.Assembly.GetType(simpleTypeName);
                        }

                        formatter = (TypeConverter)Activator.CreateInstance(converterType);
                        IFormatter iFormatter = (IFormatter)formatter;
                        if (!string.IsNullOrEmpty(converterAttribute.FormatString)) {
                            iFormatter.FormatString = converterAttribute.FormatString;
                        }

                        if (!string.IsNullOrEmpty(converterAttribute.Unit)) {
                            iFormatter.Unit = converterAttribute.Unit;
                        }
                    }

                    List<Attribute> decorationAttributes = new List<Attribute>();
                    foreach (object attribute in property.GetCustomAttributes(false)) {
                        if (attribute is DomainAttribute || attribute is TypeConverterAttribute || attribute is ValidationAttribute) {
                            continue;
                        }

                        Attribute extraAttribute = attribute as Attribute;
                        decorationAttributes.Add(extraAttribute);
                    }

                    IDomainChecker domain = (IDomainChecker)Activator.CreateInstance(
                        typeof(Domain<>).MakeGenericType(property.PropertyType),
                        domainAttr.Name,
                        validationAttributes,
                        formatter,
                        decorationAttributes,
                        false,
                        domainAttr.ErrorMessageResourceType,
                        domainAttr.ErrorMessageResourceName,
                        domainAttr.MetadataPropertySuffix);

                    this.RegisterDomain(domain);
                    list.Add(domain);
                }
            }

            return list;
        }


        /// <summary>
        /// Retourne le domaine associé à une propriété.
        /// </summary>
        /// <param name="property">Description de la propriété.</param>
        /// <returns>Null si aucun domaine n'est associé.</returns>
        public IDomainChecker GetDomain(BeanPropertyDescriptor property) {
            IDomainChecker domain = null;
            if (property == null) {
                throw new ArgumentNullException("property");
            }

            if (property.DomainName == null) {
                Type primitiveType = property.PrimitiveType;
                if (primitiveType != null) {
                    if (!_domainDictionary.TryGetValue(DomainPrefix + primitiveType.Name, out domain)) {
                        throw new NotSupportedException("Pas de domaine par défaut pour le type " + primitiveType.Name + " !");
                    }
                }
            } else {
                domain = GetDomainInternal(property.DomainName);
            }

            if (domain != null) {
                domain.CheckPropertyType(property);
            }

            return domain;
        }

        /// <summary>
        /// Retourne la description d'un domaine.
        /// </summary>
        /// <param name="domainName">Nom du domaine.</param>
        /// <returns>Domaine.</returns>
        /// <exception cref="NotSupportedException">Si le domaine n'est pas connu.</exception>
        internal IDomainChecker GetDomainInternal(string domainName) {
            IDomainChecker domain;
            if (!_domainDictionary.TryGetValue(domainName, out domain)) {
                throw new NotSupportedException("Domain " + domainName + " not found !");
            }

            return domain;
        }

        /// <summary>
        /// Enregistre un nouveau domaine.
        /// </summary>
        /// <param name="d">Domaine.</param>
        private void RegisterDomain(IDomainChecker d) {
            ILog log = LogManager.GetLogger("Application");
            if (log.IsDebugEnabled) {
                log.Debug("Enregistrement du domaine : " + d.Name);
            }

            _domainDictionary[d.Name] = d;
        }
    }
}
