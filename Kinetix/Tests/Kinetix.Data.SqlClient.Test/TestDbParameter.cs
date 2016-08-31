using System.Data;
using System.Data.Common;

namespace Kinetix.Data.SqlClient {
    /// <summary>
    /// Paramètre d'appel d'une commande de test.
    /// </summary>
    public sealed class TestDbParameter : DbParameter {

        /// <summary>
        /// Crée un nouveau paramétre.
        /// </summary>
        internal TestDbParameter() {
        }

        /// <summary>
        /// Obtient ou définit le type de données.
        /// </summary>
        public override DbType DbType {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la direction du paramètre.
        /// </summary>
        public override ParameterDirection Direction {
            get;
            set;
        }

        /// <summary>
        /// Indique si le paramètre est nullable.
        /// </summary>
        public override bool IsNullable {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit le nom du paramétre.
        /// </summary>
        public override string ParameterName {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la précision.
        /// </summary>
        public byte Precision {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la précision.
        /// </summary>
        public byte Scale {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la taille.
        /// </summary>
        public override int Size {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la colonne source.
        /// </summary>
        public override string SourceColumn {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la version de la colonne source.
        /// </summary>
        public override DataRowVersion SourceVersion {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la valeur du paramètre.
        /// La valeur peut être nulle.
        /// </summary>
        public override object Value {
            get;
            set;
        }

        /// <summary>
        /// Mapping null.
        /// </summary>
        public override bool SourceColumnNullMapping {
            get;
            set;
        }

        /// <summary>
        /// Reset le type.
        /// </summary>
        public override void ResetDbType() {
            return;
        }
    }
}
