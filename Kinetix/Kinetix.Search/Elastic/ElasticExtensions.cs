using System.Text;
using log4net;
using Nest;

namespace Kinetix.Search.Elastic {

    /// <summary>
    /// Méthodes d'extensions pour ElasticSearch.
    /// </summary>
    internal static class ElasticExtensions {

        private static ILog _log = LogManager.GetLogger("Search");

        /// <summary>
        /// Traite les réponses en erreurs de Elastic Search.
        /// </summary>
        /// <param name="response">Réponse.</param>
        /// <param name="context">Contexte pour le message.</param>
        public static void CheckStatus(this IResponse response, string context) {
            if (_log.IsInfoEnabled) {
                _log.InfoFormat(
                    "{0} {1} {2}",
                    response.ApiCall.HttpMethod,
                    response.ApiCall.Uri,
                    response.ApiCall.HttpStatusCode);
            }

            if (_log.IsDebugEnabled) {
                var request = response.ApiCall.RequestBodyInBytes;
                if (request != null) {
                    var str = Encoding.UTF8.GetString(request);
                    _log.Debug(str);
                }
            }

            if (!response.ApiCall.Success) {
                var ex = response.ServerError;
                var sb = new StringBuilder();
                sb.Append("Error " + response.ApiCall.HttpStatusCode + " in ");
                sb.Append(context);
                if (ex != null) {
                    sb.Append(" : [");
                    sb.Append(ex.Error.Type);
                    sb.Append("] ");
                    sb.Append(ex.Error);
                }

                string message = sb.ToString();
                throw new ElasticException(message);
            }
        }
    }
}
