using System;
using System.Configuration;
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
        public static void Main() {
            Console.WriteLine("Numéro de version ?");
            var numeroVersion = Console.ReadLine();
            var files = Directory.GetFiles(ConfigurationManager.AppSettings["MainDirectory"], "AssemblyInfo.cs", SearchOption.AllDirectories);
            var versionRegex = new Regex(@"Version\(""[\d\.]+""\)");
            foreach (var file in files) {
                File.WriteAllText(file, versionRegex.Replace(File.ReadAllText(file, Encoding.UTF8), $@"Version(""{numeroVersion}"")"), Encoding.UTF8);
            }

            var packageJson = ConfigurationManager.AppSettings["PackageJsonPath"];
            var pjNumero = numeroVersion.Length > 5 ? numeroVersion.Remove(5) : numeroVersion;
            File.WriteAllText(packageJson, Regex.Replace(File.ReadAllText(packageJson, Encoding.UTF8), @"""version"": ""(.+)""", $@"""version"": ""{pjNumero}"""), Encoding.UTF8);
        }
    }
}
