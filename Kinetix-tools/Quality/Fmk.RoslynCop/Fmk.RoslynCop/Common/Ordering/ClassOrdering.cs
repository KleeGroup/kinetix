using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fmk.RoslynCop.Common.Ordering {

    /// <summary>
    /// Classe regroupant les fonctions partagées entre l'analyseur et le correcteur pour ClassOrdering.
    /// </summary>
    public static class ClassOrdering {

        /// <summary>
        /// Ordonne les membres d'une déclaration de type.
        /// </summary>
        /// <param name="éléments">Les éléments.</param>
        /// <param name="modèleSémantique">Le modèle sémantique.</param>
        /// <returns>Les éléments ordonnés.</returns>
        public static IEnumerable<MemberDeclarationSyntax> OrdonnerMembres(IEnumerable<MemberDeclarationSyntax> éléments, SemanticModel modèleSémantique) {
            var comparateurAccessibilite = new AccessibilityComparer();
            var comparateurStatiqueLectureSeule = new StaticReadonlyComparer();

            var constantes = éléments.OfType<FieldDeclarationSyntax>()
                .Where(élément => élément.Modifiers.Any(jeton => jeton.Kind() == SyntaxKind.ConstKeyword))
                .TrierParNom()
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var champs = éléments.OfType<FieldDeclarationSyntax>()
                .Where(élément => !élément.Modifiers.Any(jeton => jeton.Kind() == SyntaxKind.ConstKeyword))
                .TrierParNom()
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var constructeurs = éléments.OfType<ConstructorDeclarationSyntax>()
                .TrierParNombreParametres()
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var destructeurs = éléments.OfType<DestructorDeclarationSyntax>()
                .TrierParNombreParametres()
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var délégués = éléments.OfType<DelegateDeclarationSyntax>()
                .OrderBy(élément => élément.Identifier.ToString(), StringComparer.Ordinal)
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var évènements = éléments.OfType<EventFieldDeclarationSyntax>()
                .TrierParNom()
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var énumérations = éléments.OfType<EnumDeclarationSyntax>()
                .TrierParNom()
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var interfaces = éléments.OfType<InterfaceDeclarationSyntax>()
                .TrierParNom()
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var propriétés = éléments.OfType<PropertyDeclarationSyntax>()
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var indexeurs = éléments.OfType<IndexerDeclarationSyntax>()
                .OrderBy(élément => élément.Type.ToString(), StringComparer.Ordinal)
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var méthodes = éléments.OfType<MethodDeclarationSyntax>()
                .TrierParNombreParametres()
                .OrderBy(élément => (élément as MethodDeclarationSyntax).Identifier.ToString(), StringComparer.Ordinal)
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var structs = éléments.OfType<StructDeclarationSyntax>()
                .TrierParNom()
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            var classes = éléments.OfType<ClassDeclarationSyntax>()
                .TrierParNom()
                .TrierParSymbole(modèleSémantique, comparateurStatiqueLectureSeule)
                .TrierParSymbole(modèleSémantique, comparateurAccessibilite);

            return Concaténer(
                constantes,
                champs,
                constructeurs,
                destructeurs,
                délégués,
                évènements,
                énumérations,
                interfaces,
                propriétés,
                indexeurs,
                méthodes,
                structs,
                classes);
        }

        private static IEnumerable<T> Concaténer<T>(params IEnumerable<T>[] listes) => listes.SelectMany(x => x);

        private static IEnumerable<BaseFieldDeclarationSyntax> TrierParNom(this IEnumerable<BaseFieldDeclarationSyntax> éléments)
            => éléments.OrderBy(élément => élément.Declaration.Variables.First().Identifier.ToString(), StringComparer.Ordinal);

        private static IEnumerable<BaseTypeDeclarationSyntax> TrierParNom(this IEnumerable<BaseTypeDeclarationSyntax> éléments)
            => éléments.OrderBy(élément => élément.Identifier.ToString(), StringComparer.Ordinal);

        private static IEnumerable<BaseMethodDeclarationSyntax> TrierParNombreParametres(this IEnumerable<BaseMethodDeclarationSyntax> éléments)
            => éléments.OrderBy(élément => élément.ParameterList.ChildNodes().Count());

        private static IEnumerable<BaseFieldDeclarationSyntax> TrierParSymbole(this IEnumerable<BaseFieldDeclarationSyntax> éléments, SemanticModel modèleSémantique, IComparer<ISymbol> comparateur)
            => éléments.OrderBy(élément => modèleSémantique.GetDeclaredSymbol(élément.Declaration.Variables.First()), comparateur);

        private static IEnumerable<MemberDeclarationSyntax> TrierParSymbole(this IEnumerable<MemberDeclarationSyntax> éléments, SemanticModel modèleSémantique, IComparer<ISymbol> comparateur)
            => éléments.OrderBy(élément => modèleSémantique.GetDeclaredSymbol(élément), comparateur);
    }
}
