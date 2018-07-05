using System;
using System.Collections.Generic;
using System.Linq.Expressions;

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
        /// Retourne la liste de référence du type TReferenceType.
        /// </summary>
        /// <param name="referenceName">Nom de la liste de référence à utiliser.</param>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <returns>Liste de référence.</returns>
        ICollection<TReferenceType> GetReferenceList<TReferenceType>(string referenceName = null);

        /// <summary>
        /// Retourne les éléments de la liste de référence du type TReference correspondant au prédicat.
        /// </summary>
        /// <typeparam name="TReference">Type de la liste de référence.</typeparam>
        /// <param name="predicate">Prédicat de filtrage.</param>
        /// <param name="referenceName">Nom de la liste de référence.</param>
        /// <returns>Ensemble des éléments.</returns>
        ICollection<TReference> GetReferenceList<TReference>(Func<TReference, bool> predicate, string referenceName = null);

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="referenceName">Nom de la liste de référence à utiliser.</param>
        /// <returns>Liste de référence.</returns>
        object GetReferenceList(Type referenceType, string referenceName);

        /// <summary>
        /// Retourne la liste de référence du type TReferenceType à partir d'un objet de critères.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="criteria">Objet contenant les critères.</param>
        /// <returns>Les éléments de la liste qui correspondent aux critères.</returns>
        ICollection<TReferenceType> GetReferenceListByCriteria<TReferenceType>(TReferenceType criteria);

        /// <summary>
        /// Retourne la liste de référence du type TReferenceType à partir d'un objet de critères.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <typeparam name="TCriteria">Type du critère.</typeparam>
        /// <param name="criteria">Objet contenant les critères.</param>
        /// <returns>Les éléments de la liste qui correspondent aux critères.</returns>
        ICollection<TReferenceType> GetReferenceListByCriteria<TReferenceType, TCriteria>(TCriteria criteria) where TCriteria : TReferenceType;

        /// <summary>
        /// Retourne un type de référence à partir de sa clef primaire.
        /// </summary>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <typeparam name="TReferenceType">Type de référence.</typeparam>
        /// <returns>Le type de référence.</returns>
        TReferenceType GetReferenceObjectByPrimaryKey<TReferenceType>(object primaryKey);

        /// <summary>
        /// Retourne un type de référence à partir de sa clef primaire.
        /// </summary>
        /// <param name="referenceType">Type de référence.</param>
        /// <param name="primaryKey">Clef primaire.</param>
        /// <returns>Le type de référence.</returns>
        object GetReferenceObjectByPrimaryKey(Type referenceType, object primaryKey);

        /// <summary>
        /// Retourne la valeur d'une liste de référence à partir de son identifiant.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="primaryKey">Clef primaire de l'objet.</param>
        /// <param name="propertySelector">Lambda expression de sélection de la propriété.</param>
        /// <returns>Valeur de la propriété sur le bean.</returns>
        string GetReferenceValueByPrimaryKey<TReferenceType>(object primaryKey, Expression<Func<TReferenceType, object>> propertySelector);

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <param name="primaryKeyArray">Liste des clés primaires.</param>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <returns>Liste de référence.</returns>
        ICollection<TReferenceType> GetReferenceListByPrimaryKeyList<TReferenceType>(params object[] primaryKeyArray);

        /// <summary>
        /// Retourne la liste de référence du type referenceType.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="primaryKeyArray">Liste des clés primaires.</param>
        /// <returns>Liste de référence.</returns>
        object GetReferenceListByPrimaryKeyList(Type referenceType, params object[] primaryKeyArray);

        /// <summary>
        /// Retourne l'élément unique de la liste de référence du type TReference correspondant au prédicat.
        /// </summary>
        /// <typeparam name="TReference">Type de la liste de référence.</typeparam>
        /// <param name="predicate">Prédicat de filtrage.</param>
        /// <param name="referenceName">Nom de la liste de référence.</param>
        /// <returns>Ensemble des éléments.</returns>
        TReference GetReferenceObject<TReference>(Func<TReference, bool> predicate, string referenceName = null);

        /// <summary>
        /// Retourne la valeur d'une liste de référence à partir
        /// de son identifiant.
        /// </summary>
        /// <typeparam name="TReferenceType">Type de la liste de référence.</typeparam>
        /// <param name="primaryKey">Identifiant de la liste de référence.</param>
        /// <param name="defaultPropertyName">Nom de la propriété par défaut à utiliser. Null pour utiliser la valeur définie au niveau de l'objet.</param>
        /// <returns>Libellé de la liste de référence.</returns>
        string GetReferenceValueByPrimaryKey<TReferenceType>(object primaryKey, string defaultPropertyName = null);

        /// <summary>
        /// Retourne la valeur d'une liste de référence à partir
        /// de son identifiant.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="primaryKey">Identifiant de la liste de référence.</param>
        /// <param name="defaultPropertyName">Nom de la propriété par défaut à utiliser. Null pour utiliser la valeur définie au niveau de l'objet.</param>
        /// <returns>Libellé de la liste de référence.</returns>
        string GetReferenceValueByPrimaryKey(Type referenceType, object primaryKey, string defaultPropertyName = null);

        /// <summary>
        /// Retourne la valeur d'une liste de référence à partir
        /// de son identifiant et d'une datasource.
        /// </summary>
        /// <param name="referenceType">Type de la liste de référence.</param>
        /// <param name="primaryKey">Identifiant de la liste de référence.</param>
        /// <param name="dataSource">Datasource.</param>
        /// <param name="defaultPropertyName">Nom de la propriété par défaut à utiliser. Null pour utiliser la valeur définie au niveau de l'objet.</param>
        /// <returns>Libellé de la liste de référence.</returns>
        string GetReferenceValueByPrimaryKey(Type referenceType, object primaryKey, object dataSource, string defaultPropertyName = null);

        /// <summary>
        /// Remet à jour le cache pour le type spécifié.
        /// </summary>
        /// <param name="referenceType">Type de référence mis en cache.</param>
        void FlushCache(Type referenceType);

        /// <summary>
        /// Flush all cache.
        /// </summary>
        void FlushAll();
    }
}
