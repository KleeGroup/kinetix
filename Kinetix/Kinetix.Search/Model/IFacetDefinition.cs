namespace Kinetix.Search.Model {

    /// <summary>
    /// Définition d'une facette.
    /// </summary>
    public interface IFacetDefinition {

        /// <summary>
        /// Code de la facette.
        /// </summary>
        string Code { get; set; }

        /// <summary>
        /// Libellé de la facette.
        /// </summary>
        string Label { get; set; }

        /// <summary>
        /// Nom du champ (en PascalCase) sur lequel on facette.
        /// </summary>
        string FieldName { get; set; }

        /// <summary>
        /// Précise s'il est possible de sélectionner plusieurs valeurs en même temps sur la facette.
        /// </summary>
        bool IsMultiSelectable { get; }

        /// <summary>
        /// Résout le libellé de la facette.
        /// </summary>
        /// <param name="primaryKey">Code ou libellé.</param>
        /// <returns>Le libellé de la facette.</returns>
        string ResolveLabel(object primaryKey);
    }
}
