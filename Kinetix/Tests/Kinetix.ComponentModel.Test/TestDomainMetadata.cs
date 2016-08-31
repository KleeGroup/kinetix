using System;
using System.ComponentModel.DataAnnotations;
using Kinetix.ComponentModel.Formatters;

namespace Kinetix.ComponentModel.Test {
    /// <summary>
    /// Définit les domaines utilisés par l'application.
    /// Une instance de cette classe doit être créée et initialisée
    /// au démarrage de l'application.
    /// </summary>
    [DomainMetadataType]
    public class TestDomainMetadata {

        /// <summary>
        /// Domaine BOOLEEN.
        /// </summary>
        [Domain("BOOLEEN")]
        public bool? Booleen { get; set; }

        /// <summary>
        /// Domaine COMMENTAIRE.
        /// </summary>
        [Domain("COMMENTAIRE")]
        [StringLength(4000)]
        public string Commentaire { get; set; }

        /// <summary>
        /// Domaine DATE.
        /// </summary>
        [Domain("DATE")]
        [CustomTypeConverter(typeof(FormatterDate))]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Domaine IDENTIFIANT.
        /// </summary>
        [Domain("IDENTIFIANT")]
        public int? Identifiant { get; set; }

        /// <summary>
        /// Domaine LIBELLE_COURT.
        /// </summary>
        [Domain("LIBELLE_COURT")]
        [StringLength(30)]
        public string LibelleCourt { get; set; }

        /// <summary>
        /// Domaine LIBELLE_LONG.
        /// </summary>
        [Domain("LIBELLE_LONG")]
        [StringLength(250)]
        public string LibelleLong { get; set; }

        /// <summary>
        /// Domaine PRIX.
        /// </summary>
        [Domain("PRIX")]
        public decimal? Prix { get; set; }

        /// <summary>
        /// Domaine POURCENTAGE.
        /// </summary>
        [Domain("POURCENTAGE")]
        [CustomTypeConverter(typeof(FormatterPercent))]
        public decimal? Pourcentage { get; set; }
    }
}
