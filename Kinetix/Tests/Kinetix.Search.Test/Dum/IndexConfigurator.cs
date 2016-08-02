using System.Collections.Generic;
using Kinetix.Search.Elastic;
using Nest;

namespace Kinetix.Search.Test.Dum {

    /// <summary>
    /// Configurateur de l'index Chaine.
    /// </summary>
    public class IndexConfigurator : IIndexConfigurator {

        /// <inheritdoc cref="IIndexConfigurator.Configure" />
        public CreateIndexDescriptor Configure(CreateIndexDescriptor descriptor) {
            return descriptor.Analysis(y => y
                .Analyzers(z => z
                    /* Code : aucun traitement. */
                    .Add("code", new CustomAnalyzer("code") {
                        Tokenizer = "keyword",
                        Filter = new List<string> { "standard" }
                    })
                    /* Texte français pour la recherche : normalisé et découpé. */
                    .Add("text_fr", new CustomAnalyzer("text_fr") {
                        Tokenizer = "pattern",
                        Filter = new List<string> { "standard", "lowercase", "asciifolding" }
                    })
                    /* Tri : normalisé pas mais pas découpé. */
                    .Add("sort", new CustomAnalyzer("sort") {
                        Tokenizer = "keyword",
                        Filter = new List<string> { "standard", "lowercase", "asciifolding" }
                    }))
                .Tokenizers(w => w
                    .Add("keyword", new KeywordTokenizer())
                    .Add("pattern", new PatternTokenizer { Pattern = @"([^\p{L}\d^&^-^.]+)" })));
        }
    }
}
