using System;

namespace Kinetix.ServiceModel {
    /// <summary>
    /// Attribut indiquant qu'une méthode permet l'accès à un objet
    /// par sa clef primaire.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AutoCompleteAccessorAttribute : Attribute {

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AutoCompleteAccessorAttribute() {
            this.MinimumPrefixLength = 3;
        }

        /// <summary>
        /// Nom de l'accesseur.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le type cible pour l'autocompletion.
        /// </summary>
        public Type TargetType {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la propriété déterminant la taille minimal de déclenchement de l'autocomplete.
        /// </summary>
        public int MinimumPrefixLength {
            get;
            set;
        }
    }
}
