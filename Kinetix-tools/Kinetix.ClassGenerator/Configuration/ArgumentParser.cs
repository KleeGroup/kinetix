using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;

namespace Kinetix.ClassGenerator.Configuration {

    /// <summary>
    /// Parseur des arguments du programme.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Conceptuellement non statique")]
    public class ArgumentParser {

        /// <summary>
        /// Composant de chargement des paramètres du programme.
        /// </summary>
        private readonly ModelConfigurationLoader _modelConfigurationLoader = new ModelConfigurationLoader();

        /// <summary>
        /// Parse les arguments d'entrée du programme.
        /// </summary>
        /// <param name="args">Arguments de la commande.</param>
        public void Parse(string[] args) {

            CheckArguments(args);
            LoadConfiguration(args);
            InitDefaultParameters();
            ParseCommandLine(args);
        }

        /// <summary>
        /// Initialise les paramètres par défaut (paramètres non-obligatoires).
        /// </summary>
        private static void InitDefaultParameters() {
            GeneratorParameters.Generate = false;
            GeneratorParameters.GenerateSql = false;
            GeneratorParameters.Pause = false;
            GeneratorParameters.OverWrite = false;
            GeneratorParameters.IsPostSharpDisabled = false;
            GeneratorParameters.IsNotifyPropertyChangeEnabled = false;
            GeneratorParameters.GenerateJavascript = false;
            GeneratorParameters.GenerateJavascriptRedirect = false;
            GeneratorParameters.IsProjetUesl = false;
        }

        /// <summary>
        /// Vérifie que les paramètres de lancement sont bien conformes à ce qui est attendu (voir PrintUsage).
        /// </summary>
        /// <param name="args">Arguments.</param>
        private static void CheckArguments(string[] args) {
            if (args == null || args.Length == 0) {
                PrintUsage();
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Parse la ligne de commande.
        /// </summary>
        /// <param name="args">Arguments du programme.</param>
        private static void ParseCommandLine(string[] args) {
            bool isPostSharpDisabled = false;
            bool isNotifyPropertyChangeEnabled = false;
            for (int i = 0; i < args.Length; ++i) {
                if (args[i].StartsWith("-", StringComparison.Ordinal)) {
                    switch (args[i]) {
                        case "-G":
                            GeneratorParameters.Generate = true;
                            break;
                        case "-S":
                            GeneratorParameters.GenerateSql = true;
                            break;
                        case "-P":
                            GeneratorParameters.Pause = true;
                            break;
                        case "-F":
                            GeneratorParameters.OverWrite = true;
                            break;
                        case "-A":
                            GeneratorParameters.IsPostSharpDisabled = true;
                            break;
                        case "-N":
                            GeneratorParameters.IsNotifyPropertyChangeEnabled = true;
                            break;
                        case "-J":
                            GeneratorParameters.GenerateJavascript = true;
                            break;
                        case "-R":
                            GeneratorParameters.GenerateJavascriptRedirect = true;
                            break;
                        case "-UESL":
                            GeneratorParameters.IsProjetUesl = true;
                            break;
                        case "-EF":
                            GeneratorParameters.IsEntityFrameworkUsed = true;
                            break;
                        default:
                            PrintUsage(args[i]);
                            Environment.Exit(-1);
                            break;
                    }
                }
            }

            if (isNotifyPropertyChangeEnabled && !isPostSharpDisabled) {
                PrintUsage();
                Environment.Exit(-1);
            }
        }

        /// <summary>
        /// Ecrit dans la console le mode d'emploi.
        /// </summary>
        private static void PrintUsage() {
            PrintUsage(null);
        }

        /// <summary>
        /// Ecrit dans la console le mode d'emploi en indiquant quelle commande passée n'était pas reconnue.
        /// </summary>
        /// <param name="opt">Option passée.</param>
        private static void PrintUsage(string opt) {
            if (!string.IsNullOrEmpty(opt)) {
                Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "Option inconnue : {0}", opt));
                Console.Out.WriteLine();
            }

            Console.Out.WriteLine("usage : ClassGenerator <configuration file> [options]");
            Console.Out.WriteLine("Options :");
            Console.Out.WriteLine(" -G : génére les classes");
            Console.Out.WriteLine(" -S : génére les scripts SQL");
            Console.Out.WriteLine(" -F : réécrit par dessus les fichiers existants");
            Console.Out.WriteLine(" -P : marque une pause à la fin du traitement");
            Console.Out.WriteLine(" -A : PostSharp désactivé");
            Console.Out.WriteLine(" -N : si -A et que l'on souhaite gérer la notification de modification de champ");
            Console.Out.WriteLine(" -J : génère les scripts Javascript");
            Console.Out.WriteLine(" -R : génère les redirect pour la composition Javascript");
            Console.Out.WriteLine(" -UESL : pour gérer les conventions de nommage de l'UESL");
            Console.Out.WriteLine(" -EF : pour générer les attributs utiles à l'utilisation d'Entity Framework");
        }

        /// <summary>
        /// Charge la configuration depuis le fichier de configuration.
        /// </summary>
        /// <param name="args">Arguments de la ligne de commande.</param>
        private void LoadConfiguration(string[] args) {
            string xmlPath = args[0];
            if (!File.Exists(xmlPath)) {
                throw new FileNotFoundException("Le fichier de configuration n'existe pas");
            }

            _modelConfigurationLoader.LoadModelConfiguration(xmlPath);
        }
    }
}
