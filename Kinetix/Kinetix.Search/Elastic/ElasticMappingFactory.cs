using Kinetix.Search.ComponentModel;
using Kinetix.Search.MetaModel;
using Nest;
using System;

namespace Kinetix.Search.Elastic
{

    /// <summary>
    /// Usine à mapping ElasticSearch.
    /// </summary>
    public class ElasticMappingFactory
    {

        /// <summary>
        /// Obtient le mapping de champ Elastic pour une catégorie donnée.
        /// </summary>
        /// <param name="selector">Descripteur des propriétés.</param>
        /// <param name="field">Catégorie de champ.</param>
        /// <returns>Mapping de champ.</returns>
        /// <typeparam name="T">Type du document.</typeparam>
        public PropertiesDescriptor<T> AddField<T>(PropertiesDescriptor<T> selector, DocumentFieldDescriptor field)
            where T : class
        {
            var fieldName = field.FieldName;

            /* TODO Externaliser. */

            switch (field.SearchCategory)
            {
                case SearchFieldCategory.Result:
                    if (field.PropertyType == typeof(DateTime?))
                    {
                        return selector.Date(x => x
                            .Name(fieldName)
                            .Index(false)
                            .Store(true));
                    }

                    if (field.PropertyType == typeof(int?))
                    {
                        return selector.Number(x => x
                            .Name(fieldName)
                            .Type(NumberType.Integer)
                            .Index(false)
                            .Store(true));
                    }

                    if (field.PropertyType == typeof(decimal?))
                    {
                        return selector.Number(x => x
                            .Name(fieldName)
                            .Type(NumberType.Double)
                            .Index(false)
                            .Store(true));
                    }

                    return selector.Text(x => x
                        .Name(fieldName)
                        .Index(false)
                        .Store(true));

                case SearchFieldCategory.TextSearch:
                    /* Champ de recherche textuelle full-text. */
                    return selector.Text(x => x
                        .Name(fieldName)
                        .Index(true)
                        .Store(false)
                        .Analyzer("text_fr"));

                case SearchFieldCategory.Term:
                    /* Champ de facette/filtre. */
                    if (field.PropertyType == typeof(DateTime?))
                    {
                        throw new ElasticException("Le type DateTime n'est pas supporté pour les champ de Term " + field.FieldName);
                    }

                    if (field.PropertyType == typeof(decimal?))
                    {
                        return selector.Number(x => x
                            .Name(fieldName)
                            .Index(true)
                            .Store(false));
                    }

                    return selector.Keyword(x => x
                        .Name(fieldName)
                        .Index(true)
                        .Store(false));

                case SearchFieldCategory.ListTerm:
                    return selector.Text(x => x
                        .Name(fieldName)
                        .Index(true)
                        .Store(false)
                        .Analyzer("text_fr"));

                case SearchFieldCategory.Sort:
                    if (field.PropertyType == typeof(DateTime?))
                    {
                        return selector.Date(x => x
                            .Name(fieldName)
                            .Index(true)
                            .Store(false));
                    }

                    if (field.PropertyType == typeof(decimal?))
                    {
                        return selector.Number(x => x
                            .Name(fieldName)
                            .Index(true)
                            .Store(false));
                    }

                    return selector.Keyword(x => x
                        .Name(fieldName)
                        .Index(true)
                        .Store(false));

                case SearchFieldCategory.None:
                    return selector;

                default:
                    throw new NotSupportedException("Category not supported : " + field.SearchCategory);
            }
        }
    }
}
