using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Kinetix.Reporting {

    /// <summary>
    /// Argument d'un export.
    /// </summary>
    public sealed class ExportEventArgs : CancelEventArgs {

        /// <summary>
        /// Constructeur.
        /// </summary>
        public ExportEventArgs() {
            this.CriteriaProperties = new List<ExportPropertyDefinition>();
            this.ListProperties = new List<ExportPropertyDefinition>();
        }

        /// <summary>
        /// Nom du fichier d'export.
        /// </summary>
        public string FileName {
            get;
            set;
        }

        /// <summary>
        /// Texte de l'entête.
        /// </summary>
        public string HeaderText {
            get;
            set;
        }

        /// <summary>
        /// Texte du bas de page.
        /// </summary>
        public string FooterText {
            get;
            set;
        }

        /// <summary>
        /// Liste des fields exportés depuis la liste personnalisée de l'export.
        /// </summary>
        public string[] ExportSpecifiedFields {
            get;
            set;
        }

        /// <summary>
        /// Liste des champs de critères.
        /// </summary>
        public ICollection<ExportPropertyDefinition> CriteriaProperties {
            get;
            private set;
        }

        /// <summary>
        /// Liste des champs de données formattées.
        /// </summary>
        public ICollection<ExportPropertyDefinition> ListProperties {
            get;
            private set;
        }

        /// <summary>
        /// Obtient ou définit la source de données critères.
        /// </summary>
        public object CriteriaDataSource {
            get;
            set;
        }

        /// <summary>
        /// Obtient ou définit la source de données de la liste.
        /// </summary>
        public object ListDataSource {
            get;
            set;
        }

        /// <summary>
        /// Obtient une définition de propriété par son PropertyPath.
        /// </summary>
        /// <param name="propertyPath">PropertyPath de la prorpiété.</param>
        /// <returns>Définition de la propriété.</returns>
        public ExportPropertyDefinition this[string propertyPath] {
            get {
                return this.ListProperties.FirstOrDefault(x => x.PropertyPath == propertyPath);
            }
        }

        /// <summary>
        /// Ajout des propriétés de liste à partir d'une liste de PropertyPath.
        /// </summary>
        /// <param name="propertyList">Liste de property path.</param>
        public void AddListProperties(params string[] propertyList) {
            if (propertyList == null) {
                throw new ArgumentNullException("propertyList");
            }

            foreach (string propertyPath in propertyList) {
                this.ListProperties.Add(new ExportPropertyDefinition {
                    PropertyPath = propertyPath
                });
            }
        }
    }
}
