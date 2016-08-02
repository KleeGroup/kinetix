using System.Text;

namespace Kinetix.ClassGenerator.Model {

    /// <summary>
    /// Cette classe représente le contrat d'une classe.
    /// </summary>
    public sealed class ModelDataContract {

        private const string Tab = "\t\t";

        /// <summary>
        /// Retourne le caractère persistent du contrat.
        /// </summary>
        public bool IsPersistent {
            get;
            set;
        }

        /// <summary>
        /// Retourne le nom de l'objet persitent.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Retourne le namespace.
        /// </summary>
        public string Namespace {
            get;
            set;
        }

        /// <summary>
        /// Retourne la chaine de debug.
        /// </summary>
        public string DebugString {
            get {
                StringBuilder sb = new StringBuilder();
                sb.Append(Tab);
                sb.Append("IsPersistent : ");
                sb.AppendLine(IsPersistent.ToString());
                sb.Append(Tab);
                sb.Append("Name : ");
                sb.AppendLine(Name);
                sb.Append(Tab);
                sb.Append("Namespace : ");
                sb.AppendLine(Namespace);
                return sb.ToString();
            }
        }
    }
}
