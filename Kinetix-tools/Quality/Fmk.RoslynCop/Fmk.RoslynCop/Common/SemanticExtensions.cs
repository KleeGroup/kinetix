using System.Linq;
using Microsoft.CodeAnalysis;

namespace Fmk.RoslynCop.Common {

    /// <summary>
    /// Extensions pour les symboles du modèle sémantique.
    /// </summary>
    internal static class SemanticExtensions {
        private const string OperationContractAttributeName = "System.ServiceModel.OperationContractAttribute";

        private const string ServiceContractAttributeName = "System.ServiceModel.ServiceContractAttribute";
        private const string ServiceImplementationAttributeName = "System.ServiceModel.ServiceBehaviorAttribute";
        private static readonly string[] HttpVerbAttributes = {
                "System.Web.Http.HttpGetAttribute",
                "System.Web.Http.HttpPostAttribute",
                "System.Web.Http.HttpPutAttribute",
                "System.Web.Http.HttpDeleteAttribute"
            };

        /// <summary>
        /// Renvoie le nom de l'application d'une assemblée.
        /// Chaine.ReferentielImplementation => Chaine.
        /// </summary>
        /// <param name="symbol">Symbole de l'assemblée.</param>
        /// <returns>Nom de l'application.</returns>
        public static string GetApplicationName(this IAssemblySymbol symbol) {
            return symbol.Name.Split('.').First();
        }

        /// <summary>
        /// Indique si le symbole est décoré par un attribut de verbes HTTP.
        /// </summary>
        /// <param name="symbol">Symbole à analyser.</param>
        /// <returns><code>True</code> si le symbole est décoré avec un attribut de verbes HTTP.</returns>
        public static bool HasHttpVerbAttribute(this IMethodSymbol symbol) {
            if (symbol == null) {
                return false;
            }

            return symbol.GetAttributes()
                            .Any(a => HttpVerbAttributes.Contains(a.AttributeClass?.ToString()));
        }

        /// <summary>
        /// Indique si le symbole est décoré par un attribut de route.
        /// </summary>
        /// <param name="symbol">Symbole à analyser.</param>
        /// <returns><code>True</code> si le symbole est décoré avec un attribut de route.</returns>
        public static bool HasRouteAttribute(this IMethodSymbol symbol) {
            if (symbol == null) {
                return false;
            }

            return symbol.GetAttributes()
                            .Any(a => a.AttributeClass?.ToString() == "System.Web.Http.RouteAttribute");
        }

        /// <summary>
        /// Indique si un symbole est un controller Web API.
        /// </summary>
        /// <param name="symbol">Symbole.</param>
        /// <returns><code>True</code> si le symbole est un controller Web API.</returns>
        public static bool IsApiController(this INamedTypeSymbol symbol) {
            if (symbol == null || !(symbol.TypeKind == TypeKind.Class)) {
                return false;
            }

            var baseType = symbol.BaseType;
            if (baseType == null) {
                return false;
            }

            // TODO : gérer récursivement.
            return baseType.ToString() == "System.Web.Http.ApiController";
        }

