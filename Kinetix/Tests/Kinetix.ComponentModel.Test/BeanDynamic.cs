using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Kinetix.ComponentModel.Test.Contract;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Bean implémentatant ICustomTypeDescriptor.
    /// </summary>
    public class BeanDynamic : ICustomTypeDescriptor {

        private readonly Hashtable _valueTable = new Hashtable();
        private readonly PropertyDescriptorCollection _propertyDescriptors;

        /// <summary>
        /// Constructeur.
        /// </summary>
        public BeanDynamic() {
            PropertyDescriptor[] propertyArray = new PropertyDescriptor[4];

            DataMemberAttribute memberAttribute = new DataMemberAttribute();
            memberAttribute.IsRequired = true;
            RequiredAttribute requiredAttribute = new RequiredAttribute();
            ColumnAttribute columnAttribute = new ColumnAttribute("BEA_ID");
            KeyAttribute keyAttribute = new KeyAttribute();
            DatabaseGeneratedAttribute dbGenAttr = new DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity);
            ReferencedTypeAttribute associationAttribute = new ReferencedTypeAttribute(typeof(Bean));
            DomainAttribute domainAttribute = new DomainAttribute("IDENTIFIANT");

            propertyArray[0] = new DynamicPropertyDescriptor("Id", typeof(int?),
                    new Attribute[] { associationAttribute, memberAttribute, domainAttribute, columnAttribute, requiredAttribute, keyAttribute, dbGenAttr });

            memberAttribute = new DataMemberAttribute();
            columnAttribute = new ColumnAttribute("OTH_ID");
            domainAttribute = new DomainAttribute("IDENTIFIANT");
            propertyArray[1] = new DynamicPropertyDescriptor("OtherId", typeof(int?),
                    new Attribute[] { domainAttribute, memberAttribute, columnAttribute });

            propertyArray[2] = new DynamicPropertyDescriptor("Libelle", typeof(string), new Attribute[] { });

            propertyArray[3] = new DynamicPropertyDescriptor("DateCreation", typeof(DateTime?), new Attribute[] { });

            _propertyDescriptors = new PropertyDescriptorCollection(propertyArray);
        }

        /// <summary>
        /// Retourne la liste des attributs.
        /// </summary>
        /// <returns>Liste vide : pas d'attribut.</returns>
        public AttributeCollection GetAttributes() {
            return AttributeCollection.Empty;
        }

        /// <summary>
        /// Retourne le nom de la classe.
        /// </summary>
        /// <returns>Null : null pas de nom de classe.</returns>
        public string GetClassName() {
            return null;
        }

        /// <summary>
        /// Retourne le nom du composant.
        /// </summary>
        /// <returns>Null : pas de nom de composant.</returns>
        public string GetComponentName() {
            return "ReferenceId";
        }

        /// <summary>
        /// Retourne le formatter pour l'objet.
        /// </summary>
        /// <returns>Null : pas de formatter.</returns>
        public TypeConverter GetConverter() {
            return null;
        }

        /// <summary>
        /// Retourne l'évènement par défaut.
        /// </summary>
        /// <returns>Null : pas d'évènement.</returns>
        public EventDescriptor GetDefaultEvent() {
            return null;
        }

        /// <summary>
        /// Retourne la propriété par défaut.
        /// Non implémenté.
        /// </summary>
        /// <returns>Propriété par défaut.</returns>
        public PropertyDescriptor GetDefaultProperty() {
            return null;
        }

        /// <summary>
        /// Retourne un éditeur pour le composant.
        /// </summary>
        /// <param name="editorBaseType">Type.</param>
        /// <returns>Null : pas d'éditeur.</returns>
        public object GetEditor(Type editorBaseType) {
            return null;
        }

        /// <summary>
        /// Retourne la liste des évènements.
        /// </summary>
        /// <param name="attributes">Attribut de filtrage.</param>
        /// <returns>Liste vide.</returns>
        public EventDescriptorCollection GetEvents(Attribute[] attributes) {
            return EventDescriptorCollection.Empty;
        }

        /// <summary>
        /// Retourne la liste des évènements.
        /// </summary>
        /// <returns>Retourne une liste vide.</returns>
        public EventDescriptorCollection GetEvents() {
            return EventDescriptorCollection.Empty;
        }

        /// <summary>
        /// Retourne la liste des propriétés du bean ayant un attribut.
        /// </summary>
        /// <param name="attributes">Attributs.</param>
        /// <returns>Propriétés.</returns>
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retourne la liste des propriétés du bean.
        /// </summary>
        /// <returns>Propriétés.</returns>
        public PropertyDescriptorCollection GetProperties() {
            return _propertyDescriptors;
        }

        /// <summary>
        /// Retourne le propriétaire de la propriété.
        /// </summary>
        /// <param name="pd">Property descriptor.</param>
        /// <returns>Le bean.</returns>
        public object GetPropertyOwner(PropertyDescriptor pd) {
            return this;
        }

        /// <summary>
        /// Retourne la valeur d'une propriété.
        /// </summary>
        /// <param name="pd">Property descripto.</param>
        /// <returns>Valeur de la propriété.</returns>
        private object GetValue(PropertyDescriptor pd) {
            if (_valueTable.ContainsKey(pd.Name)) {
                return _valueTable[pd.Name];
            }
            return null;
        }

        /// <summary>
        /// Définit la valeur d'une propriété.
        /// </summary>
        /// <param name="pd">Property Descriptor.</param>
        /// <param name="value">Valeur.</param>
        private void SetValue(PropertyDescriptor pd, object value) {
            _valueTable[pd.Name] = value;
        }

        /// <summary>
        /// Property descriptor dynamic.
        /// </summary>
        private class DynamicPropertyDescriptor : PropertyDescriptor {
            private Type _type;

            /// <summary>
            /// Crée une nouvelle instance.
            /// </summary>
            /// <param name="name">Nom de la propriété.</param>
            /// <param name="type">Type de la propriété.</param>
            /// <param name="attrs">Attributs de la propriété.</param>
            public DynamicPropertyDescriptor(string name, Type type, Attribute[] attrs)
                : base(name, attrs) {
                _type = type;
            }

            /// <summary>
            /// Retourne le type du composant.
            /// </summary>
            public override Type ComponentType {
                get {
                    return typeof(BeanDynamic);
                }
            }

            /// <summary>
            /// Indique si le composant est en lecture seule.
            /// </summary>
            public override bool IsReadOnly {
                get {
                    return false;
                }
            }

            /// <summary>
            /// Indique le type de la propriété.
            /// </summary>
            public override Type PropertyType {
                get {
                    return _type;
                }
            }

            /// <summary>
            /// Indique si la valeur du composant peut être réinitialisé
            /// à sa valeur par défaut.
            /// </summary>
            /// <param name="component">Composant.</param>
            /// <returns>False.</returns>
            public override bool CanResetValue(object component) {
                return false;
            }

            /// <summary>
            /// Retourne la valeur de la propriété pour un bean.
            /// </summary>
            /// <param name="component">Bean.</param>
            /// <returns>Valeur.</returns>
            public override object GetValue(object component) {
                BeanDynamic bean = component as BeanDynamic;
                if (bean == null) {
                    throw new NotSupportedException();
                }
                return bean.GetValue(this);
            }

            /// <summary>
            /// Définit la valeur de la propriété pour un bean.
            /// </summary>
            /// <param name="component">Bean.</param>
            /// <param name="value">Valeur.</param>
            public override void SetValue(object component, object value) {
                BeanDynamic bean = component as BeanDynamic;
                if (bean == null) {
                    throw new NotSupportedException();
                }
                bean.SetValue(this, value);
            }

            /// <summary>
            /// Réinitialise la valeur du composant à sa valeur par défaut.
            /// </summary>
            /// <param name="component">Composant.</param>
            public override void ResetValue(object component) {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Indique si la valeur du composant doit être réinitialisé.
            /// </summary>
            /// <param name="component">Composant.</param>
            /// <returns>False.</returns>
            public override bool ShouldSerializeValue(object component) {
                return false;
            }
        }
    }
}
