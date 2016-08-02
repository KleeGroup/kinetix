using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Manager;
using Kinetix.Monitoring.Storage;

namespace Kinetix.Monitoring.Html {
    /// <summary>
    /// Contrôle de rendu du monitoring.
    /// </summary>
    [ToolboxData("<{0}:MonitoringControl />")]
    public sealed class MonitoringControl : Control, IHttpHandler, IReadOnlySessionState {

        private static ExternalDatabaseSet _databaseSet;
        private readonly Dictionary<string, string> _params = new Dictionary<string, string>();
        private readonly ICollection<CounterData> _counters = new List<CounterData>();

        /// <summary>
        /// Indique que les données ont expirées et doivent être rechargée.
        /// </summary>
        public event EventHandler CountersExpired;

        /// <summary>
        /// Indique qu'une réinitialisation de base est demandée.
        /// </summary>
        public event CommandEventHandler Reseting;

        /// <summary>
        /// Chemin d'accès au handler du contrôle.
        /// </summary>
        public string HandlerPath {
            get;
            set;
        }

        /// <summary>
        /// Liste des compteurs à afficher.
        /// </summary>
        public ICollection<CounterData> Counters {
            get {
                return _counters;
            }
        }

        /// <summary>
        /// Indique si le handler est réutilisable (et donc stateless).
        /// </summary>
        bool IHttpHandler.IsReusable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Liste des bases de données.
        /// </summary>
        [SuppressMessage("Klee.FxCop", "EX0037:DoNotUseHttpSession", Justification = "Usage maitrisé de la session HTTP.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Conceptuellement non statique")]
        private Dictionary<string, IManagerDescription> DatabaseDefinition {
            get {
                Dictionary<string, IManagerDescription> databaseDefinition =
                        (Dictionary<string, IManagerDescription>)HttpContext.Current.Session["DatabaseDefinition"];
                if (databaseDefinition == null) {
                    databaseDefinition = new Dictionary<string, IManagerDescription>();
                    HttpContext.Current.Session["DatabaseDefinition"] = databaseDefinition;
                }

                return databaseDefinition;
            }
        }

        /// <summary>
        /// Liste des compteurs.
        /// </summary>
        [SuppressMessage("Klee.FxCop", "EX0037:DoNotUseHttpSession", Justification = "Usage maitrisé de la session HTTP.")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Conceptuellement non statique")]
        private Dictionary<string, ICounterDefinition> CounterDefinition {
            get {
                Dictionary<string, ICounterDefinition> counterDefinition =
                        (Dictionary<string, ICounterDefinition>)HttpContext.Current.Session["CounterDefinition"];
                if (counterDefinition == null) {
                    counterDefinition = new Dictionary<string, ICounterDefinition>();
                    HttpContext.Current.Session["CounterDefinition"] = counterDefinition;
                }

                return counterDefinition;
            }
        }

        /// <summary>
        /// Ajoute un nouveau paramètre.
        /// </summary>
        /// <param name="param">Paramètre.</param>
        /// <param name="value">Valeur.</param>
        public void AddParam(string param, string value) {
            _params.Add(param, value);
        }

        /// <summary>
        /// Ajoute la description d'une base de données.
        /// </summary>
        /// <param name="databaseName">Nom de la base de données.</param>
        /// <param name="description">Description.</param>
        public void AddDatabaseDescription(string databaseName, IManagerDescription description) {
            if (!this.DatabaseDefinition.ContainsKey(databaseName)) {
                this.DatabaseDefinition.Add(databaseName, description);
            }
        }

        /// <summary>
        /// Ajoute la définition d'un compteur.
        /// </summary>
        /// <param name="counter">Définition.</param>
        public void AddCounterDefinition(ICounterDefinition counter) {
            if (counter == null) {
                throw new ArgumentNullException("counter");
            }

            if (!this.CounterDefinition.ContainsKey(counter.Code)) {
                this.CounterDefinition.Add(counter.Code, counter);
            }
        }

        /// <summary>
        /// Réalise le rendu du contrôle.
        /// </summary>
        /// <param name="writer">Writer HTML.</param>
        public override void RenderControl(HtmlTextWriter writer) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            Context ctx = new Context(HttpContext.Current, this.ResolveUrl(this.HandlerPath));
            ctx.ClearAction();
            foreach (string param in _params.Keys) {
                ctx.AddParam(param, _params[param]);
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Id, "managers");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            SortedDictionary<int, string> sortMap = new SortedDictionary<int, string>();
            foreach (string databaseName in _databaseSet.DatabaseNames) {
                IManagerDescription description = this.DatabaseDefinition[databaseName];
                sortMap.Add(description.Priority, databaseName);
            }

            foreach (int priority in sortMap.Keys) {
                string databaseName = sortMap[priority];
                IManagerDescription description = this.DatabaseDefinition[databaseName];

                writer.AddAttribute(HtmlTextWriterAttribute.Id, "manager");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                writer.WriteLine("<h1 style=\"background-image: url('" + this.ResolveUrl(this.HandlerPath) + "?CONTENT=img_manager&MON_ID=" +
                        description.Image + "');\">");

                writer.WriteLine(description.Name);
                writer.WriteLine("</h1>");
                writer.WriteLine("<div id=\"description\">");

                IHyperCube hyperCube = _databaseSet.GetDatabase(databaseName);
                HtmlPageHelper.ToSummary(hyperCube, ctx, writer);
                HtmlPageHelper.ToTable(hyperCube, ctx, writer);

                writer.WriteLine("</div>"); // description

                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }

        /// <summary>
        /// Traite une requête HTTP.
        /// </summary>
        /// <param name="context">Context HTTP de la requête.</param>
        void IHttpHandler.ProcessRequest(HttpContext context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            Context requestContext = new Context(context, null);

            if (requestContext.Content.Equals("sparklines.png")) {
                context.Response.ContentType = "image/png";
                HtmlPageRenderer.ToChart(_databaseSet.GetDatabase(requestContext.Id), requestContext, context.Response.OutputStream);
            } else if (requestContext.Content.Equals("img_manager")) {
                foreach (IManagerDescription description in this.DatabaseDefinition.Values) {
                    if (description.Image == requestContext.Id) {
                        context.Response.ContentType = description.ImageMimeType;
                        context.Response.OutputStream.Write(description.ImageData, 0, description.ImageData.Length);
                        break;
                    }
                }
            } else if (HtmlPageRenderer.IsExportPage(requestContext.Content)) {
                HtmlPageRenderer.SetHeaderCsv(context.Response);
                HtmlPageHelper.ToCsv(_databaseSet.GetDatabase(requestContext.ActionDataBase), requestContext, context.Response.Output);
            } else {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Chargement du contrôle.
        /// </summary>
        /// <param name="e">Argument.</param>
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            string action = this.Page.Request["ACTION"];
            string actionDatabase = this.Page.Request["ACTION_DATABASE"];
            if (action == "RESET" && !string.IsNullOrEmpty(actionDatabase) && this.Reseting != null) {
                this.Reseting(this, new CommandEventArgs(actionDatabase, actionDatabase));
            }
        }

        /// <summary>
        /// Pré-rendu de la page.
        /// </summary>
        /// <param name="e">Argument.</param>
        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            this.CountersExpired(this, EventArgs.Empty);
            _databaseSet = new ExternalDatabaseSet(this.Counters, this.CounterDefinition.Values, true);
        }
    }
}
