using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.Common {

    /// <summary>
    /// Méthodes communes entre l'analyzer et le codefix pour inheritdoc.
    /// </summary>
    public static class Inheritdoc {

        /// <summary>
        /// Détermine si le inheritDoc du symbole méthode est présent et correct.
        /// </summary>
        /// <param name="racine">Le nœud racine de l'arbre syntaxtique courant.</param>
        /// <param name="modèleSémantique">Le modèle sémantique lié.</param>
        /// <param name="méthode">La méthode concernée.</param>
        /// <returns>La ligne inheritDoc correcte dans le cas où l'actuelle est manquante/incorrecte, sinon null.</returns>
        public static string InheritDocEstCorrect(SyntaxNode racine, SemanticModel modèleSémantique, MethodDeclarationSyntax méthode) {
            var classe = méthode?.Parent as ClassDeclarationSyntax;

            // Si on est bien dans une méthode de classe.
            if (méthode != null && classe != null) {
                // On récupère la version sémantique de la méthode pour identifier ses paramètres.
                var méthodeSémantique = modèleSémantique.GetDeclaredSymbol(méthode);

                // On liste toutes les méthodes des interfaces de la classe puis on cherche l'unique méthode avec la même signature.
                var méthodeCorrespondantes = modèleSémantique.GetDeclaredSymbol(classe).Interfaces
                    .SelectMany(contrat => contrat.GetMembers())
                    .Where(méthodeInterface => méthodeInterface.Name == méthode.Identifier.Text
                        && ((méthodeInterface as IMethodSymbol)?.Parameters.SequenceEqual(méthodeSémantique.Parameters, (p1, p2) => p1.Name == p2.Name) ?? false));

                var méthodeCorrespondante = (méthodeCorrespondantes.Count() == 1 ? méthodeCorrespondantes.Single() : null) as IMethodSymbol;

                // S'il y a bien une méthode correspondante, on continue.
                if (méthodeCorrespondante != null) {
                    // On récupère le nombre de méthode du même nom dans l'interface pour savoir s'il faut spécifier les paramètres ou non.
                    var nombreMéthodesSurchargées = (méthodeCorrespondante.ContainingSymbol as INamedTypeSymbol).GetMembers()
                        .Count(méthodeInterface => méthodeInterface.Name == méthode.Identifier.Text);

#pragma warning disable SA1013, SA1513

                    // On génère la ligne de documentation.
                    var inheritDoc = $@"/// <inheritdoc cref=""{
                        RécupérerNomType(méthode, méthodeCorrespondante, modèleSémantique)
                    }.{
                        RécupérerNomMéthode(méthode, méthodeCorrespondante)
                      + RécupérerParamètres(méthode, méthodeCorrespondante, modèleSémantique, nombreMéthodesSurchargées)
                    }"" />";

#pragma warning restore SA1013, SA1513

                    // On récupère la documentation actuelle de la classe.
                    var documentationActuelle = méthode.GetLeadingTrivia().ToString().Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(" ", string.Empty);

                    // On la compare avec la ligne existante de façon bien crade, parce qu'en vrai elle n'est pas générée correctement.
                    // Désolé. J'ai vraiment essayé de faire proprement mais la génération propre de commentaires XML est odieuse.
                    // Si la ligne est différente et ne contient pas le mot "summary", on retourne la ligne de commentaire attendue.
                    if (!inheritDoc.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(" ", string.Empty)
                            .Equals(documentationActuelle) && !documentationActuelle.Contains("summary")) {
                        return inheritDoc;
                    }
                }
            }

            // Sinon on renvoie null, pour affirmer au diagnostic que tout va bien.
            return null;
        }

        /// <summary>
        /// Récupère le nom complet de la méthode (avec paramètre de type éventuel).
        /// </summary>
        /// <param name="méthode">La méthode.</param>
        /// <param name="méthodeCorrespondante">La méthode correspondante.</param>
        /// <returns>Le nom complet de la méthode.</returns>
        private static string RécupérerNomMéthode(MethodDeclarationSyntax méthode, IMethodSymbol méthodeCorrespondante) =>
            méthodeCorrespondante.Name + (méthodeCorrespondante.TypeParameters.Count() > 0 ?
                "{" + string.Join(", ", méthodeCorrespondante.TypeParameters.Select(type => type.Name)) + "}"
                : string.Empty);

        /// <summary>
        /// Récupère le nom complet du type de la méthode (avec paramètre de type éventuel).
        /// </summary>
        /// <param name="méthode">La méthode.</param>
        /// <param name="méthodeCorrespondante">La méthode correspondante.</param>
        /// <param name="modèleSémantique">Le modèle sémantique (pour simplifier les types).</param>
        /// <returns>Le nom complet de la méthode.</returns>
        private static string RécupérerNomType(MethodDeclarationSyntax méthode, IMethodSymbol méthodeCorrespondante, SemanticModel modèleSémantique) =>
            méthodeCorrespondante.ContainingType.ConstructedFrom
                .ToMinimalDisplayString(modèleSémantique, méthode.GetLocation().SourceSpan.Start)
                .Replace('<', '{').Replace('>', '}');

        /// <summary>
        /// Récupère les paramètres de la méthode.
        /// </summary>
        /// <param name="méthode">La méthode.</param>
        /// <param name="méthodeCorrespondante">La méthode correspondante.</param>
        /// <param name="modèleSémantique">Le modèle sémantique (pour simplifier les types).</param>
        /// <param name="nombreMéthodesSurchargées">Le nombre de méthodes avec le même nom dans l'interface.</param>
        /// <returns>Le nom complet de la méthode.</returns>
        private static string RécupérerParamètres(MethodDeclarationSyntax méthode, IMethodSymbol méthodeCorrespondante, SemanticModel modèleSémantique, int nombreMéthodesSurchargées) {
            if (nombreMéthodesSurchargées == 1 || méthodeCorrespondante.Parameters.Count() == 0) {
                return string.Empty;
            }

#pragma warning disable SA1513

            return $@"({string.Join(", ", méthodeCorrespondante.Parameters
                .Select(symbol => symbol.Type.ToMinimalDisplayString(
                    modèleSémantique,
                    méthode.GetLocation().SourceSpan.Start)))})";

#pragma warning restore SA1513

        }
    }
}
