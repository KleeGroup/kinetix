using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Manager;

namespace Kinetix.Monitoring.Html {
    /// <summary>
    /// Rendu visuel des compteurs : affichage des pages.
    /// </summary>
    public static class HtmlPageRenderer {

        /// <summary>
        /// Renvoie une indentation a un niveau.
        /// </summary>
        internal const string Indent = "     ";

        /// <summary>
        /// Renvoie une indentation a deux niveaux.
        /// </summary>
        internal const string Indent2 = Indent + Indent;

        /// <summary>
        /// Nom de l'export CSV.
        /// </summary>
        internal const string ExportCsvPage = "counter_export.csv";

        [ThreadStatic]
        private static Context _context = new Context(null, null);

        /// <summary>
        /// Obtient ou définit le context courant.
        /// </summary>
        internal static Context CurrentContext {
            get {
                if (HttpContext.Current == null) {
                    return _context;
                } else {
                    return (Context)HttpContext.Current.Items["MonitoringContext"];
                }
            }

            set {
                if (HttpContext.Current == null) {
                    _context = value;
                } else {
                    HttpContext.Current.Items["MonitoringContext"] = value;
                }
            }
        }

        /// <summary>
        /// Rendu de la synthèse des statistiques relatives à une base de données analytique.
        /// </summary>
        /// <param name="databaseName">Nom de la base de données.</param>
        /// <param name="writer">Writer HTML.</param>
        public static void ToHtml(string databaseName, HtmlTextWriter writer) {
            Context context = HtmlPageRenderer.CurrentContext;
            CounterDataBase counterDataBase = Analytics.Instance.GetDataBase(databaseName);
            HtmlPageHelper.ToSummary(counterDataBase.HyperCube, context, writer);
            HtmlPageHelper.ToTable(counterDataBase.HyperCube, context, writer);
        }

        /// <summary>
        /// Indique si page d'expott des compteurs.
        /// </summary>
        /// <param name="pathInfo">Pathinfo de la request.</param>
        /// <returns>Si page d'export des compteurs.</returns>
        internal static bool IsExportPage(string pathInfo) {
            return IsPage(ExportCsvPage, pathInfo);
        }

        /// <summary>
        /// Indique si page de visualistion des compteurs.
        /// </summary>
        /// <param name="pathInfo">Pathinfo de la request.</param>
        /// <returns>Si page de visualistion des compteurs.</returns>
        internal static bool IsCounterPage(string pathInfo) {
            return string.IsNullOrEmpty(pathInfo);
        }

        /// <summary>
        /// Rendu de graphique selon les paramètres dans la requête http.
        /// </summary>
        /// <param name="hyperCube">Hypercube.</param>
        /// <param name="context">Context courant de la requête.</param>
        /// <param name="s">Stream.</param>
        internal static void ToChart(IHyperCube hyperCube, Context context, Stream s) {
            HtmlGraphHelper.RenderGraph(hyperCube, context, s);
        }

        /// <summary>
        /// Rendu html de la page globale des compteurs.
        /// </summary>
        /// <param name="context">Context courant.</param>
        /// <param name="writer">Writer HTML.</param>
        internal static void CounterPage(Context context, HtmlTextWriter writer) {
            RenderManagers(context, writer);
        }

        /// <summary>
        /// Réalise le rendu correspondant à la fin de traitement d'une action.
        /// </summary>
        /// <param name="context">Context de la requête.</param>
        /// <param name="writer">Writer HTML.</param>
        internal static void DoEnd(Context context, HtmlTextWriter writer) {
            writer.WriteLine(GetLink(context.NoAction(), "Retour", true));
        }

