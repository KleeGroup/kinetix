using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Interface de chargement des ressources.
    /// </summary>
    [ServiceContract]
    public interface IResourceLoader {

        /// <summary>
        /// Retourne les ressources disponibles pour un type.
        /// </summary>
        /// <param name="referenceType">Type de référence.</param>
        /// <returns>Liste des ressources.</returns>
        [OperationContract]
        [SuppressMessage("Klee.FxCop", "EX0003:LoadListRule", Justification = "Nommage correct de la méthode.")]
        ICollection<ReferenceResource> LoadReferenceResourceListByReferenceType(Type referenceType);

        /// <summary>
        /// Returns the code of the default language.
        /// </summary>
        /// <returns>Code of the default language.</returns>
        [OperationContract]
        string LoadLangueCodeDefaut();

        /// <summary>
        /// Returns the current lang code.
        /// </summary>
        /// <returns>Lang code.</returns>
        [OperationContract]
        string LoadCurrentLangueCode();
    }
}
