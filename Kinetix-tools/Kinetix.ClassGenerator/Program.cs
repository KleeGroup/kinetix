using System;
using System.Diagnostics.CodeAnalysis;
using Kinetix.ClassGenerator.Configuration;
using Kinetix.ClassGenerator.Main;

namespace Kinetix.ClassGenerator {

    /// <summary>
    /// Point d'entrée du générateur de classes.
    /// </summary>
    public static class Program {

        /// <summary>
        /// Composant de parsing des arguments du programme.
        /// </summary>
        private static readonly ArgumentParser ArgumentParser = new ArgumentParser();

        /// <summary>
        /// Composant contenant le générateur principal.
        /// </summary>
        private static readonly MainGenerator MainGenerator = new MainGenerator();

        /// <summary>
        /// Lance la construction du modèle puis la génération des classes.
        /// </summary>
        /// <param name="args">
        /// Paramètres de ligne de commande.
        /// </param>
        /// <remarks>
        /// Génération des classes et du SQL, options -G et -S.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Interception de toutes les exceptions pour écriture sur flux de sortie d'erreur.")]
        [STAThreadAttribute]
        public static void Main(string[] args) {
            try {

                Console.WriteLine("******************************************************");
                Console.WriteLine("*                Kinetix.ClassGenerator                  *");
                Console.WriteLine("******************************************************");
                Console.WriteLine();

                // Lecture des paramètres d'entrée.
                ArgumentParser.Parse(args);

                // Exécution de la génération
                MainGenerator.Generate();

                Console.WriteLine();
                Console.WriteLine("Fin de la génération");
            } catch (Exception ex) {
                Console.Error.WriteLine("Une erreur est arrivée durant la génération des classes : ");
                Console.Error.WriteLine(ex.ToString());

                Console.ReadKey();
                Environment.Exit(-1);
            }
        }
    }
}
