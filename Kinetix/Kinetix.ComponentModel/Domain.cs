using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using Kinetix.ComponentModel.DataAnnotations;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Cette classe décrit un domaine.
    /// Les domaines sont associés aux propriétés des objets métiers grâce à
    /// la propriété DomainName de l'attribut DataDescriptionAttribute.
    /// Un domaine porte :
    ///     - un type de données (type primitif)
    ///     - un formateur responsable de la convertion des données depuis ou vers le
    ///       type string
    ///     - une contrainte responsable de vérifier que la donnée typée est dans les
    ///       plages de valeurs acceptées.
    /// Pour un domaine portant sur un type string, la longueur maximum autorisé est
    /// définie par une contrainte. Dans ce cas, la contrainte doit implémenter l'interface
    /// IConstraintLength. Le domaine est ainsi en mesure de publier la longueur qui
    /// lui est associé.
    /// Un domaine ne définit pas si la données est obligatoire ou facultative.
    /// </summary>
    /// <typeparam name="T">Type du domaine.</typeparam>
    public sealed class Domain<T> : IDomainChecker {

        private readonly PropertyInfo _propertyMessage;
        private readonly IFormatter<T> _formatter;
        private readonly IFormatter<ExtendedValue> _extendedFormatter;

        /// <summary>
        /// Crée un nouveau domaine.
        /// Le formateur et la contrainte sont facultatifs.
        /// </summary>
        /// <param name="name">Nom du domaine.</param>
        /// <param name="validationAttributes">Attributs gérant la validation de la donnée.</param>
        /// <param name="formatter">Formatteur.</param>
        public Domain(string name, ICollection<ValidationAttribute> validationAttributes, TypeConverter formatter)
            : this(name, validationAttributes, formatter, null, false, null, string.Empty, null) {
        }

        /// <summary>
        /// Crée un nouveau domaine.
        /// Le formateur et la contrainte sont facultatifs.
        /// </summary>
        /// <param name="name">Nom du domaine.</param>
        /// <param name="validationAttributes">Attributs gérant la validation de la donnée.</param>
        /// <param name="formatter">Formatteur.</param>
        /// <param name="decorationAttributes">Attributs de décoration.</param>
        public Domain(string name, ICollection<ValidationAttribute> validationAttributes, TypeConverter formatter, ICollection<Attribute> decorationAttributes)
            : this(name, validationAttributes, formatter, decorationAttributes, false, null, string.Empty, null) {
        }

        /// <summary>
        /// Crée un nouveau domaine.
        /// Le formateur et la contrainte sont facultatifs.
        /// </summary>
        /// <param name="name">Nom du domaine.</param>
        /// <param name="validationAttributes">Attributs gérant la validation de la donnée.</param>
        /// <param name="formatter">Formateur.</param>
        /// <param name="decorationAttributes">Attributs de décoration.</param>
        /// <param name="isHtml">Si les données sont en HTML.</param>
        /// <param name="errorMessageResourceType">Type contenant les messages d'erreur.</param>
        /// <param name="errorMessageResourceName">Nom de la propriété portant le message d'erreur.</param>
        /// <param name="metadataPropertySuffix">Suffix de la propriété portant les métadonnées utilent au domaine.</param>
        public Domain(string name, ICollection<ValidationAttribute> validationAttributes, TypeConverter formatter, ICollection<Attribute> decorationAttributes, bool isHtml, Type errorMessageResourceType, string errorMessageResourceName, string metadataPropertySuffix) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            Type dataType = typeof(T);
            if (dataType.IsGenericType && typeof(Nullable<>).Equals(dataType.GetGenericTypeDefinition())) {
                dataType = dataType.GetGenericArguments()[0];
                if (!dataType.IsPrimitive && !typeof(decimal).Equals(dataType)
                        && !typeof(DateTime).Equals(dataType) && !typeof(Guid).Equals(dataType) && !typeof(TimeSpan).Equals(dataType)) {
                    throw new ArgumentException(dataType + "? is not a primitive Type");
                }
            } else if (!typeof(string).Equals(dataType) && !typeof(byte[]).Equals(dataType)
                && !typeof(System.Collections.Generic.ICollection<string>).Equals(dataType)
                && !typeof(System.Collections.Generic.ICollection<int>).Equals(dataType)) {
                throw new ArgumentException(dataType + " is not a nullable Type");
            }

            this.Name = name;
            this.DataType = dataType;
            _formatter = formatter as IFormatter<T>;
            _extendedFormatter = formatter as IFormatter<ExtendedValue>;
            this.ValidationAttributes = validationAttributes;
            this.IsHtml = isHtml;
            this.DecorationAttributes = decorationAttributes;
            this.ErrorMessageResourceType = errorMessageResourceType;
            this.ErrorMessageResourceName = errorMessageResourceName;
            this.MetadataPropertySuffix = metadataPropertySuffix;

            if (this.ErrorMessageResourceType != null && !string.IsNullOrEmpty(this.ErrorMessageResourceName)) {
                _propertyMessage = this.ErrorMessageResourceType.GetProperty(this.ErrorMessageResourceName, BindingFlags.Public | BindingFlags.Static);
                if (_propertyMessage == null) {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "{0} doesn't have a property named {1}.", this.ErrorMessageResourceType.FullName, this.ErrorMessageResourceName));
                }

                if (_propertyMessage.PropertyType != typeof(string)) {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "{0}.{1} is not a string typed property.", this.ErrorMessageResourceType.FullName, this.ErrorMessageResourceName));
                }
            }
        }

        /// <summary>
        /// Obtient la clef de ressource pour les erreurs de conversion.
        /// </summary>
        public string ErrorMessageResourceName {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le type portant les messages pour les erreurs de conversion.
        /// </summary>
        public Type ErrorMessageResourceType {
            get;
            private set;
        }

        /// <summary>
        /// Liste des attributs de décoration.
        /// </summary>
        public ICollection<Attribute> DecorationAttributes {
            get;
            private set;
        }

        /// <summary>
        /// Liste des attributs de validation.
        /// </summary>
        public ICollection<ValidationAttribute> ValidationAttributes {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le nom du domaine.
        /// </summary>
        public string Name {
            get;
            private set;
        }

        /// <summary>
        /// Obtient ou définit le suffix de la propriété portant les métadonnées utilent au domaine.
        /// </summary>
        public string MetadataPropertySuffix {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le type de données du domaine.
        /// </summary>
        public Type DataType {
            get;
            private set;
        }

        /// <summary>
        /// Obtient la longueur maximale des données autorisées si elle est définie.
        /// </summary>
        public int? Length {
            get {
                StringLengthAttribute strLenAttr = GetValidationAttribute<StringLengthAttribute>();
                if (strLenAttr != null) {
                    return strLenAttr.MaximumLength;
                }

                RangeAttribute ranAttr = GetValidationAttribute<RangeAttribute>();
                if (ranAttr != null && ranAttr.Maximum != null) {
                    return ranAttr.Maximum.ToString().Length;
                }

                DateAttribute dateAttr = GetValidationAttribute<DateAttribute>();
                if (dateAttr != null) {
                    return dateAttr.Precision;
                }

                NumeroSiretAttribute siretAttr = GetValidationAttribute<NumeroSiretAttribute>();
                if (siretAttr != null) {
                    return NumeroSiretAttribute.SiretLength;
                }

                return null;
            }
        }

        /// <summary>
        /// Obtient si les données associées sont du Html.
        /// </summary>
        public bool IsHtml {
            get;
            private set;
        }

        /// <summary>
        /// Retourne le convertisseur de type.
        /// </summary>
        public TypeConverter Converter {
            get {
                if (_extendedFormatter != null) {
                    return (TypeConverter)_extendedFormatter;
                }

                if (_formatter != null) {
                    return (TypeConverter)_formatter;
                }

                return null;
            }
        }

        /// <summary>
        /// Retourne l'unité associée au format.
        /// </summary>
        string IDomainChecker.Unit {
            get {
                if (_formatter != null) {
                    return _formatter.Unit;
                }

                if (_extendedFormatter != null) {
                    return _extendedFormatter.Unit;
                }

                return null;
            }
        }

        /// <summary>
        /// Retourne les attributs de validation associés.
        /// </summary>
        ICollection<ValidationAttribute> IDomainChecker.ValidationAttributes {
            get {
                return this.ValidationAttributes;
            }
        }

        /// <summary>
        /// Obtient la valeur d'un attribut décoratif à partir de son type s'il a été défini, null sinon.
        /// </summary>
        /// <param name="attributeType">Type de l'attribut décoratif.</param>
        /// <returns>Valeur de l'attribut.</returns>
        public Attribute GetDecorationAttribute(Type attributeType) {
            if (attributeType == null) {
                throw new ArgumentNullException("attributeType");
            }

            if (DecorationAttributes == null) {
                return null;
            }

            foreach (Attribute attr in DecorationAttributes) {
                if (attr.GetType() == attributeType) {
                    return attr;
                }
            }

            return null;
        }

        /// <summary>
        /// Obtient la valeur d'un attribut de validation à partir de son type s'il a été défini, null sinon.
        /// </summary>
        /// <param name="attributeType">Type de l'attribut de validation.</param>
        /// <returns>Valeur de l'attribut.</returns>
        public Attribute GetValidationAttribute(Type attributeType) {
            if (attributeType == null) {
                throw new ArgumentNullException("attributeType");
            }

            if (ValidationAttributes == null) {
                return null;
            }

            foreach (Attribute attr in ValidationAttributes) {
                if (attributeType.IsAssignableFrom(attr.GetType())) {
                    return attr;
                }
            }

            return null;
        }

        /// <summary>
        /// Obtient la valeur d'un attribut de validation à partir de son type s'il a été défini, null sinon.
        /// </summary>
        /// <returns>Valeur de l'attribut.</returns>
        /// <typeparam name="TValidation">Type de l'attribut de validation.</typeparam>
        public TValidation GetValidationAttribute<TValidation>()
            where TValidation : class {
            if (ValidationAttributes == null) {
                return default(TValidation);
            }

            foreach (Attribute attr in ValidationAttributes) {
                if (typeof(TValidation).IsAssignableFrom(attr.GetType())) {
                    return attr as TValidation;
                }
            }

            return default(TValidation);
        }

        /// <summary>
        /// Vérifie la cohérence de la propriété
        /// avec le domaine.
        /// </summary>
        /// <param name="propertyDescriptor">Propriété.</param>
        void IDomainChecker.CheckPropertyType(BeanPropertyDescriptor propertyDescriptor) {
            if (propertyDescriptor == null) {
                throw new ArgumentNullException("propertyDescriptor");
            }

            if (!this.DataType.Equals(propertyDescriptor.PrimitiveType)) {
                if (propertyDescriptor.PrimitiveType != null) {
                    throw new NotSupportedException("Invalid property type " + propertyDescriptor.PropertyType +
                            " for domain " + this.Name + " " + this.DataType + " expected." + propertyDescriptor.PrimitiveType);
                }

                throw new NotSupportedException("Invalid property type " + propertyDescriptor.PropertyType +
                                                " for domain " + this.Name + " " + this.DataType + " expected. PrimitiveType is null.");
            }
        }

        /// <summary>
        /// Teste si la valeur passée en paramètre est valide pour le champ.
        /// </summary>
        /// <param name="value">Valeur à tester.</param>
        /// <param name="propertyDescriptor">Propriété.</param>
        /// <exception cref="System.InvalidCastException">En cas d'erreur de type.</exception>
        /// <exception cref="ConstraintException">En cas d'erreur, le message décrit l'erreur.</exception>
        void IDomainChecker.CheckValue(object value, BeanPropertyDescriptor propertyDescriptor) {
            if (propertyDescriptor == null) {
                throw new ArgumentNullException("propertyDescriptor");
            }

            CheckValueType(value, propertyDescriptor);
            CheckValueValidation(value, propertyDescriptor);
        }

        /// <summary>
        /// Converti un string en valeur.
        /// </summary>
        /// <param name="text">Valeur à convertir.</param>
        /// <param name="propertyDescriptor">Propriété.</param>
        /// <returns>Valeur dans le type cible.</returns>
        /// <exception cref="System.FormatException">En cas d'erreur de convertion.</exception>
        object IDomainChecker.ConvertFromString(string text, BeanPropertyDescriptor propertyDescriptor) {
            if (propertyDescriptor == null) {
                throw new ArgumentNullException("propertyDescriptor");
            }

            object v = null;
            try {
                if (_formatter != null) {
                    v = _formatter.ConvertFromString(text);
                } else if (_extendedFormatter != null) {
                    v = _extendedFormatter.ConvertFromString(text);
                } else {
                    if (!string.IsNullOrEmpty(text)) {
                        v = TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(text);
                    }
                }
            } catch (Exception e) {
                string message = e.Message;
                if (_propertyMessage != null) {
                    message = string.Format(CultureInfo.CurrentCulture, (string)_propertyMessage.GetValue(null, null), text);
                }

                throw new ConstraintException(propertyDescriptor, message, e);
            }

            CheckValueType(v, propertyDescriptor);
            CheckValueValidation(v, propertyDescriptor);
            return v;
        }

        /// <summary>
        /// Converti une valeur en string.
        /// </summary>
        /// <param name="value">Valeur à convertir.</param>
        /// <param name="propertyDescriptor">Propriété.</param>
        /// <exception cref="System.InvalidCastException">En cas d'erreur de type.</exception>
        /// <returns>La valeur sous sa forme textuelle.</returns>
        string IDomainChecker.ConvertToString(object value, BeanPropertyDescriptor propertyDescriptor) {
            if (propertyDescriptor != null) {
                CheckValueType(value, propertyDescriptor);
            }

            if (_formatter != null) {
                return _formatter.ConvertToString((T)value);
            }

            ExtendedValue extValue = value as ExtendedValue;
            if (_extendedFormatter != null && extValue != null) {
                return _extendedFormatter.ConvertToString(extValue);
            }

            return TypeDescriptor.GetConverter(typeof(T)).ConvertToString(value);
        }

        /// <summary>
        /// Vérifie la cohérence de la valeur passée et de la propriété
        /// avec le domaine.
        /// </summary>
        /// <param name="value">Valeur.</param>
        /// <param name="propertyDescriptor">Propriété.</param>
        private static void CheckValueType(object value, BeanPropertyDescriptor propertyDescriptor) {
            propertyDescriptor.CheckValueType(value);
        }

        /// <summary>
        /// Vérifie la cohérence de la valeur passée avec les attributs de validations du domaine.
        /// </summary>
        /// <param name="value">Valeur.</param>
        /// <param name="propertyDescriptor">Propriété.</param>
        private void CheckValueValidation(object value, BeanPropertyDescriptor propertyDescriptor) {
            if (this.ValidationAttributes != null) {
                foreach (ValidationAttribute validationAttribute in this.ValidationAttributes) {
                    if (!validationAttribute.IsValid(value)) {
                        string errorMessage;
                        try {
                            errorMessage = validationAttribute.FormatErrorMessage(propertyDescriptor.PropertyName);
                        } catch (Exception e) {
                            throw new ConstraintException(propertyDescriptor, "Impossible de formater le message d'erreur pour la propriété " + propertyDescriptor.PropertyName, e);
                        }

                        throw new ConstraintException(propertyDescriptor, errorMessage);
                    }
                }
            }
        }
    }
}
