using System;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.MetaModel;
using Nest;

namespace Kinetix.Search.Elastic {

    /// <summary>
    /// Usine à mapping ElasticSearch.
    /// </summary>
    public class ElasticMappingFactory {

        /// <summary>
        /// Obtient le mapping de champ Elastic pour une catégorie donnée.
        /// </summary>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        public IElasticType GetElasticType(DocumentFieldDescriptor field) {
            // TODO Externaliser.
            switch (field.Category) {
                case SearchFieldCategory.Result:
                    if (field.PropertyType == typeof(DateTime?)) {
                        return new DateMapping {
                            Index = NonStringIndexOption.No,
                            Store = true
                        };
                    }

                    if (field.PropertyType == typeof(decimal?) || field.PropertyType == typeof(int?)) {
                        return new NumberMapping {
                            Index = NonStringIndexOption.No,
                            Store = true
                        };
                    }

                    return new StringMapping {
                        Index = FieldIndexOption.No,
                        Store = true
                    };
                case SearchFieldCategory.Search:
                    /* Champ de recherche textuelle full-text. */
                    return new StringMapping {
                        Index = FieldIndexOption.Analyzed,
                        Store = false,
                        Analyzer = "text_fr"
                    };
                case SearchFieldCategory.Security:
                    /* Champ de filtrage de sécurité : listes de code. */
                    /* TODO : faire un mapping plus spécifique ? */
                    return new StringMapping {
                        Index = FieldIndexOption.Analyzed,
                        Store = true,
                        Analyzer = "text_fr"
                    };
                case SearchFieldCategory.Facet:
                    /* Champ de facette. */
                    if (field.PropertyType == typeof(DateTime?)) {
                        throw new ElasticException("Le type DateTime n'est pas supporté pour le champ de facette " + field.FieldName);
                    }

                    if (field.PropertyType == typeof(decimal?)) {
                        return new NumberMapping {
                            Index = NonStringIndexOption.NotAnalyzed,
                            Store = false
                        };
                    }

                    return new StringMapping {
                        Index = FieldIndexOption.NotAnalyzed,
                        Store = false
                    };
                case SearchFieldCategory.Sort:
                    if (field.PropertyType == typeof(DateTime?)) {
                        return new DateMapping {
                            Index = NonStringIndexOption.NotAnalyzed,
                            Store = false
                        };
                    }

                    if (field.PropertyType == typeof(decimal?)) {
                        return new NumberMapping {
                            Index = NonStringIndexOption.NotAnalyzed,
                            Store = false
                        };
                    }

                    return new StringMapping {
                        Index = FieldIndexOption.NotAnalyzed,
                        Store = false/*,
                        Analyzer = "sort"*/
                    };
                case SearchFieldCategory.Filter:
                    /* Champ filtre. */
                    if (field.PropertyType == typeof(DateTime?)) {
                        throw new ElasticException("Le type DateTime n'est pas supporté pour le champ de filtrage " + field.FieldName);
                    }

                    if (field.PropertyType == typeof(decimal?)) {
                        return new NumberMapping {
                            Index = NonStringIndexOption.NotAnalyzed,
                            Store = false
                        };
                    }

                    return new StringMapping {
                        Index = FieldIndexOption.NotAnalyzed,
                        Store = false
                    };
                case SearchFieldCategory.Id:
                    return null;
                default:
                    throw new NotSupportedException("Category not supported : " + field.Category);
            }
        }
    }
}
