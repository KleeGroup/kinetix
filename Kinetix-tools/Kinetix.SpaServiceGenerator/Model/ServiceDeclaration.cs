﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Kinetix.SpaServiceGenerator.Model {

    /// <summary>
    /// Représente un service.
    /// </summary>
    public struct ServiceDeclaration {

        /// <summary>
        /// La route du service.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Le verbe du service (get ou post).
        /// </summary>
        public Verb Verb { get; set; }

        /// <summary>
        /// Le nom du service.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Le type de retour du service.
        /// </summary>
        public INamedTypeSymbol ReturnType { get; set; }

        /// <summary>
        /// Les paramètres du service.
        /// </summary>
        public ICollection<Parameter> Parameters { get; set; }

        /// <summary>
        /// La documentation du service.
        /// </summary>
        public Documentation Documentation { get; set; }
    }
}