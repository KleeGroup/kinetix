using System;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Interface pour le gestionnaire de liste de référence.
    /// </summary>
    public interface IReferenceManager {

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <returns>Liste de référence.</returns>
        object GetReferenceList(Type referenceType);

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="referenceName">Nom de la liste de référence à utiliser.</param>
        /// <returns>Liste de référence.</returns>
        object GetReferenceList(Type referenceType, string referenceName);

        /// <summary>
        /// Retourne un type de référence à partir de sa clef primaire.
        /// </summary>
        /// <param name="referenceType">Type de référence.</param>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <returns>Le type de référence.</returns>
        object GetReferenceObjectByPrimaryKey(Type referenceType, object primaryKey);

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="primaryKeyArray">Liste des clés primaires.</param>
        /// <returns>Liste de référence.</returns>
        object GetReferenceListByPrimaryKeyList(Type referenceType, params object[] primaryKeyArray);

        /// <summary>
        /// Retourne la valeur d'une liste de référence à partir
        /// de son identifiant.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="primaryKey">Identifiant de la liste de référence.</param>
        /// <param name="defaultPropertyName">Nom de la propriété par défaut à utiliser. Null pour utiliser la valeur définie au niveau de l'objet.</param>
        /// <returns>Libellé de la liste de référence.</returns>
        string GetReferenceValueByPrimaryKey(Type referenceType, object primaryKey, string defaultPropertyName);

        /// <summary>
        /// Remet à jour le cache pour le type spécifié.
        /// </summary>
        /// <param name="referenceType">Type de référence mis en cache.</param>
        void FlushCache(Type referenceType);

        /// <summary>
        /// Flisuf all cache.
        /// </summary>
        void FlushAll();
    }
}
