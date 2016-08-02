using System;
using System.ComponentModel;

namespace Kinetix.Reporting {
    /// <summary>
    /// Description des propriétés d'un objet de l'arbre.
    /// </summary>
    internal class ReportPropertyDescriptor : PropertyDescriptor {

        private readonly Type _componentType;
        private readonly Type _propertyType;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="name">Nom de la propriété.</param>
        /// <param name="propertyType">Type de la propriété.</param>
        /// <param name="componentType">Type du composant.</param>
        /// <param name="attrs">Attributs de la propriété.</param>
        public ReportPropertyDescriptor(string name, Type propertyType, Type componentType, Attribute[] attrs)
            : base(name, attrs) {
            if (propertyType == null) {
                throw new ArgumentNullException("propertyType");
            }

            if (componentType == null) {
                throw new ArgumentNullException("componentType");
            }

            _componentType = componentType;
            _propertyType = propertyType;
        }

        /// <summary>
        /// Retourne le type du composant.
        /// </summary>
        public override Type ComponentType {
            get {
                return _componentType;
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
                return _propertyType;
            }
        }

        /// <summary>
        /// Indique si la valeur du composant peut être réinitialisé
        /// à sa valeur par défaut.
        /// </summary>
        /// <param name="component">Composant propriétaire de la propriété.</param>
        /// <returns>False.</returns>
        public override bool CanResetValue(object component) {
            return false;
        }

        /// <summary>
        /// Retourne la valeur de la propriété pour un bean.
        /// </summary>
        /// <param name="component">Composant propriétaire de la propriété.</param>
        /// <returns>Valeur de la propriété pour ce composant.</returns>
        public override object GetValue(object component) {
            IReportBean bean = component as IReportBean;
            if (bean == null) {
                throw new NotSupportedException();
            }

            return bean.GetValue(this);
        }

        /// <summary>
        /// Réinitialise la valeur du composant à sa valeur par défaut.
        /// </summary>
        /// <param name="component">Composant propriétaire de la propriété.</param>
        public override void ResetValue(object component) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Définit la valeur de la propriété pour un bean.
        /// </summary>
        /// <param name="component">Composant propriétaire de la propriété.</param>
        /// <param name="value">Valeur de la propriété pour le composant.</param>
        public override void SetValue(object component, object value) {
            IReportBean bean = component as IReportBean;
            if (bean == null) {
                throw new NotSupportedException();
            }

            bean.SetValue(this, value);
        }

        /// <summary>
        /// Indique si la valeur du composant doit être réinitialisée.
        /// </summary>
        /// <param name="component">Composant propriétaire de la propriété.</param>
        /// <returns>False.</returns>
        public override bool ShouldSerializeValue(object component) {
            return false;
        }
    }
}
