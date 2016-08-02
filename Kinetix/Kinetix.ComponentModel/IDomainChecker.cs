using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Interface de vérification des domaines.
    /// </summary>
    public interface IDomainChecker : IDomain {

        /// <summary>
        /// Retourne l'unité associée au format.
        /// </summary>
        string Unit {
            get;
        }

        /// <summary>
        /// Retourne les attributs de validation associés.
        /// </summary>
        ICollection<ValidationAttribute> ValidationAttributes {
            get;
        }

        /// <summary>
        /// Retourne le suffix de la propriété portant les métadonnées utilent au domaine.
        /// </summary>
        string MetadataPropertySuffix {
            get;
        }

        /// <summary>
        /// Vérifie la cohérence de la propriété
        /// avec le domaine.
        /// </summary>
        /// <param name="propertyDescriptor">Propriété.</param>
        void CheckPropertyType(BeanPropertyDescriptor propertyDescriptor);

        /// <summary>
        /// Teste si la valeur passée en paramètre est valide pour le champ.
        /// </summary>
        /// <param name="value">Valeur à tester.</param>
        /// <param name="propertyDescriptor">Propriété.</param>
        /// <exception cref="System.InvalidCastException">En cas d'erreur de type.</exception>
        /// <exception cref="ConstraintException">En cas d'erreur, le message décrit l'erreur.</exception>
        void CheckValue(object value, BeanPropertyDescriptor propertyDescriptor);

        /// <summary>
        /// Converti un string en valeur.
        /// </summary>
        /// <param name="text">Valeur à convertir.</param>
        /// <param name="propertyDescriptor">Propriété.</param>
        /// <returns>Valeur dans le type cible.</returns>
        /// <exception cref="System.FormatException">En cas d'erreur de convertion.</exception>
        object ConvertFromString(string text, BeanPropertyDescriptor propertyDescriptor);

        /// <summary>
        /// Converti une valeur en string.
        /// </summary>
        /// <param name="value">Valeur à convertir.</param>
        /// <param name="propertyDescriptor">Propriété.</param>
        /// <exception cref="System.InvalidCastException">En cas d'erreur de type.</exception>
        /// <returns>La valeur sous sa forme textuelle.</returns>
        string ConvertToString(object value, BeanPropertyDescriptor propertyDescriptor);
    }
}
