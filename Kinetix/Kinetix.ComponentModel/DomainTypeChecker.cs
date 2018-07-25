using System;
using System.Collections.Generic;

namespace Kinetix.ComponentModel
{
    /// <summary>
    /// Classe pour la gestion des types associés aux domaines.
    /// </summary>
    public class DomainTypeChecker {

        /// <summary>
        /// Singleton.
        /// </summary>
        private static readonly DomainTypeChecker _instance = new DomainTypeChecker();

        /// <summary>
        /// Liste des types complexes autorisé pour les domaines
        /// </summary>
        private ICollection<Type> _whiteListComplexType = new List<Type>();


        /// <summary>
        /// Retourne une instance de DomainTypeChecker.
        /// </summary>
        public static DomainTypeChecker Instance => _instance;

        /// <summary>
        /// Retourne la liste des Types complexes autorisées dans les domaines
        /// </summary>
        public ICollection<Type> WhiteListComplexType => _whiteListComplexType;

        /// <summary>
        /// Ajoute un type complexe pouvant être utilisé par un Domaine.
        /// L'ajout de type complexe doit être effectué avec parcimonie pour une utilisation avec Entity Framework uniquement.
        /// Attention, les types complexes ne sont pas compatibles avec le Broker Kinetix :
        ///     - La factory DataRecordAdapterFactory et AbstractDataReaderAdapter ne contiennent pas les 
        /// méthodes de read appelées par le code IL (DataRecordAdapterFactory.GetReadMethodByType)
        /// </summary>
        /// <param name="type">Le type complexe associé au doamine</param>
        public void AddComplexType(Type type) {
            _whiteListComplexType.Add(type);
        }


        /// <summary>
        /// Permet de vérifier si le type associé à un domaine est authorisé.
        /// </summary>
        /// <param name="dataType">Le type vérifié et transformé en non nullable</param>
        public Type CheckTypeAndExtractNullable(Type dataType)
        {
            if (dataType.IsGenericType && typeof(Nullable<>).Equals(dataType.GetGenericTypeDefinition()))
            {
                dataType = dataType.GetGenericArguments()[0];
                if (!dataType.IsPrimitive && !typeof(decimal).Equals(dataType)
                        && !typeof(DateTime).Equals(dataType) && !typeof(Guid).Equals(dataType) && !typeof(TimeSpan).Equals(dataType))
                {
                    throw new ArgumentException(dataType + "? is not a primitive Type");
                }
            }
            else if (!typeof(string).Equals(dataType) && !typeof(byte[]).Equals(dataType)
              && !_whiteListComplexType.Contains(dataType)
              && !typeof(System.Collections.Generic.ICollection<string>).Equals(dataType)
              && !typeof(System.Collections.Generic.ICollection<int>).Equals(dataType))
            {
                throw new ArgumentException(dataType + " is not a nullable Type");
            }

            return dataType;
        }

        
    }
}
