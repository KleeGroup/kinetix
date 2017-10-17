using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Kinetix.Versionning {

    /// <summary>
    /// Versionning.
    /// </summary>
    public class Program {

        /// <summary>
        /// Point d'entrée.
        /// </summary>
        /// <param name="args">
        /// Premier argument : MainDirectory.
        /// Deuxième argument : PackageJsonPath.
        /// </param>
        public static void Main(string[] args) {
            Console.WriteLine("Numéro de version ?");
            var numeroVersion = Console.ReadLine();
            var files = Directory.GetFiles(args[0], "AssemblyInfo.cs", SearchOption.AllDirectories);
            var versionRegex = new Regex(@"Version\(""[\d\.]+""\)");
            foreach (var file in files) {
                File.WriteAllText(file, versionRegex.Replace(File.ReadAllText(file, Encoding.UTF8), $@"Version(""{numeroVersion}"")"), Encoding.UTF8);
            }

            // encodage en UTF8 sans BOM pour webpack.
            var utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var packageJson = args[1];
            var pjNumero = numeroVersion.Length > 5 ? numeroVersion.Remove(5) : numeroVersion;
            File.WriteAllText(packageJson, Regex.Replace(File.ReadAllText(packageJson, utf8WithoutBom), @"""version"": ""(.+)""", $@"""version"": ""{pjNumero}"""), utf8WithoutBom);
        }
    }
}
