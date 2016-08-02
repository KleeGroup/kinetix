using System;
using System.Collections;
using System.ServiceModel;

namespace Kinetix.ServiceModel {

    /// <summary>
    /// Interface de sauvegarde des ressources.
    /// </summary>
    [ServiceContract]
    public interface IResourceWriter {

        /// <summary>
        /// Sauvegarde des traductions de beans de référence.
        /// </summary>
        /// <param name="referenceType">Type de référence.</param>
        /// <param name="beanList">Liste des beans à sauvegarder. </param>
        /// <param name="cultureUI">Culture.</param>
        [OperationContract]
        void SaveTraductionReferenceList(Type referenceType, ICollection beanList, string cultureUI);

        /// <summary>
        /// Sauvegarde de la traduction d'un bean de référence.
        /// </summary>
        /// <param name="referenceType">Type de référence.</param>
        /// <param name="bean">Bean à sauvegarder.</param>
        /// <param name="cultureUI">Culture.</param>
        [OperationContract]
        void SaveTraductionReference(Type referenceType, object bean, string cultureUI);

        /// <summary>
        /// Supprime les traductions d'un bean de référence.
        /// </summary>
        /// <param name="referenceType">Type de référence.</param>
        /// <param name="primaryKey">Clé du bean à supprimer.</param>
        [OperationContract]
        void DeleteTraductionReferenceByReferenceAndPrimaryKey(Type referenceType, object primaryKey);
    }
}
