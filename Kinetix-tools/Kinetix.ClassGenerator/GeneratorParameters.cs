using System.Collections.Generic;

namespace Kinetix.ClassGenerator {

    /// <summary>
    /// Paramètres de lancement du générateur de classes.
    /// </summary>
    public static class GeneratorParameters {

        /// <summary>
        /// Constructeur.
        /// </summary>
        static GeneratorParameters() {
            ModelFiles = new List<string>();
        }

        /// <summary>
        /// URL de la collection TFS du projet.
        /// </summary>
        public static string TfsCollectionUrl {
            get;
            set;
        }

        /// <summary>
        /// Liste des fichiers de modélisation.
        /// </summary>
        public static ICollection<string> ModelFiles {
            get;
            private set;
        }

        /// <summary>
        /// Obtient ou définit si le générateur doit produire le SQL.
        /// </summary>
        public static bool GenerateSql {
            get;
            set;
        }

        /// <summary>
        /// True si oracle DB.
        /// </summary>
        public static bool IsOracle {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le type de fichier de modèle.
        /// </summary>
        public static string ModelType {
            get;
            set;
        }

        /// <summary>
        /// True s'il s'agit d'une SPA.
        /// </summary>
        public static bool IsSpa {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit si le générateur doit produit le javascript.
        /// </summary>
        public static bool GenerateJavascript {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit si le générateur doit écrire les redirect pour la composition (Focus V3).
        /// </summary>
        public static bool GenerateJavascriptRedirect {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit si le générateur doit respecter la convention de nommage de l'UESL.
        /// </summary>
        public static bool IsProjetUesl {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit si le générateur doit générer les attributs de décoration pour Entity Framework.
        /// </summary>
        public static bool IsEntityFrameworkUsed {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le nom de la table où stocker l'historique de passage des scripts.
        /// </summary>
        public static string LogScriptTableName {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le nom du champ où stocker le nom des scripts exécutés.
        /// </summary>
        public static string LogScriptVersionField {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le nom du champ où stocker la date d'exécution des scripts.
        /// </summary>
        public static string LogScriptDateField {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit si le générateur doit être mis en pause en sortie.
        /// </summary>
        public static bool Pause {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit si la génération de code doit être effectuée.
        /// </summary>
        public static bool Generate {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit si le générateur doit écraser les fichiers existants.
        /// </summary>
        public static bool OverWrite {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le répertoire de génération.
        /// </summary>
        public static string OutputDirectory {
            get;
            set;
        }

        /// <summary>
        /// Repository de contrôle de source.
        /// </summary>
        public static string SourceRepository {
            get;
            set;
        }

        /// <summary>
        /// Nom de l'assembly contenant des implémentations de AbstractDomainFactory.
        /// </summary>
        public static string DomainFactoryAssembly {
            get;
            set;
        }

        /// <summary>
        /// Retourne true si PostSharp est activé, false sinon.
        /// </summary>
        public static bool IsPostSharpDisabled {
            get;
            set;
        }

        /// <summary>
        /// Retourne true si la notification de modifiaction de rpopriété est activée (pour PostSharp désactivé).
        /// </summary>
        public static bool IsNotifyPropertyChangeEnabled {
            get;
            set;
        }

        /// <summary>
        /// Retourne ou définit le nom du fichier vortex pour CruiseControl.
        /// </summary>
        public static string VortexFile {
            get;
            set;
        }

        /// <summary>
        /// Retourn ou définit l'emplacement du fichier de création de base (SQL).
        /// </summary>
        public static string CrebasFile {
            get;
            set;
        }

        /// <summary>
        /// Namespace de base de l'application.
        /// </summary>
        public static string RootNamespace {
            get;
            set;
        }

        /// <summary>
        /// Racine de la SPA.
        /// </summary>
        public static string SpaAppPath {
            get;
            set;
        }

        /// <summary>
        /// Renvoie ou définit l'emplacement du dossier contenant les modèles de données a générer.
        /// </summary>
        public static string JsModelRoot {
            get;
            set;
        }

        /// <summary>
        /// Projet Focus v4.
        /// </summary>
        public static bool IsFocus4 {
            get;
            set;
        }

        /// <summary>
        /// Renvoie ou définit l'emplacement du dossier contenant les ressources à générer.
        /// </summary>
        public static string JsResourceRoot {
            get;
            set;
        }

        /// <summary>
        /// Retourne ou définit l'emplacement du fichier de création des index uniques (SQL).
        /// </summary>
        public static string UKFile {
            get;
            set;
        }

        /// <summary>
        /// Retourne ou définit l'emplacement du fichier de création des clés étrangères (SQL).
        /// </summary>
        public static string IndexFKFile {
            get;
            set;
        }

        /// <summary>
        /// Retourne ou définit l'emplacement du ficheir de création des types (SQL).
        /// </summary>
        public static string TypeFileName {
            get;
            set;
        }

        /// <summary>
        /// Retourne ou définit l'emplacement du fichier de création des synonymes (SQL).
        /// </summary>
        public static string SynonymFile {
            get;
            set;
        }

        /// <summary>
        /// Retourne ou définit l'emplacement du script d'insertion des données des listes statiques (SQL).
        /// </summary>
        public static string StaticListFile {
            get;
            set;
        }

        /// <summary>
        /// Retourne ou définit l'emplacement du script d'insertion des données des listes administrables (SQL).
        /// </summary>
        public static string ReferenceListFile {
            get;
            set;
        }

        /// <summary>
        /// Nom du fichier de configuration des colonnes avec default value.
        /// </summary>
        public static string DefaultValuesFile {
            get;
            set;
        }

        /// <summary>
        /// Nom du fichier de configuration tables à ne pas générer.
        /// </summary>
        public static string NoTableFile {
            get;
            set;
        }

        /// <summary>
        /// Nom du fichier de configuration de l'historique de création des colonnes.
        /// </summary>
        public static string HistoriqueCreationFile {
            get;
            set;
        }

        /// <summary>
        /// Nom du fichier sql d'insertion des libellés de liste statique.
        /// </summary>
        public static string StaticListLabelFile {
            get;
            set;
        }

        /// <summary>
        /// Nom du fichier sql d'insertion des libellés de liste de ref.
        /// </summary>
        public static string ReferenceListLabelFile {
            get;
            set;
        }

        /// <summary>
        /// Chemin du fichier sqlproj du projet SSDT.
        /// </summary>
        public static string SsdtProjFileName {
            get;
            set;
        }

        /// <summary>
        /// Dossier du projet SSDT pour les scripts de déclaration de table.
        /// </summary>
        public static string SsdtTableScriptFolder {
            get;
            set;
        }

        /// <summary>
        /// Dossier du projet SSDT pour les scripts de déclaration de type table.
        /// </summary>
        public static string SsdtTableTypeScriptFolder {
            get;
            set;
        }

        /// <summary>
        /// Dossier du projet SSDT pour les scripts d'initialisation des listes de références administrables.
        /// </summary>
        public static string SsdtInitReferenceListScriptFolder {
            get;
            set;
        }

        /// <summary>
        /// Dossier du projet SSDT pour les scripts d'initialisation des listes statiques.
        /// </summary>
        public static string SsdtInitStaticListScriptFolder {
            get;
            set;
        }

        /// <summary>
        /// Fichier du projet SSDT référençant les scripts d'initialisation de références administrables.
        /// </summary>
        public static string SsdtInitReferenceListMainScriptName {
            get;
            set;
        }

        /// <summary>
        /// Fichier du projet SSDT référençant les scripts d'initialisation des listes statiques.
        /// </summary>
        public static string SsdtInitStaticListMainScriptName {
            get;
            set;
        }
    }
}