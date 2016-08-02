using System.Collections;
using System.Collections.Generic;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Interface pour une entrée dans le cache de liste de référence.
    /// </summary>
    internal interface IReferenceEntry {

        /// <summary>
        /// Crée une nouvelle entrée pour le type.
        /// </summary>
        /// <param name="referenceList">Liste de référence.</param>
        /// <param name="resourceList">Liste des resources disponible.</param>
        void Initialize(ICollection referenceList, ICollection<ReferenceResource> resourceList);

        /// <summary>
        /// Retourne la liste de référence pour une locale.
        /// </summary>
        /// <param name="locale">Locale.</param>
        /// <returns>Liste de référence.</returns>
        ICollection GetReferenceList(string locale);

        /// <summary>
        /// Retourne un object de référence pour une locale.
        /// </summary>
        /// <param name="locale">Locale.</param>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <returns>Objet.</returns>
        object GetReferenceValue(string locale, object primaryKey);
    }
}
