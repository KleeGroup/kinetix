namespace Kinetix.Search.Model {

    /// <summary>
    /// Définition d'une facette.
    /// </summary>
    public interface IFacetDefinition {

        /// <summary>
        /// Nom de la facette.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Nom du champ (en PascalCase) sur lequel on facette.
        /// </summary>
        string FieldName { get; set; }

        /// <summary>
        /// Résout le libellé de la facette.
        /// </summary>
        /// <param name="primaryKey">Code ou libellé.</param>
        /// <returns>Le libellé de la facette.</returns>
        string ResolveLabel(object primaryKey);
    }
}
