using System;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Attribut permettant de spécifier le porteur de méta-données.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CustomMetadataTypeAttribute : Attribute {

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="metadataClassType">Type portant les méta-données.</param>
        /// <exception cref="System.ArgumentNullException">Si type vaut <code>Null</code>.</exception>
        public CustomMetadataTypeAttribute(Type metadataClassType) {
            if (metadataClassType == null) {
                throw new ArgumentNullException("metadataClassType");
            }

            this.MetadataClassType = metadataClassType;
        }

        /// <summary>
        /// Type portant les méta-données.
        /// </summary>
        public Type MetadataClassType {
            get;
            private set;
        }
    }
}
