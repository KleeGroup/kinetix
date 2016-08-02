using System.ComponentModel;

namespace Kinetix.Reporting {
    /// <summary>
    /// Interface pour la manipulation des beans.
    /// </summary>
    internal interface IReportBean {
        /// <summary>
        /// Retourne la valeur d'une propriété.
        /// </summary>
        /// <param name="propertyDescriptor">Propriété du composant.</param>
        /// <returns>Valeur de la propriété pour ce composant.</returns>
        object GetValue(PropertyDescriptor propertyDescriptor);

        /// <summary>
        /// Définit la valeur d'une propriété.
        /// </summary>
        /// <param name="propertyDescriptor">PRopriété du composant.</param>
        /// <param name="value">Valeur à affecter à la propriété.</param>
        void SetValue(PropertyDescriptor propertyDescriptor, object value);
    }
}