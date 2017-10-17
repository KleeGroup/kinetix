using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kinetix.ClassGenerator.Writer {

    /// <summary>
    /// Classe de base pour l'écriture des fichiers générés.
    /// </summary>
    public class FileWriter : TextWriter {

        /// <summary>
        /// Nombre de lignes d'en-tête à ignorer dans le calcul de checksum.
        /// </summary>
        private const int LinesInHeader = 4;

        private readonly StringBuilder _sb;
        private readonly string _fileName;

        /// <summary>
        /// Crée une nouvelle instance.
        /// </summary>
        /// <param name="fileName">Nom du fichier à écrire.</param>
        public FileWriter(string fileName)
            : base(CultureInfo.InvariantCulture) {
            if (fileName == null) {
                throw new ArgumentNullException("fileName");
            }

            _fileName = fileName;
            _sb = new StringBuilder();
        }

        /// <summary>
        /// Retourne l'encodage à utiliser.
        /// </summary>
        public override Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// Retourne le numéro de la ligne qui contient la version.
        /// </summary>
        protected virtual int VersionLine => 3;

        /// <summary>
        /// Active la lecture et l'écriture d'un entête avec un hash du fichier.
        /// </summary>
        protected virtual bool EnableHeader => true;

        /// <summary>
        /// Renvoie le token de début de ligne de commentaire dans le langage du fichier.
        /// </summary>
        /// <returns>Toket de début de ligne de commentaire.</returns>
        protected virtual string StartCommentToken => "////";

        /// <summary>
        /// Ecrit un caractère dans le stream.
        /// </summary>
        /// <param name="value">Caractère.</param>
        public override void Write(char value) {
            _sb.Append(value);
        }

        /// <summary>
        /// Ecrit un string dans le stream.
        /// </summary>
        /// <param name="value">Chaîne de caractère.</param>
        public override void Write(string value) {
            _sb.Append(value);
        }

        /// <summary>
        /// Libère les ressources.
        /// </summary>
        /// <param name="disposing">True.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Le fichier d'origine n'est pas nécessairement présent.")]
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            string currentContent = null;
            string currentVersion = null;
            bool fileExists = File.Exists(_fileName);
            if (fileExists) {
                using (StreamReader reader = new StreamReader(_fileName, this.Encoding)) {
                    if (this.EnableHeader) {
                        for (int i = 0; i < LinesInHeader; i++) {
                            string line = reader.ReadLine();
                            if (i == this.VersionLine) {
                                currentVersion = line;
                            }
                        }
                    }

                    currentContent = reader.ReadToEnd();
                }
            }

            string newContent = _sb.ToString();
            string hash = Sha1Hash(newContent);
            if (newContent.Equals(currentContent)) {
                return;
            }

            /* Création du répertoire si inexistant. */
            var dir = new FileInfo(_fileName).DirectoryName;
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            using (StreamWriter sw = new StreamWriter(_fileName, false, this.Encoding)) {
                if (this.EnableHeader) {
                    sw.WriteLine(this.StartCommentToken);
                    sw.WriteLine(this.StartCommentToken + " ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT (" + hash + ") !");
                    sw.WriteLine(this.StartCommentToken);
                    sw.WriteLine();
                }

                sw.Write(newContent);
            }

            if (!fileExists) {
                this.FinishFile(_fileName);
            }
        }

        /// <summary>
        /// Appelé après la création d'un nouveau fichier.
        /// </summary>
        /// <param name="fileName">Nom du fichier.</param>
        protected virtual void FinishFile(string fileName) {
        }

        /// <summary>
        /// Calcul une empreinte SHA1 du contenu du fichier.
        /// </summary>
        /// <param name="content">Contenu.</param>
        /// <returns>Hash.</returns>
        private static string Sha1Hash(string content) {
            ASCIIEncoding encoding = new ASCIIEncoding();
            SHA1 sha = new SHA1CryptoServiceProvider();
            return BitConverter.ToString(sha.ComputeHash(encoding.GetBytes(content))).Replace("-", string.Empty);
        }
    }
}
