using System;
using System.ComponentModel;

namespace Kinetix.ComponentModel {
    /// <summary>
    /// Interface d'un domaine
    /// Les domaines sont associés aux propriétés des objets métiers grâce à l'attribut DomainAttribute.
    /// Un domaine porte :
    ///     - un type de données (type primitif)
    ///     - un formateur responsable de la convertion des données depuis ou vers le
    ///       type string
    ///     - une contrainte responsable de vérifier que la donnée typée est dans les
    ///       plages de valeurs acceptées.
    /// Pour un domaine portant sur un type string, la longueur maximum autorisée est
    /// définie par une contrainte. Dans ce cas, la contrainte doit implémenter l'interface
    /// IConstraintLength. Le domaine est ainsi en mesure de publier la longueur qui
    /// lui est associée.
    /// Un domaine ne définit pas si la donnée est obligatoire ou facultative.
    /// </summary>
    public interface IDomain {

        /// <summary>
        /// Convertisseur de valeurs.
        /// </summary>
        TypeConverter Converter {
            get;
        }

        /// <summary>
        /// Obtient le nom du domaine.
        /// </summary>
        string Name {
            get;
        }

        /// <summary>
        /// Obtient le type de données du domaine.
        /// </summary>
        Type DataType {
            get;
        }

        /// <summary>
        /// Obtient la longueur maximale des données autorisées si elle est définie.
        /// </summary>
        int? Length {
            get;
        }

        /// <summary>
        /// Obtient si les données sont des données HTML.
        /// </summary>
        bool IsHtml {
            get;
        }

        /// <summary>
        /// Obtient l'attribut décoratif à partir de son type s'il a été défini, null sinon.
        /// </summary>
        /// <param name="attributeType">Type de l'attribut décoratif.</param>
        /// <returns>L'attribut.</returns>
        Attribute GetDecorationAttribute(Type attributeType);
    }
}
