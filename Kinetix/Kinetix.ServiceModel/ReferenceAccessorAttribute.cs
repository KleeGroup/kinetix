using System;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Attribut indiquant qu'une méthode permet l'accès à une
    /// liste de reférence.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ReferenceAccessorAttribute : Attribute {

        /// <summary>
        /// Retourne le nom de l'accesseur.
        /// </summary>
        public string Name {
            get;
            set;
        }
    }
}
