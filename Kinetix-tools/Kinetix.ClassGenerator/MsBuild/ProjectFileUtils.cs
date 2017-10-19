using System.IO;

namespace Kinetix.ClassGenerator.MsBuild {

    /// <summary>
    /// Classe utilitaire pour gérer les projets MSBUILD.
    /// </summary>
    public static class ProjectFileUtils {

        /// <summary>
        /// Obtient le chemin d'un fichier relatif à un fichier de projet MSBUILD.
        /// </summary>
        /// <param name="fileName">Chemin du fichier.</param>
        /// <param name="projFileName">Chemin du fichier projet MSBUILD.</param>
        /// <returns>Chemin relatif au projet.</returns>
        public static string GetProjectRelativeFileName(string fileName, string projFileName) {
            var projectDirectory = new FileInfo(projFileName).Directory.FullName + Path.DirectorySeparatorChar;
            var absoluteFilePath = new FileInfo(fileName).FullName;
            var localFileName = absoluteFilePath.Substring(projectDirectory.Length);
            return localFileName;
        }
    }
}
