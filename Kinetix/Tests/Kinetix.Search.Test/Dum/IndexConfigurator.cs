using Kinetix.Search.Elastic;
using Nest;

namespace Kinetix.Search.Test.Dum {

    /// <summary>
    /// Configurateur de l'index Chaine.
    /// </summary>
    public class IndexConfigurator : IIndexConfigurator {

        /// <inheritdoc cref="IIndexConfigurator.Configure" />
        public CreateIndexDescriptor Configure(CreateIndexDescriptor descriptor) {
            return descriptor.Settings(s => s.Analysis(y => y
                .Analyzers(a => a
                    .Custom("code", c => c
                        .Tokenizer("keyword")
                        .Filters("standard"))
                    .Custom("text_fr", c => c
                        .Tokenizer("pattern")
                        .Filters("standard", "lowercase", "asciifolding")))
                .Tokenizers(w => w
                    .Keyword("keyword", t => t)
                    .Pattern("pattern", t => t.Pattern(@"([^\p{L}\d^&^-^.]+)")))));
        }
    }
}