        /// <summary>
        /// Indique si une assemblée est une implémentation de module métier.
        /// </summary>
        /// <param name="symbol">Symbole.</param>
        /// <returns><code>True</code> si l'assemblée est une implémentation de module métier.</returns>
        public static bool IsBusinessImplementationAssembly(this IAssemblySymbol symbol) {
            if (symbol == null) {
                return false;
            }

            if (string.IsNullOrEmpty(symbol.Name)) {
                return false;
            }

            return symbol.Name.EndsWith("Implementation", System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Indique si un symbole est une implémentation de DAL.
        /// </summary>
        /// <param name="symbol">Symbole.</param>
        /// <returns><code>True</code> si le symbole est une implémentation de DAL.</returns>
        public static bool IsDalImplementation(this INamedTypeSymbol symbol) {
            if (symbol == null || !(symbol.TypeKind == TypeKind.Class)) {
                return false;
            }

            if (symbol.Name == "AbstractDal") {
                return true;
            }

            if (!symbol.Name.StartsWith("Dal", System.StringComparison.Ordinal)) {
                return false;
            }

            return symbol.GetAttributes()
                            .Any(a => a.AttributeClass?.ToString() == ServiceImplementationAttributeName);
        }

        /// <summary>
        /// Indique si un symbole fait partie du Framework.
        /// </summary>
        /// <param name="symbol">Symbole.</param>
        /// <returns><code>True</code> si le symbole est une implémentation de DAL.</returns>
        public static bool IsFramework(this INamedTypeSymbol symbol) {
            if (symbol == null) {
                return false;
            }

            return symbol.ToString().StartsWith("Fmk.", System.StringComparison.Ordinal);
        }

        /// <summary>
        /// Indique si le symbole est une opération de contrat de service WCF.
        /// </summary>
        /// <param name="symbol">Symbole à analyser.</param>
        /// <returns><code>True</code> si le symbole est une opération de contrat de service WCF.</returns>
        public static bool IsOperationContract(this IMethodSymbol symbol) {
            if (symbol == null) {
                return false;
            }

            return symbol.GetAttributes()
                            .Any(a => a.AttributeClass?.ToString() == OperationContractAttributeName);
        }

        /// <summary>
        /// Indique si le symbole est un contrat de service WCF.
        /// </summary>
        /// <param name="symbol">Symbole à analyser.</param>
        /// <returns><code>True</code> si le symbole est un contrat de service WCF.</returns>
        public static bool IsServiceContract(this INamedTypeSymbol symbol) {
            if (symbol == null || !(symbol.TypeKind == TypeKind.Interface)) {
                return false;
            }

            return symbol.GetAttributes()
                            .Any(a => a.AttributeClass?.ToString() == ServiceContractAttributeName);
        }

        /// <summary>
        /// Indique si un symbole est une implémentation de service WCF.
        /// </summary>
        /// <param name="symbol">Symbole.</param>
        /// <returns><code>True</code> si le symbole est une implémentation de service WCF.</returns>
        public static bool IsServiceImplementation(this INamedTypeSymbol symbol) {
            if (symbol == null || !(symbol.TypeKind == TypeKind.Class)) {
                return false;
            }

            return symbol.GetAttributes()
                            .Any(a => a.AttributeClass?.ToString() == ServiceImplementationAttributeName);
        }

        /// <summary>
        /// Indique si le symbole est un attribut autorisé pour un service d'implémentation.
        /// </summary>
        /// <param name="symbol">Symbole à analyser.</param>
        /// <returns><code>True</code> si le symbole est un attribut autorisé.</returns>
        public static bool IsServiceImplementationAttribute(this INamedTypeSymbol symbol) {
            if (symbol == null || !(symbol.TypeKind == TypeKind.Class)) {
                return false;
            }

            switch (symbol.ToString()) {
                case "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Indique si deux symboles ont la même mignature.
        /// </summary>
        /// <param name="left">Méthode de gauche.</param>
        /// <param name="right">Méthode de droite.</param>
        /// <returns><code>True</code> si les méthodes ont la même signature.</returns>
        public static bool SignatureEquals(this IMethodSymbol left, IMethodSymbol right) {
            if (left == null || right == null) {
                return false;
            }

            /* Compare le nom des méthodes. */
            if (left.Name != right.Name) {
                return false;
            }

            /* Compare le type de retour. */
            if (left.ReturnType.ToString() != right.ReturnType.ToString()) {
                return false;
            }

            /* Compare les listes de paramètres */
            var leftParams = left.Parameters;
            var rightParams = right.Parameters;
            if (leftParams.Length != rightParams.Length) {
                return false;
            }

            for (int i = 0; i < leftParams.Length; ++i) {
                var leftParam = leftParams[i];
                var rightParam = rightParams[i];
                if (leftParam.Type.ToString() != rightParam.Type.ToString()) {
                    return false;
                }
            }

            return true;
        }
    }
}
