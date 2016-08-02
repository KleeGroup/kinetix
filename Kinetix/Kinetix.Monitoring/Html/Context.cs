using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

namespace Kinetix.Monitoring.Html {
    /// <summary>
    /// Contexte d'intérrogation de la page de monitoring.
    /// </summary>
    internal sealed class Context {

        private readonly Dictionary<string, string> _otherParams;

        /// <summary>
        /// Graphique :
        ///  - données contextuelles (représente-t-on les services, les requêtes, les reports...)
        ///  - exemple ZOOM=SERVICES:SV_TOTO.
        /// </summary>
        private readonly string _dataBase;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="context">Context HTTP.</param>
        /// <param name="handler">Adresse du handler pour le contenu des pages.</param>
        internal Context(HttpContext context, string handler) {
            HttpRequest request = (context != null) ? context.Request : null;
            this.Handler = handler;
            _otherParams = new Dictionary<string, string>();
            _dataBase = ValueOf(request, "DATABASE", null);
            this.RequestName = ValueOf(request, "REQUEST_NAME", null);
            this.Action = ValueOf(request, "ACTION", null);
            this.ListenerClassName = ValueOf(request, "LISTENER", null);
            this.ActionDataBase = ValueOf(request, "ACTION_DATABASE", null);

            this.Sort = ValueOf(request, "SORT", null);
            this.Forme = ValueOf(request, "FORME", null);
            this.Filtre = ValueOf(request, "FILTRE", null);
            this.Content = ValueOf(request, "CONTENT", null);
            this.Id = ValueOf(request, "MON_ID", null);
            this.Level = ValueOf(request, "LEVEL", null);
            this.EndDate = DateTime.Now;

            this.Height = Context.ValueOf(request, "HEIGHT", 400);
            this.Width = Context.ValueOf(request, "WIDTH", 80);
        }

        /// <summary>
        /// Crée un clone.
        /// </summary>
        /// <param name="initialContext">Context initial.</param>
        private Context(Context initialContext) {
            _dataBase = initialContext._dataBase;
            _otherParams = new Dictionary<string, string>(initialContext._otherParams);
            this.Handler = initialContext.Handler;
            this.RequestName = initialContext.RequestName;
            this.Action = initialContext.Action;
            this.ListenerClassName = initialContext.ListenerClassName;
            this.ActionDataBase = initialContext.ActionDataBase;

            this.Sort = initialContext.Sort;
            this.Forme = initialContext.Forme;
            this.Filtre = initialContext.Filtre;
            this.Content = initialContext.Content;
            this.Id = initialContext.Id;
            this.EndDate = initialContext.EndDate;

            this.Height = initialContext.Height;
            this.Width = initialContext.Width;
        }

        /// <summary>
        /// Représente la date cible.
        /// </summary>
        internal DateTime EndDate {
            get;
            private set;
        }

