using System.Collections.Generic;
using System.Text;

namespace Kinetix.ClassGenerator.Model {

    /// <summary>
    /// Cette classe représente le domaine d'une propriété.
    /// </summary>
    public sealed class ModelDomain : IModelObject, IPersistenceData {

        private static IList<string> _translatableDomainCodeList = new List<string> {
            "DO_LIBELLE_COURT",
            "DO_LIBELLE",
            "DO_LIBELLE_LONG"
        };

        /// <summary>
        /// Nom du modèle définissant l'objet.
        /// </summary>
        public string ModelFile {
            get;
            set;
        }

        /// <summary>
        /// Retourne le nom du domaine.
        /// </summary>
        public string Name {
            get;
            set;
        }

        /// <summary>
        /// Retourne le code du domaine.
        /// </summary>
        public string Code {
            get;
            set;
        }

        /// <summary>
        /// Retourne le type de donnée du domaine.
        /// </summary>
        public string DataType {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le type persistant.
        /// </summary>
        public string PersistentDataType {
            get;
            set;
        }

        /// <summary>
        /// Retourne la longueur persistente du domaine.
        /// </summary>
        public int? PersistentLength {
            get;
            set;
        }

        /// <summary>
        /// Retourne la précision du dommaine.
        /// </summary>
        public int? PersistentPrecision {
            get;
            set;
        }

        /// <summary>
        /// Le modèle du domaine.
        /// </summary>
        public ModelRoot Model {
            get;
            set;
        }

        /// <summary>
        /// Indique si les champs portant ce domaine sont traduisible.
        /// </summary>
        public bool IsTranslatable {
            get {
                return _translatableDomainCodeList.Contains(this.Code);
            }
        }

        /// <summary>
        /// Retourne la chaine de Debug.
        /// </summary>
        public string DebugString {
            get {
                StringBuilder sb = new StringBuilder();
                sb.Append("------------------------ Domaine ");
                sb.Append(Name);
                sb.AppendLine(" ------------------------");
                sb.Append("Name : ");
                sb.AppendLine(Name);
                sb.Append("Code : ");
                sb.AppendLine(Code);
                sb.Append("DataType : ");
                sb.AppendLine(DataType);
                sb.Append("PersistentLength : ");
                sb.AppendLine(PersistentLength.ToString());
                return sb.ToString();
            }
        }
    }
}
