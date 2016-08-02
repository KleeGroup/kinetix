using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Classe de description d'une propriété.
    /// </summary>
    [Serializable]
    public sealed class BeanPropertyDescriptor {

        private IDomainChecker _domainChecker;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété.</param>
        /// <param name="memberName">Nom du membre (DataMemberAttribute).</param>
        /// <param name="propertyType">Type de la propriété.</param>
        /// <param name="description">Description de la propriété.</param>
        /// <param name="domainName">Domaine de la propriété.</param>
        /// <param name="isPrimaryKey">True si la propriété correspond à la clef primaire.</param>
        /// <param name="isDefault">True si la propriété est la propriété par défaut de l'objet.</param>
        /// <param name="isRequired">True si la propriété doit être renseignée.</param>
        /// <param name="referenceType">Type de la liste de référence associée à la propriété.</param>
        /// <param name="isReadOnly"><code>True</code> si la propriété est en lecture seule, <code>False</code> sinon.</param>
        /// <param name="isBrowsable">Indique si la propriété est affichable.</param>
        /// <param name="isTranslatable">Indique si la propriété est traduisible.</param>
        internal BeanPropertyDescriptor(
            string propertyName,
            string memberName,
            Type propertyType,
            string description,
            string domainName,
            bool isPrimaryKey,
            bool isDefault,
            bool isRequired,
            Type referenceType,
            bool isReadOnly,
            bool isBrowsable,
            bool isTranslatable) {
            this.PropertyName = propertyName;
            this.MemberName = memberName;
            this.PropertyType = propertyType;
            this.Description = description;
            this.DomainName = domainName;
            this.IsPrimaryKey = isPrimaryKey;
            this.IsDefault = isDefault;
            this.IsRequired = isRequired;
            this.ReferenceType = referenceType;
            this.IsReadOnly = isReadOnly;
            this.IsBrowsable = isBrowsable;
            this.IsTranslatable = isTranslatable;

            this.InitPrimitiveType();
            this.CheckPropertyTypeForReference();
        }

        /// <summary>
        /// Obtient la description de la valeur.
        /// </summary>
        public string Description {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le nom du domaine.
        /// </summary>
        public string DomainName {
            get;
            private set;
        }

        /// <summary>
        /// Indique si la propriété est affichable à l'écran.
        /// </summary>
        public bool IsBrowsable {
            get;
            private set;
        }

        /// <summary>
        /// Indique si la propriété est la propriété par défaut de l'objet.
        /// </summary>
        public bool IsDefault {
            get;
            private set;
        }

        /// <summary>
        /// Indique si la propriété correspond à la clef primaire.
        /// </summary>
        public bool IsPrimaryKey {
            get;
            private set;
        }

        /// <summary>
        /// Indique si la valeur est obligatoire.
        /// </summary>
        public bool IsRequired {
            get;
            private set;
        }

        /// <summary>
        /// Indique si la colonne est traduisible (dans le cas d'une liste de référence).
        /// </summary>
        public bool IsTranslatable {
            get;
            private set;
        }

        /// <summary>
        /// Retourne le nom du membre porté par DataMemberAttribute.
        /// Pour les propriétés persistantes, il s'agit
        /// de la colonne base de données. Pour des objets issus
        /// d'un service Web, il s'agit du nom Soap de la propriété.
        /// </summary>
        public string MemberName {
            get;
            private set;
        }

        /// <summary>
        /// Retourne le type primitif de la propriété.
        /// Si PropertyType vaut Nullable&lt;int&gt;, PrimitiveType vaut int.
        /// </summary>
        public Type PrimitiveType {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le nom de la propriété.
        /// </summary>
        public string PropertyName {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le type de la propriété.
        /// </summary>
        public Type PropertyType {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le type de l'objet de référence
        /// associé à la propriété.
        /// </summary>
        public Type ReferenceType {
            get;
            private set;
        }

        /// <summary>
        /// Retourne l'unité associée au format.
        /// </summary>
        public string Unit {
            get {
                return (this.Domain == null) ? null : this.Domain.Unit;
            }
        }

        /// <summary>
        /// Retourne si la propriété est en lecture seule.
        /// </summary>
        public bool IsReadOnly {
            get;
            private set;
        }

        /// <summary>
        /// Retourne le domaine associé à la propriété.
        /// </summary>
        private IDomainChecker Domain {
            get {
                if (_domainChecker == null) {
                    _domainChecker = DomainManager.Instance.GetDomain(this);
                }

                return _domainChecker;
            }
        }

        /// <summary>
        /// Vérifie que le type d'une valeur correspond bien au type
        /// de la propriété.
        /// </summary>
        /// <param name="value">Valeur à tester.</param>
        public void CheckValueType(object value) {
            if (value == null) {
                return;
            }

            Type valueType = value.GetType();
            if (valueType.IsPrimitive || typeof(decimal).Equals(valueType) || typeof(Guid).Equals(valueType) ||
                    typeof(string).Equals(valueType) || typeof(DateTime).Equals(valueType)
                    || typeof(byte[]).Equals(valueType) || typeof(TimeSpan).Equals(valueType)) {
                if (!valueType.Equals(PrimitiveType)) {
                    throw new InvalidCastException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            SR.ExceptionInvalidValueType,
                            valueType,
                            PropertyType,
                            PropertyName));
                }

                return;
            }

            ExtendedValue extValue = value as ExtendedValue;
            if (extValue != null) {
                this.CheckValueType(extValue.Value);
                return;
            }

            throw new NotSupportedException(string.Format(
                CultureInfo.CurrentUICulture,
                SR.ExceptionTypeDescription,
                PropertyType));
        }

        /// <summary>
        /// Convertit une chaine de texte en valeur.
        /// </summary>
        /// <param name="value">Chaine de caractère.</param>
        /// <returns>Valeur.</returns>
        public object ConvertFromString(string value) {
            return this.Domain.ConvertFromString(value, this);
        }

        /// <summary>
        /// Convertit la valeur d'une propriété en string.
        /// </summary>
        /// <param name="value">Valeur.</param>
        /// <returns>Chaine de texte.</returns>
        public string ConvertToString(object value) {
            return this.Domain.ConvertToString(value, this);
        }

        /// <summary>
        /// Retourne la valeur de la propriété pour un objet.
        /// </summary>
        /// <param name="bean">Objet.</param>
        /// <param name="noExtendedValue">Si <code>True</code> alors retourne la valeur associée à l'ExtendedValue, sinon la valeur réelle de la propriété.</param>
        /// <returns>Valeur.</returns>
        public object GetValue(object bean, bool noExtendedValue = false) {
            object value = TypeDescriptor.GetProperties(bean)[this.PropertyName].GetValue(bean);
            if (noExtendedValue || this.Domain == null || this.Domain.MetadataPropertySuffix == null) {
                return value;
            }

            PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(bean)[this.PropertyName + this.Domain.MetadataPropertySuffix];
            if (propertyDescriptor == null) {
                throw new NotSupportedException("ExtendedValue requires a " + this.PropertyName + this.Domain.MetadataPropertySuffix + " property to enable automatic precision.");
            }

            return new ExtendedValue(value, propertyDescriptor.GetValue(bean));
        }

        /// <summary>
        /// Définit la valeur de la propriété pour un objet.
        /// </summary>
        /// <param name="bean">Objet.</param>
        /// <param name="value">Valeur.</param>
        /// <exception cref="System.NotSupportedException">Si la propriété est en lecture seule.</exception>
        public void SetValue(object bean, object value) {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(bean)[this.PropertyName];
            if (descriptor.IsReadOnly) {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, SR.ReadOnlyProperty, this.PropertyName));
            }

            if (this.Domain == null || this.Domain.MetadataPropertySuffix == null) {
                descriptor.SetValue(bean, value);
                return;
            }

            ExtendedValue extValue = (ExtendedValue)value;
            descriptor.SetValue(bean, extValue.Value);
            PropertyDescriptor metadataDescriptor = TypeDescriptor.GetProperties(bean)[this.PropertyName + this.Domain.MetadataPropertySuffix];
            metadataDescriptor.SetValue(bean, extValue.Metadata);
        }

        /// <summary>
        /// Valide les contraintes pour une propriété.
        /// Le fait que la propriété soit requise ou non
        /// est outrepassée par le champ booléen fourni si non-nul.
        /// </summary>
        /// <param name="value">Valeur de la propriété.</param>
        /// <param name="isRequired">Si la propriété est requise.</param>
        public void ValidConstraints(object value, bool? isRequired) {

            // Vérifie les contraintes sur le champ,
            // la nullité du champ n'est pas vérifiée
            this.ValidConstraints(value, true, isRequired);
        }

        /// <summary>
        /// Retourne une chaîne de caractère représentant l'objet.
        /// </summary>
        /// <returns>Chaîne de caractère représentant l'objet.</returns>
        public override string ToString() {
            return this.PropertyName;
        }

        /// <summary>
        /// Valide les contraintes pour une propriété.
        /// </summary>
        /// <param name="value">Valeur de la propriété.</param>
        /// <param name="checkNullValue">True si la nullité doit être vérifiée.</param>
        /// <param name="isForceRequirement">Si on force le fait que la propriété est requise ou non.</param>
        internal void ValidConstraints(object value, bool checkNullValue, bool? isForceRequirement) {
            bool isRequired = this.IsRequired;
            if (isForceRequirement.HasValue) {
                isRequired = isForceRequirement.Value;
            }

            if (isRequired && checkNullValue) {
                RequiredAttribute c = new RequiredAttribute { AllowEmptyStrings = false, ErrorMessageResourceType = typeof(SR), ErrorMessageResourceName = "ConstraintNotNull" };
                ExtendedValue extVal = value as ExtendedValue;
                if (extVal != null) {
                    if (!c.IsValid(extVal.Value) || !c.IsValid(extVal.Metadata)) {
                        throw new ConstraintException(this, c.FormatErrorMessage(PropertyName));
                    }
                } else if (!c.IsValid(value)) {
                    throw new ConstraintException(this, c.FormatErrorMessage(PropertyName));
                }
            }

            this.Domain.CheckValue(value, this);
        }

        /// <summary>
        /// Vérifie la cohérence du type de la propriété si elle est associée
        /// à une liste de référence.
        /// </summary>
        private void CheckPropertyTypeForReference() {
            if (MemberName != null && ReferenceType != null && PrimitiveType != typeof(int) && PrimitiveType != typeof(string) && PrimitiveType != typeof(Guid)) {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, SR.ExceptionTypeInt32Required, PropertyType, PropertyName));
            }
        }

        /// <summary>
        /// Initialise le type primitif de la propriété.
        /// </summary>
        private void InitPrimitiveType() {
            if (PropertyType.IsPrimitive || typeof(decimal).Equals(PropertyType) || typeof(DateTime).Equals(PropertyType) || typeof(string).Equals(PropertyType) || typeof(byte[]).Equals(PropertyType)
                || typeof(System.Collections.Generic.ICollection<string>).Equals(PropertyType)
                || typeof(System.Collections.Generic.ICollection<int>).Equals(PropertyType)) {
                PrimitiveType = PropertyType;
                return;
            }

            if (PropertyType.IsGenericType && PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                Type primitive = PropertyType.GetGenericArguments()[0];
                if (primitive.IsPrimitive || typeof(decimal).Equals(primitive) || typeof(DateTime).Equals(primitive)
                    || typeof(Guid).Equals(primitive) || typeof(TimeSpan).Equals(primitive)) {
                    PrimitiveType = primitive;
                    return;
                }

                throw new NotSupportedException(string.Format(CultureInfo.CurrentUICulture, SR.ExceptionTypeDescription, PropertyType));
            }

            PrimitiveType = null;
        }
    }
}
