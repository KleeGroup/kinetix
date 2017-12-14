using Microsoft.CodeAnalysis;

namespace Kinetix.SpaServiceGenerator.Model {

    /// <summary>
    /// Représente un paramètre de service.
    /// </summary>
    public struct Parameter {

        /// <summary>
        /// Le type du paramètre.
        /// </summary>
        public INamedTypeSymbol Type { get; set; }

        /// <summary>
        /// Le nom du paramètre.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optionnel ou non.
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Avec attribut FromBody ou non.
        /// </summary>
        public bool? IsFromBody { get; set; }

    }
}
