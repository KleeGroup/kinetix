using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Kinetix.Reporting.TagHandlers {

    /// <summary>
    /// Handler du tag field, permet de remplacer le tag par du texte.
    /// </summary>
    internal class FieldCurrencyHandler : FieldHandler {

        /// <summary>
        /// Printing culture.
        /// </summary>
        private static CultureInfo printingCulture = new CultureInfo("fr-FR");

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="currentPart">OpenXmlPart courant.</param>
        /// <param name="currentXmlElement">Tag Custom OpenXML.</param>
        /// <param name="currentDataSource">Source de données courante.</param>
        /// <param name="documentId">Id document en cours.</param>
        /// <param name="isXmlData">Si la source en xml.</param>
        public FieldCurrencyHandler(OpenXmlPart currentPart, CustomXmlElement currentXmlElement, object currentDataSource, Guid documentId, bool isXmlData)
            : base(currentPart, currentXmlElement, currentDataSource, documentId, isXmlData) {
            this.Precision = int.Parse(this["precision"], NumberStyles.Integer, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        /// Precision.
        /// </summary>
        public int Precision {
            get;
            private set;
        }

        /// <summary>
        /// Récupérer l'objet Text a inséré avec un formatage avec precision.
        /// </summary>
        /// <param name="propertyValue">Property value.</param>
        /// <returns>Text.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "A un sens ici.")]
        public override Text GetText(object propertyValue) {
            if (!string.IsNullOrEmpty(propertyValue.ToString())) {
                decimal d = 0;

                // On utilise le même mécanisme que dans la méthode InternalConvertFromString de FormatterMontant vu avec OGY.
                if (this.IsXmlData) {
                    d = decimal.Parse(propertyValue.ToString().Replace(".", ",").Replace(" ", string.Empty).Trim(), NumberStyles.Currency, printingCulture.NumberFormat);
                } else {
                    d = Convert.ToDecimal(propertyValue.ToString(), printingCulture);
                }

                Text tx = new Text(string.Format(printingCulture, "{0:N" + this.Precision + "}", d));
                return tx;
            }

            return new Text();
        }
    }
}
