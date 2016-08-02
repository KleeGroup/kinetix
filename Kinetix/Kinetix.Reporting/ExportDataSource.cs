using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kinetix.Reporting {

    /// <summary>
    /// Données d'un export.
    /// </summary>
    public sealed class ExportDataSource {

        private readonly IList<ExportSheet> _sheets = new List<ExportSheet>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        public ExportDataSource() {
        }

        /// <summary>
        /// Constructeur via IExportableControl.
        /// </summary>
        /// <param name="args">Données de l'export.</param>
        public ExportDataSource(ExportEventArgs args) {
            if (args == null) {
                throw new ArgumentNullException("args");
            }

            if (args.CriteriaProperties.Count != 0) {
                this.AddExportSheet("Critère de recherche", args.CriteriaDataSource, args.CriteriaProperties);
            }

            this.AddExportSheet("Liste des données", args.ListDataSource, args.ListProperties);
        }

        /// <summary>
        /// Retourne une collection en lecture seule présentant les données de l'export.
        /// </summary>
        internal ICollection<ExportSheet> Sheets {
            get {
                return new ReadOnlyCollection<ExportSheet>(_sheets);
            }
        }

        /// <summary>
        /// Ajoute une feuille d'export.
        /// </summary>
        /// <param name="name">Nom de la feuille de données.</param>
        /// <param name="dataSource">Source de la feuille de données.</param>
        /// <param name="exportProperties">Liste des propriétés à afficher.</param>
        public void AddExportSheet(string name, object dataSource, ICollection<ExportPropertyDefinition> exportProperties) {
            if (string.IsNullOrEmpty(name)) {
                throw new ArgumentNullException("name");
            }

            if (dataSource == null) {
                throw new ArgumentNullException("dataSource");
            }

            if (exportProperties == null) {
                throw new ArgumentNullException("exportProperties");
            }

            if (exportProperties.Count == 0) {
                throw new NotSupportedException("Export with no export properties is not supported.");
            }

            _sheets.Add(new ExportSheet(name, dataSource, exportProperties));
        }
    }
}
