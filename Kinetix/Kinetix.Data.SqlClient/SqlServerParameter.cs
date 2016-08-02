using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Kinetix.Data.SqlClient {
    /// <summary>
    /// Paramètre d'appel d'une commande Sql Server.
    /// </summary>
    public sealed class SqlServerParameter : IDbDataParameter {
        private readonly IDbDataParameter _innerParameter;

        /// <summary>
        /// Crée un nouveau paramétre.
        /// </summary>
        /// <param name="parameter">Paramétre interne.</param>
        internal SqlServerParameter(IDbDataParameter parameter) {
            _innerParameter = parameter;
        }

        /// <summary>
        /// Obtient ou définit le type SQL Server dédié.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Justification = "Respect de l'API ADO.NET.")]
        public SqlDbType SqlDbType {
            get {
                return ((SqlParameter)_innerParameter).SqlDbType;
            }

            set {
                ((SqlParameter)_innerParameter).SqlDbType = value;
            }
        }

        /// <summary>
        /// Obtient ou définit le nom du type dédié.
        /// </summary>
        public string TypeName {
            get {
                return ((SqlParameter)_innerParameter).TypeName;
            }

            set {
                ((SqlParameter)_innerParameter).TypeName = value;
            }
        }

        /// <summary>
        /// Obtient ou définit le nom du type dédié.
        /// </summary>
        public string UdtTypeName {
            get {
                return ((SqlParameter)_innerParameter).UdtTypeName;
            }

            set {
                ((SqlParameter)_innerParameter).UdtTypeName = value;
            }
        }

        /// <summary>
        /// Obtient ou définit le type de données.
        /// </summary>
        public DbType DbType {
            get {
                return _innerParameter.DbType;
            }

            set {
                _innerParameter.DbType = value;
            }
        }

        /// <summary>
        /// Obtient ou définit la direction du paramètre.
        /// </summary>
        public ParameterDirection Direction {
            get {
                return _innerParameter.Direction;
            }

            set {
                _innerParameter.Direction = value;
            }
        }

        /// <summary>
        /// Obtient ou définit le nom du paramétre.
        /// </summary>
        public string ParameterName {
            get {
                return _innerParameter.ParameterName;
            }

            set {
                _innerParameter.ParameterName = value;
            }
        }

        /// <summary>
        /// Obtient ou définit la précision.
        /// </summary>
        public byte Precision {
            get {
                return _innerParameter.Precision;
            }

            set {
                _innerParameter.Precision = value;
            }
        }

        /// <summary>
        /// Obtient ou définit la précision.
        /// </summary>
        public byte Scale {
            get {
                return _innerParameter.Scale;
            }

            set {
                _innerParameter.Scale = value;
            }
        }

        /// <summary>
        /// Obtient ou définit la taille.
        /// </summary>
        public int Size {
            get {
                return _innerParameter.Size;
            }

            set {
                _innerParameter.Size = value;
            }
        }

        /// <summary>
        /// Obtient ou définit la valeur du paramètre.
        /// La valeur peut être nulle.
        /// </summary>
        public object Value {
            get {
                object val = _innerParameter.Value;
                return DBNull.Value.Equals(val) ? null : val;
            }

            set {
                _innerParameter.Value = value ?? DBNull.Value;
            }
        }

        /// <summary>
        /// Obtient ou définit la colonne source.
        /// </summary>
        string IDataParameter.SourceColumn {
            get {
                return _innerParameter.SourceColumn;
            }

            set {
                _innerParameter.SourceColumn = value;
            }
        }

        /// <summary>
        /// Obtient ou définit la version de la colonne source.
        /// </summary>
        DataRowVersion IDataParameter.SourceVersion {
            get {
                return _innerParameter.SourceVersion;
            }

            set {
                _innerParameter.SourceVersion = value;
            }
        }

        /// <summary>
        /// Indique si le paramètre est nullable.
        /// </summary>
        bool IDataParameter.IsNullable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Retourne le paramètre interne.
        /// </summary>
        internal IDbDataParameter InnerParameter {
            get {
                return _innerParameter;
            }
        }
    }
}
