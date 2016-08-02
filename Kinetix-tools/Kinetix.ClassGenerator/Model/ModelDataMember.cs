using System.Text;

namespace Kinetix.ClassGenerator.Model {

    /// <summary>
    /// Classe présentant le DataMember.
    /// </summary>
    public sealed class ModelDataMember : IPersistenceData {

        private const string Tab = "\t\t\t\t";

        /// <summary>
        /// Retourne le nom du champ persistant.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Retourne si le champ est requis.
        /// </summary>
        public bool IsRequired {
            get;
            set;
        }

        /// <summary>
        /// Ordre de déclaration de la colonne.
        /// </summary>
        public int? Order {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le type persistant (dans le cas d'une propriété non applicative sans domaine).
        /// </summary>
        public string PersistentDataType {
            get;
            set;
        }

        /// <summary>
        /// Retourne la longueur persistente du domaine (dans le cas d'une propriété non applicative sans domaine).
        /// </summary>
        public int? PersistentLength {
            get;
            set;
        }

        /// <summary>
        /// Retourne la précision du dommaine (dans le cas d'une propriété non applicative sans domaine).
        /// </summary>
        public int? PersistentPrecision {
            get;
            set;
        }

        /// <summary>
        /// Retourne la chaine de Debug.
        /// </summary>
        public string DebugString {
            get {
                StringBuilder sb = new StringBuilder();
                sb.Append(Tab);
                sb.Append("Name : ");
                sb.AppendLine(Name);
                return sb.ToString();
            }
        }
    }
}