        /// <summary>
        /// Données non contextuelles, c-à-d transverses à tous les graphiques.
        /// </summary>
        internal string Sort {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la forme d'affichage des graphiques.
        /// </summary>
        internal string Forme {
            get;
            set;
        }

        /// <summary>
        /// Retourne le filtre de criticité pour l'affichage des axes.
        /// </summary>
        internal string Filtre {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la largeur d'un graphique à générer.
        /// </summary>
        internal int Width {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la hauteur d'un graphique à générer.
        /// </summary>
        internal int Height {
            get;
            private set;
        }

        /// <summary>
        /// Précise un type de contenu.
        /// </summary>
        internal string Content {
            get;
            private set;
        }

        /// <summary>
        /// Précise l'identifiant d'un contenu.
        /// </summary>
        internal string Id {
            get;
            private set;
        }

        /// <summary>
        /// Représente le niveau utilisé dans la
        /// dimension temps.
        /// </summary>
        internal string Level {
            get;
            private set;
        }

        /// <summary>
        /// Action.
        /// </summary>
        internal string Action {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la classe listener responsable du traitement d'une action.
        /// </summary>
        internal string ListenerClassName {
            get;
            private set;
        }

        /// <summary>
        /// Représente la base de données ciblée.
        /// </summary>
        internal string ActionDataBase {
            get;
            private set;
        }

        /// <summary>
        /// Lien du handler.
        /// </summary>
        internal string Handler {
            get;
            private set;
        }

        /// <summary>
        /// Représente l'axe recherché.
        /// </summary>
        internal string RequestName {
            get;
            private set;
        }

        /// <summary>
        /// Supprime l'action courante.
        /// </summary>
        public void ClearAction() {
            this.Action = null;
            this.ActionDataBase = null;
        }

        /// <summary>
        /// Ajoute un nouveau paramètre.
        /// </summary>
        /// <param name="param">Paramètre.</param>
        /// <param name="value">Valeur.</param>
        internal void AddParam(string param, string value) {
            _otherParams.Add(param, value);
        }

        /// <summary>
        /// Retourne le nom de la requête correspondant à la base, null si aucun.
        /// </summary>
        /// <param name="dataBaseName">Nom de la base.</param>
        /// <returns>Nom de la requête.</returns>
        internal string GetRequestName(string dataBaseName) {
            if (dataBaseName == null) {
                throw new ArgumentNullException("dataBaseName");
            }

            if (dataBaseName.Equals(_dataBase)) {
                return this.RequestName;
            }

            return null;
        }

        /// <summary>
        /// Export des données d'une base.
        /// </summary>
        /// <param name="newDataBase">Base de données à exporter.</param>
        /// <returns>Context.</returns>
        internal Context Export(string newDataBase) {
            if (newDataBase == null) {
                throw new ArgumentNullException("newDataBase");
            }

            Context newContext = this.Copy();
            newContext.ActionDataBase = newDataBase;
            newContext.Filtre = "NONE";
            newContext.Content = '/' + HtmlPageRenderer.ExportCsvPage;
            return newContext;
        }

        /// <summary>
        /// Modification du contexte pour mettre à zero les statistiques.
        /// </summary>
        /// <param name="newDataBase">Base de données à réinitialiser.</param>
        /// <returns>Context.</returns>
        internal Context Reset(string newDataBase) {
            if (newDataBase == null) {
                throw new ArgumentNullException("newDataBase");
            }

            Context newContext = this.Copy();
            newContext.ActionDataBase = newDataBase;
            newContext.Action = "RESET";
            return newContext;
        }

        /// <summary>
        /// Modification du contexte courante pour déclencher un zoom.
        /// </summary>
        /// <param name="newDataBase">Nom de la base sur laquelle porte le zoom.</param>
        /// <param name="newRequestName">Nom de l'axe.</param>
        /// <returns>Nouveau contexte.</returns>
        internal Context Zoom(string newDataBase, string newRequestName) {
            Context newContext = this.Copy();
            newContext.ActionDataBase = newDataBase;
            newContext.RequestName = newRequestName;
            return newContext;
        }

        /// <summary>
        /// Modification du contexte pour changer le tri des affichages.
        /// </summary>
        /// <param name="newSort">Tri à appliquer.</param>
        /// <returns>Context.</returns>
        internal Context ChangeSort(string newSort) {
            Context newContext = this.Copy();
            newContext.Sort = newSort;
            return newContext;
        }

        /// <summary>
        /// Modification du contexte pour filtrer les affichages.
        /// </summary>
        /// <param name="newFiltre">Filtre à appliquer.</param>
        /// <returns>Context.</returns>
        internal Context ChangeFiltre(string newFiltre) {
            Context newContext = this.Copy();
            newContext.Filtre = newFiltre;
            return newContext;
        }

        /// <summary>
        /// Modification du contexte pour n'effectuer aucune action.
        /// </summary>
        /// <returns>Context.</returns>
        internal Context NoAction() {
            Context newContext = this.Copy();
            newContext.Action = null;
            return newContext;
        }

        /// <summary>
        /// Retourne l'url.
        /// </summary>
        /// <returns>Url.</returns>
        internal string GetHandlerUrl() {
            return ((this.Handler != null) ? this.Handler : string.Empty) + GetUrl();
        }

        /// <summary>
        /// Retourne l'url.
        /// </summary>
        /// <returns>Url.</returns>
        internal string GetUrl() {
            StringBuilder url = new StringBuilder();
            url.Append('?');
            foreach (string param in _otherParams.Keys) {
                url.Append("&").Append(param).Append("=").Append(HttpUtility.UrlEncode(_otherParams[param]));
            }

            if (_dataBase != null) {
                url.Append("&DATABASE=").Append(HttpUtility.UrlEncode(_dataBase));
            }

            if (this.RequestName != null) {
                url.Append("&REQUEST_NAME=").Append(HttpUtility.UrlEncode(this.RequestName));
            }

            if (this.Action != null) {
                url.Append("&ACTION=").Append(HttpUtility.UrlEncode(this.Action));
            }

            if (this.ListenerClassName != null) {
                url.Append("&LISTENER=").Append(HttpUtility.UrlEncode(this.ListenerClassName));
            }

            if (this.ActionDataBase != null) {
                url.Append("&ACTION_DATABASE=").Append(HttpUtility.UrlEncode(this.ActionDataBase));
            }

            if (this.Sort != null) {
                url.Append("&SORT=").Append(HttpUtility.UrlEncode(this.Sort));
            }

            if (this.Forme != null) {
                url.Append("&FORME=").Append(HttpUtility.UrlEncode(this.Forme));
            }

            if (this.Filtre != null) {
                url.Append("&FILTRE=").Append(HttpUtility.UrlEncode(this.Filtre));
            }

            if (this.Content != null) {
                url.Append("&CONTENT=").Append(HttpUtility.UrlEncode(this.Content));
            }

            return url.ToString();
        }

        /// <summary>
        /// Lie un entier dans la requête HTTP.
        /// </summary>
        /// <param name="request">Requête.</param>
        /// <param name="paramName">Nom du paramètre.</param>
        /// <param name="defaultValue">Valeur par défaut.</param>
        /// <returns>Valeur de la requête ou valeur par défaut.</returns>
        private static string ValueOf(HttpRequest request, string paramName, string defaultValue) {
            if (request == null) {
                return defaultValue;
            }

            return request[paramName];
        }

        /// <summary>
        /// Lie un entier dans la requête HTTP.
        /// </summary>
        /// <param name="request">Requête.</param>
        /// <param name="paramName">Nom du paramètre.</param>
        /// <param name="defaultValue">Valeur par défaut.</param>
        /// <returns>Valeur de la requête ou valeur par défaut.</returns>
        private static int ValueOf(HttpRequest request, string paramName, int defaultValue) {
            if (request == null) {
                return defaultValue;
            }

            int value;
            if (!int.TryParse(request[paramName], NumberStyles.Any, CultureInfo.InvariantCulture, out value)) {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Crée une copy du contexte.
        /// </summary>
        /// <returns>Nouveau contexte.</returns>
        private Context Copy() {
            return new Context(this);
        }
    }
}
