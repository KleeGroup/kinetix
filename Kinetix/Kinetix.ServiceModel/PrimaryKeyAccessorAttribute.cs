using System;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Attribut indiquant qu'une méthode permet l'accès à un objet
    /// par sa clef primaire.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PrimaryKeyAccessorAttribute : Attribute {

        /// <summary>
        /// Obtient ou définit le type cible pour l'autocompletion.
        /// </summary>
        public Type TargetType {
            get;
            set;
        }
    }
}