        /// <summary>
        /// Réalise le rendu du début de page.
        /// Rend tout le début de page jusqu'à l'ouverture de la balise body inclu.
        /// </summary>
        /// <param name="writer">Writer HTML.</param>
        internal static void HeadPage(HtmlTextWriter writer) {
            writer.Write("<!DOCTYPE html PUBLIC \"-/");
            writer.Write("/W3C//DTD XHTML 1.0 Strict/");
            writer.Write("/EN\" \"http:/");
            writer.WriteLine("/www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">");
            writer.WriteLine("<html><head>");
            writer.WriteLine("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=ISO-8859-15\"><title>Compteurs</title>");
            writer.WriteLine("<style>");
            writer.WriteLine("body {font-family: Verdana, Arial, Helvetica, sans-serif; font-size:0.8em;}");
            writer.WriteLine("a {color: #00F;}");

            writer.WriteLine("ul {");
            writer.WriteLine("  text-align: right;");
            writer.WriteLine("  color: #333;");
            writer.WriteLine("}");

            writer.WriteLine("li {");
            writer.WriteLine("  display: inline;");
            writer.WriteLine("  list-style-type: none;");
            writer.WriteLine("  border-right: 1px solid #333;");
            writer.WriteLine("  padding-right: 10px;");
            writer.WriteLine("  padding-left: 10px;");
            writer.WriteLine("}");

            writer.WriteLine("li a:link, li a:visited {");
            writer.WriteLine("  color: #333;");
            writer.WriteLine("}");
            writer.WriteLine("li a:hover {");
            writer.WriteLine("  border-bottom: 3px solid #333;");
            writer.WriteLine("}");

            writer.WriteLine("h2 {");
            writer.WriteLine("  font-size: 1em;");
            writer.WriteLine("}");

            writer.WriteLine("h1 {");
            writer.WriteLine("  font-family: 'trebuchet ms', verdana, arial, sans-serif; ");
            writer.WriteLine("  font-size: 1.4em;");
            writer.WriteLine("  letter-spacing: 1em;");
            writer.WriteLine("  color:#333;");

            writer.WriteLine("  height:48px;");
            writer.WriteLine("  padding-top:30px;");
            writer.WriteLine("  border-bottom: 2px #333 dotted;");

            writer.WriteLine("  margin-top: 4px;");
            writer.WriteLine("  margin-bottom: 8px;");

            // Gestion de l'icone de h1
            writer.WriteLine("  padding-left: 53px;"); // 48+5
            writer.WriteLine("  background-repeat:no-repeat;");
            writer.WriteLine("  background-position:left;");

            // Image par défaut
            writer.WriteLine("  background-image: url(static/images/monitoring/misc.png);");
            writer.WriteLine("}");

            writer.WriteLine("table {border-collapse: collapse;}");
            writer.WriteLine("img {margin-top: 4px; border: 0px;}");
            writer.WriteLine("th {border: 1px solid #000000; background: #AAAAAA;}");
            writer.WriteLine("td.monitoring {border: 1px solid #000000; font-size:0.8em;}");
            writer.WriteLine("ul {margin-top: 2px; margin-bottom: 2px;}");
            writer.WriteLine("tr.odd td {background: #EEEEEE}");

            writer.WriteLine("div#managers {");
            writer.WriteLine("  margin-left: 50px;");
            writer.WriteLine("  margin-right: 50px;");
            writer.WriteLine("  background: #EEEEEE;");
            writer.WriteLine("  border-width: thin;");
            writer.WriteLine("  border-style: solid;");
            writer.WriteLine("  border-color: #CCCCCC;");
            writer.WriteLine("} ");

            writer.WriteLine("div#manager {");
            writer.WriteLine("  margin-left: 5px;");
            writer.WriteLine("  margin-right: 5px;");
            writer.WriteLine("} ");

            writer.WriteLine("div#description {");
            writer.WriteLine("  clear: both;");
            writer.WriteLine("  margin-top:5px;");
            writer.WriteLine("  margin-left:25px; font-size:0.8em;");
            writer.WriteLine("  margin-bottom:10px;");
            writer.WriteLine("} ");

            writer.WriteLine("</style>");
            writer.WriteLine("</head>");
            writer.WriteLine("<body>");
        }

        /// <summary>
        /// Retourne le code html du lien.
        /// </summary>
        /// <param name="context">Context courant.</param>
        /// <param name="libelle">Libellé du lien.</param>
        /// <param name="actif">Si actif.</param>
        /// <returns>Bloc HTML de lien.</returns>
        internal static string GetLink(Context context, string libelle, bool actif) {
            if (!actif) {
                return "<b>" + libelle + "</b>";
            }

            return "<a href=\"" + context.GetUrl() + "\">" + libelle + "</a>";
        }

        /// <summary>
        /// Positionne sur la response le header http indiquant un format csv.
        /// </summary>
        /// <param name="response">RÃ©ponse.</param>
        internal static void SetHeaderCsv(HttpResponse response) {
            response.AddHeader("content-disposition", "attachment;filename=\"" + ExportCsvPage + "\"");
            response.ContentType = "application/vnd.ms-excel";
            response.Charset = "UTF-8";
            response.ContentEncoding = Encoding.UTF8;
        }

        /// <summary>
        /// Positionne sur la response le header http indiquant un format html.
        /// </summary>
        /// <param name="response">Réponse.</param>
        internal static void SetHeaderHtml(HttpResponse response) {
            response.ContentType = "text/html";
            response.Charset = "UTF-8";
        }

        /// <summary>
        /// Indique si l'adressse transmise et la même que le Pathinfo.
        /// </summary>
        /// <param name="page">Adresse de la page.</param>
        /// <param name="pathInfo">Pathinfo de la  page.</param>
        /// <returns>Indique si l'adressse transmise et la même que le Pathinfo.</returns>
        private static bool IsPage(string page, string pathInfo) {
            return ('/' + page).Equals(pathInfo);
        }

        /// <summary>
        /// Rendu html des managers.
        /// </summary>
        /// <param name="context">Context courant.</param>
        /// <param name="writer">Writer HTML.</param>
        private static void RenderManagers(Context context, HtmlTextWriter writer) {
            // Rendu du reset global.
            writer.Write("<div>");
            writer.WriteLine("<ul>");
            writer.Write("<li>");
            writer.WriteLine(HtmlPageRenderer.GetLink(context.Reset("ALL"), "Tout remettre à zéro", true));
            writer.WriteLine("</li>");
            writer.WriteLine("</ul>");
            writer.WriteLine("</div>");

            writer.WriteLine("<div id=\"managers\">");
            writer.WriteLine("</div>");

            CurrentContext = context;

            foreach (IManager manager in ManagerContainer.Instance.ManagerList) {
                writer.WriteLine("<div id=\"manager\">");

                IManagerDescription description = manager.Description;
                string name = (description != null) ? description.Name : manager.GetType().Name;
                string image = (description != null) ? description.Image : "misc.png";

                writer.WriteLine("<h1 style=\"background-image: url('?CONTENT=img_manager&MON_ID=" + image + "');\">");
                writer.WriteLine(name);
                writer.WriteLine("</h1>");

                if (description != null) {
                    writer.WriteLine("<div id=\"description\">");

                    // On ne passe pas le contexte opour ne pas créer une adhérence au niveau de l'API.
                    description.ToHtml(writer);
                    writer.WriteLine("</div>"); // description
                }

                writer.WriteLine("</div>"); // manager
                writer.WriteLine("</td>");
            }

            writer.WriteLine("</div>");
        }
    }
}
