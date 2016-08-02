using System;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Indique le provider permettant de trouver la classe de métadonnées à utiliser.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class MetadataTypeProviderAttribute : Attribute {

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="providerType">Type du provider.</param>
        public MetadataTypeProviderAttribute(Type providerType) {
            this.ProviderType = providerType;
        }

        /// <summary>
        /// Type du provider.
        /// </summary>
        public Type ProviderType {
            get;
            private set;
        }
    }
}
