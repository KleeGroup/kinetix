using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Manager;

namespace Kinetix.Monitoring.Html {
    /// <summary>
    /// Prend en charge les requêtes HTTP de management.
    /// </summary>
    public sealed class AnalyticsHandler : IHttpHandler {

        private static string _assemblyVersion;

        /// <summary>
        /// Indique si le handler est réutilisable (et donc stateless).
        /// </summary>
        public bool IsReusable {
            get {
                return true;
            }
        }

        /// <summary>
        /// Retourne les versions en cours pour l'application.
        /// </summary>
        private static string AssemblyVersion {
            get {
                if (_assemblyVersion == null) {
                    StringBuilder sb = new StringBuilder();
                    Assembly[] myAssemblies = Thread.GetDomain().GetAssemblies();
                    Assembly myAssembly = null;
                    for (int i = 0; i < myAssemblies.Length; i++) {
                        myAssembly = myAssemblies[i];
                        string name = myAssembly.GetName().Name;
                        if (name.StartsWith("Kinetix", StringComparison.OrdinalIgnoreCase)) {
                            sb.Append(name.PadRight(40));
                            sb.Append(" : ");
                            string[] ss = myAssembly.FullName.Split(',');
                            foreach (string s in ss) {
                                if (s.Trim().ToUpper(CultureInfo.InvariantCulture).StartsWith("VERSION", StringComparison.OrdinalIgnoreCase)) {
                                    sb.Append(s.Substring(s.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1));
                                    sb.Append("\n");
                                    break;
                                }
                            }
                        }
                    }

                    _assemblyVersion = sb.ToString();
                }

                return _assemblyVersion;
            }
        }

        /// <summary>
        /// Traite une requête HTTP.
        /// </summary>
        /// <param name="context">Context HTTP de la requête.</param>
        public void ProcessRequest(HttpContext context) {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            Context requestContext = new Context(context, null);

            if (HtmlPageRenderer.IsCounterPage(requestContext.Content)) {
                ProcessPageRequest(context, requestContext);
            } else if (HtmlPageRenderer.IsExportPage(requestContext.Content)) {
                HtmlPageRenderer.SetHeaderCsv(context.Response);
                HtmlPageHelper.ToCsv(
                    Analytics.Instance.GetDataBase(requestContext.ActionDataBase).HyperCube,
                    requestContext,
                    context.Response.Output);
            } else if (requestContext.Content != null) {
                ProcessContentRequest(context, requestContext);
            } else {
                // On ne sait pas traiter la requête
                context.Response.StatusCode = 500;
            }
        }

        /// <summary>
        /// Traite une requête pour du contenu attaché à la page.
        /// </summary>
        /// <param name="context">Contexte HTTP.</param>
        /// <param name="requestContext">Contexte de la requête.</param>
        private static void ProcessContentRequest(HttpContext context, Context requestContext) {
            if (requestContext.Content.Equals("img_manager")) {
                string mimeType = null;
                IManagerDescription managerDescription = null;

                foreach (IManager manager in ManagerContainer.Instance.ManagerList) {
                    IManagerDescription description = manager.Description;
                    if (description != null && description.Image != null
                            && description.Image.Equals(requestContext.Id)) {
                        managerDescription = description;
                        break;
                    }
                }

                if (managerDescription == null) {
                    byte[] imageData = IR.Misc_png;
                    mimeType = "image/png";
                    context.Response.ContentType = mimeType;
                    context.Response.OutputStream.Write(imageData, 0, imageData.Length);
                } else {
                    context.Response.ContentType = managerDescription.ImageMimeType;
                    byte[] img = managerDescription.ImageData;
                    context.Response.OutputStream.Write(img, 0, img.Length);
                }
            } else if (requestContext.Content.Equals("sparklines.png")) {
                context.Response.ContentType = "image/png";
                CounterDataBase counterDataBase = Analytics.Instance.GetDataBase(requestContext.Id);
                HtmlPageRenderer.ToChart(counterDataBase.HyperCube, requestContext, context.Response.OutputStream);
            } else {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Traite un requête pour retourner un page de résultat.
        /// </summary>
        /// <param name="context">Contexte HTTP.</param>
        /// <param name="requestContext">Contexte de la requête.</param>
        private static void ProcessPageRequest(HttpContext context, Context requestContext) {
            TextWriter textWriter;
            if (context == null) {
                textWriter = new StringWriter(CultureInfo.CurrentCulture);
            } else {
                textWriter = context.Response.Output;
            }

            using (HtmlTextWriter writer = new HtmlTextWriter(textWriter)) {
                // Page HTML des compteurs
                if (context != null) {
                    HtmlPageRenderer.SetHeaderHtml(context.Response);
                }

                HtmlPageRenderer.HeadPage(writer);

                if ("RESET".Equals(requestContext.Action)) {
                    Analytics.Instance.Reset(requestContext.ActionDataBase);
                    writer.WriteLine("Les compteurs ont été remis à zéro.<br/>");
                    HtmlPageRenderer.DoEnd(requestContext, writer);
                } else if (requestContext.Action != null && requestContext.ListenerClassName != null) {
                    HtmlPageRenderer.DoEnd(requestContext, writer);
                } else {
                    HtmlPageRenderer.CounterPage(requestContext, writer);
                }

                writer.WriteLine("<!--");
                writer.WriteLine(AnalyticsHandler.AssemblyVersion);
                writer.WriteLine("-->");
                writer.WriteLine("</body></html>");
            }
        }
    }
}
