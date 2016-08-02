using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.UI;
using Kinetix.Monitoring.Counter;

namespace Kinetix.Monitoring.Html {
    /// <summary>
    /// Rendu HTML, CSV.
    /// </summary>
    internal static class HtmlPageHelper {
        /// <summary>
        /// Réalise le rendu du résumé d'un hypercube.
        /// </summary>
        /// <param name="hyperCube">Hypercube à rendre.</param>
        /// <param name="context">Context courant de la requête.</param>
        /// <param name="writer">Writer HTML.</param>
        internal static void ToSummary(IHyperCube hyperCube, Context context, HtmlTextWriter writer) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            string requestName = context.GetRequestName(hyperCube.Name);

            writer.WriteLine("<ul>");
            writer.Write("<li>");
            writer.WriteLine(HtmlPageRenderer.GetLink(context, "Rafraîchir", true));
            writer.WriteLine("</li>");
            if (hyperCube.IsResetable) {
                writer.Write("<li>");
                writer.WriteLine(HtmlPageRenderer.GetLink(context.Reset(hyperCube.Name), "Remettre à zéro", true));
                writer.WriteLine("</li>");
            }

            // Si on est sur une page locale on rend accès aux statistiques globales
            if (requestName != null) {
                writer.Write("<li>");
                writer.WriteLine(HtmlPageRenderer.GetLink(context.Zoom(null, null), "Statistiques globales", true));
                writer.WriteLine("</li>");
            }

            writer.WriteLine("</ul>");
            Context targetContext;

            if (requestName == null) {
                writer.Write("<h2>Statistiques globales</h2>");
                targetContext = context;
            } else {
                writer.Write("<h2>Statistiques pour ");
                writer.Write(requestName);
                writer.WriteLine("</h2>");
                targetContext = context.Zoom(null, null);
            }

            ICube cube = hyperCube.GetCube(new CounterCubeCriteria(requestName, TimeLevel.Hour).CreateCubeKey(context.EndDate));
            if (cube != null) {
                // On affiche les données relative au cube
                bool link = requestName == null; // Si pas de request précisé on met les liens.
                RenderCube(hyperCube, cube, writer, link, targetContext);
            }
        }

        /// <summary>
        /// Réalise le rendu d'une base au format CSV.
        /// </summary>
        /// <param name="hyperCube">Hypercube contenant les valeurs.</param>
        /// <param name="context">Contexte de la requête.</param>
        /// <param name="writer">Writer.</param>
        internal static void ToCsv(IHyperCube hyperCube, Context context, TextWriter writer) {
            string requestName = context.GetRequestName(hyperCube.Name);
            List<string> axisList = GetAllAxis(context.EndDate, hyperCube, context.Sort);

            writer.Write("Requete;");
            writer.Write("total;");
            writer.Write("hits;");

            Dictionary<string, string> activeCounters = RenderLabels(writer, hyperCube, context, requestName);

            foreach (string axis in axisList) {
                CreateCsvAxis(hyperCube, context, writer, activeCounters, axis);
            }
        }

        /// <summary>
        /// Réalise le rendu d'un hypercube axe par axe.
        /// </summary>
        /// <param name="hyperCube">Hypercube contenant les valeurs.</param>
        /// <param name="context">Contexte de la requête.</param>
        /// <param name="writer">Writer HTML.</param>
        internal static void ToTable(IHyperCube hyperCube, Context context, HtmlTextWriter writer) {
            string requestName = context.GetRequestName(hyperCube.Name);
            List<string> axisList = GetAllAxis(context.EndDate, hyperCube, context.Sort);

            writer.WriteLine("<h2>Nombre de requêtes (" + axisList.Count + ")</h2>");

            writer.Write("Filtre : ");
            writer.Write(HtmlPageRenderer.GetLink(context.ChangeFiltre("NONE"), "aucun", !"NONE".Equals(context.Filtre)));
            writer.Write(", ");
            writer.Write(HtmlPageRenderer.GetLink(context.ChangeFiltre("WARNING"), "warning", !"WARNING".Equals(context.Filtre)));
            writer.Write(", ");
            writer.WriteLine(HtmlPageRenderer.GetLink(context.ChangeFiltre(null), "critique", context.Filtre != null));
            writer.WriteLine(string.Empty);

            writer.WriteLine("<table cellspacing=\"0\" cellpadding=\"3\" border=\"0\" width=\"100%\">");

            writer.Write(HtmlPageRenderer.Indent);
            writer.WriteLine("<tr>");

            writer.Write(HtmlPageRenderer.Indent2);
            writer.Write("<th>");
            writer.Write(HtmlPageRenderer.GetLink(context.ChangeSort(null), "Requête", context.Sort != null));
            writer.WriteLine("</th>");

            writer.Write(HtmlPageRenderer.Indent2);
            writer.Write("<th>");
            writer.Write(HtmlPageRenderer.GetLink(context.ChangeSort("TOTAL"), "total (ms)", !"TOTAL".Equals(context.Sort)));
            writer.WriteLine("</th>");

            writer.Write(HtmlPageRenderer.Indent2);
            writer.Write("<th>");
            writer.Write(HtmlPageRenderer.GetLink(context.ChangeSort(Analytics.ElapsedTime), "avg (ms)", !Analytics.ElapsedTime.Equals(context.Sort)));
            writer.WriteLine("</th>");

            writer.Write(HtmlPageRenderer.Indent2);
            writer.Write("<th colspan='2'>");
            writer.Write(HtmlPageRenderer.GetLink(context.ChangeSort("HITS"), "hits", !"HITS".Equals(context.Sort)));
            writer.WriteLine("</th>");

            Dictionary<string, string> activeCounters = RenderLabels(writer, hyperCube, context, requestName);

            writer.Write(HtmlPageRenderer.Indent);
            writer.WriteLine("</tr>");

            bool odd = true;
            foreach (string axis in axisList) {
                CounterCubeCriteria criteria = new CounterCubeCriteria(axis, TimeLevel.Hour);
                ICube cube = hyperCube.GetCube(criteria.CreateCubeKey(context.EndDate));
                if (cube != null && IsShown(context.EndDate, context.Filtre, hyperCube, criteria)) {
                    CreateHtmlAxis(hyperCube, context, writer, requestName, activeCounters, odd, axis, criteria, cube);
                    odd = !odd;
                }
            }

            writer.WriteLine("</table>");
            writer.WriteLine(string.Empty);
            writer.WriteLine("<a href=\"" + context.Export(hyperCube.Name).GetHandlerUrl() + "\">Exporter</a>");
        }

        /// <summary>
        /// Tri une liste.
        /// </summary>
        /// <param name="hyperCube">Hybercube contenant les éléments de la liste.</param>
        /// <param name="allAxis">Liste des axes.</param>
        /// <param name="date">Date des valeurs à trier.</param>
        /// <param name="counterDefinitionCode">Compteur à trier.</param>
        /// <param name="statType">Type de statistique à trier.</param>
        /// <returns>Liste triée par ordre croissant des valeurs sur les axes.</returns>
        private static List<string> SortAllAxis(IHyperCube hyperCube, ICollection<string> allAxis, DateTime date, string counterDefinitionCode, CounterStatType statType) {
            // On crée une nouvelle liste pour pouvoir la trier sans toucher à l'originale.
            List<string> countersList = new List<string>(allAxis);
            countersList.Sort(new HyperCubeComparer(hyperCube, date, counterDefinitionCode, statType, TimeLevel.Hour));
            return countersList;
        }

        /// <summary>
        /// Retourne la liste de tous les axes d'un hypercybe trié sur la valeur moyen d'un compteur.
        /// </summary>
        /// <param name="date">Date des valeurs.</param>
        /// <param name="hyperCube">Hybercube contenant les valeurs.</param>
        /// <param name="sort">Compteur de tri.</param>
        /// <returns>Liste des axes triés sur un compteur.</returns>
        private static List<string> GetAllAxis(DateTime date, IHyperCube hyperCube, string sort) {
            ICollection<string> allAxis = hyperCube.AllAxis;
            if (sort == null) {
                // On crée une nouvelle liste pour pouvoir la trier sans toucher à l'originale.
                List<string> list = new List<string>(allAxis);

                // On trie cette liste par ordre alphabétique
                list.Sort();
                return list;
            } else if ("HITS".Equals(sort)) {
                return SortAllAxis(hyperCube, allAxis, date, Analytics.ElapsedTime, CounterStatType.Hits);
            } else if ("TOTAL".Equals(sort)) {
                return SortAllAxis(hyperCube, allAxis, date, Analytics.ElapsedTime, CounterStatType.Total);
            } else if (Analytics.ElapsedTime.Equals(sort)) {
                return SortAllAxis(hyperCube, allAxis, date, Analytics.ElapsedTime, CounterStatType.Avg);
            } else {
                return SortAllAxis(hyperCube, allAxis, date, sort, CounterStatType.Avg);
            }
        }

        /// <summary>
        /// Rend un axe au format CSV.
        /// </summary>
        /// <param name="hyperCube">HyperCube.</param>
        /// <param name="context">Contexte.</param>
        /// <param name="writer">Writer CSV.</param>
        /// <param name="activeCounters">Compteur actifs.</param>
        /// <param name="axis">Axe.</param>
        private static void CreateCsvAxis(IHyperCube hyperCube, Context context, TextWriter writer, Dictionary<string, string> activeCounters, string axis) {
            CounterCubeCriteria criteria = new CounterCubeCriteria(axis, TimeLevel.Hour);
            CubeKey key = criteria.CreateCubeKey(context.EndDate);
            ICube cube = hyperCube.GetCube(key);
            if (cube != null && IsShown(context.EndDate, context.Filtre, hyperCube, criteria)) {
                writer.Write(axis);
                writer.Write(";");

                double total = cube.GetCounter(Analytics.ElapsedTime).GetValue(CounterStatType.Total);
                writer.Write(Convert.ToString(total, CultureInfo.CurrentCulture));
                writer.Write(";");

                double hits = cube.GetCounter(Analytics.ElapsedTime).GetValue(CounterStatType.Hits);
                writer.Write(Convert.ToString(hits, CultureInfo.CurrentCulture));
                writer.Write(";");

                double avg;
                foreach (ICounterDefinition definition in hyperCube.AllDefinitions) {
                    if (activeCounters.ContainsKey(definition.Code)) {
                        ICounter counter = cube.GetCounter(definition.Code);
                        avg = (counter != null) ? counter.GetValue(CounterStatType.Avg) : 0;
                        writer.Write(string.Format(CultureInfo.CurrentCulture, "{0:0.##}", avg));
                        writer.Write(";");
                    }
                }

                writer.WriteLine();
            }
        }

        /// <summary>
        /// Crée un axe en HTML.
        /// </summary>
        /// <param name="hyperCube">Cube.</param>
        /// <param name="context">Contexte.</param>
        /// <param name="writer">Writer HTML.</param>
        /// <param name="requestName">Nom de la requête.</param>
        /// <param name="activeCounters">Compteurs actifs.</param>
        /// <param name="odd">Parité.</param>
        /// <param name="axis">Axe.</param>
        /// <param name="criteria">Critères.</param>
        /// <param name="cube">Cube.</param>
        private static void CreateHtmlAxis(IHyperCube hyperCube, Context context, HtmlTextWriter writer, string requestName, Dictionary<string, string> activeCounters, bool odd, string axis, CounterCubeCriteria criteria, ICube cube) {
            writer.Write(HtmlPageRenderer.Indent);
            writer.WriteLine("<tr " + (odd ? "class=\"odd\"" : string.Empty) + '>');

            writer.Write(HtmlPageRenderer.Indent2);
            writer.Write("<td class=\"monitoring\">");

            Context localContext = context.Zoom(hyperCube.Name, axis);
            writer.Write(HtmlPageRenderer.GetLink(localContext, axis, !axis.Equals(requestName)));
            writer.WriteLine("</td>");

            writer.Write(HtmlPageRenderer.Indent2);
            writer.Write("<td class=\"monitoring\" align=\"right\">");
            double total = cube.GetCounter(Analytics.ElapsedTime).GetValue(CounterStatType.Total);
            writer.Write(Convert.ToString(total, CultureInfo.CurrentCulture));
            writer.WriteLine("</td>");

            writer.Write(HtmlPageRenderer.Indent2);
            double avg = cube.GetCounter(Analytics.ElapsedTime).GetValue(CounterStatType.Avg);
            writer.Write("<td class=\"monitoring\" align=\"right\" "
                      + GetHtmlStyle(1000, 10000, avg) + '>');
            writer.Write(string.Format(CultureInfo.CurrentCulture, "{0:0.##}", avg));
            writer.WriteLine("</td>");

            writer.Write(HtmlPageRenderer.Indent2);
            writer.Write("<td class=\"monitoring\" align=\"right\">");
            double hits = cube.GetCounter(Analytics.ElapsedTime).GetValue(CounterStatType.Hits);
            writer.Write(Convert.ToString(hits, CultureInfo.CurrentCulture));
            writer.WriteLine("</td>");

            writer.Write(HtmlPageRenderer.Indent2);
            PrintChartSparkLines(hyperCube, new CounterCubeCriteria(axis, TimeLevel.Minute), writer, localContext);

            double midValue;
            foreach (ICounterDefinition counterDefinition in hyperCube.AllDefinitions) {
                if (activeCounters.ContainsKey(counterDefinition.Code)) {
                    ICounter counter = hyperCube.GetCube(criteria.CreateCubeKey(context.EndDate)).GetCounter(counterDefinition.Code);
                    midValue = (counter != null) ? counter.GetValue(CounterStatType.Avg) : 0;
                    writer.Write(HtmlPageRenderer.Indent2);
                    writer.Write("<td class=\"monitoring\" align=\"right\" "
                              + GetHtmlStyle(counterDefinition, midValue) + '>');
                    writer.Write(string.Format(CultureInfo.CurrentCulture, "{0:0.##}", midValue));
                    writer.WriteLine("</td>");
                }
            }

            writer.Write(HtmlPageRenderer.Indent);
            writer.WriteLine("</tr>");
        }

        /// <summary>
        /// Retourne un style html ou chaîne vide selon le niveau de criticité de
        /// la valeur en entrée.
        /// </summary>
        /// <param name="counter">Définition du compteur.</param>
        /// <param name="value">Valeur.</param>
        /// <returns>Style.</returns>
        private static string GetHtmlStyle(ICounterDefinition counter, double value) {
            return GetHtmlStyle(counter.WarningThreshold, counter.CriticalThreshold, value);
        }

        /// <summary>
        /// Retourne un style html ou chaîne vide selon le niveau de criticité de
        /// la valeur en entrée.
        /// </summary>
        /// <param name="warningThreshold">Seuil d'alerte.</param>
        /// <param name="criticalThreshold">Seuil critique.</param>
        /// <param name="value">Valeur.</param>
        /// <returns>Style.</returns>
        private static string GetHtmlStyle(long warningThreshold, long criticalThreshold, double value) {
            if (criticalThreshold >= 0 && value > criticalThreshold) {
                return "style=\"font-weight: bold; color:#FF0000;\"";
            }

            if (warningThreshold >= 0 && value > warningThreshold) {
                return "style=\"font-weight: bold; color:#FF9900;\"";
            }

            if (criticalThreshold >= 0 || warningThreshold >= 0) {
                return "style=\"font-weight: bold; color:#009900;\"";
            }

            return string.Empty;
        }

        /// <summary>
        /// Réalise le rendu HTML contenant les sparklines du nombre de hits sur les
        /// 10 dernières minutes.
        /// </summary>
        /// <param name="hyperCube">Hypercube contenant les valeurs.</param>
        /// <param name="criteria">Critère de rendu (axe courant, date, période de temps).</param>
        /// <param name="writer">Writer HTML.</param>
        /// <param name="context">Context courant de la requête.</param>
        private static void PrintChartSparkLines(IHyperCube hyperCube, CounterCubeCriteria criteria, HtmlTextWriter writer, Context context) {
            string url = context.ChangeSort(null).GetHandlerUrl() + "&CONTENT=sparklines.png&MON_ID=" + hyperCube.Name + "&LEVEL=" + criteria.Level;
            writer.Write("<td class=\"monitoring\" align=\"right\">");
            writer.Write("<img src=\"" + url + "\"/>");
            writer.WriteLine("</td>");
        }

        /// <summary>
        /// Indique si l'axe d'un hypercube doit être affiché pour un niveau d'alerte.
        /// </summary>
        /// <param name="endDate">Date de fin.</param>
        /// <param name="filtre">Niveau d'alerte (NONE, WARNING, ERROR).</param>
        /// <param name="hyperCube">Hypercube contenant les données.</param>
        /// <param name="criteria">Critère de lecture de l'hypercube.</param>
        /// <returns>True si l'axe doit être affiché.</returns>
        private static bool IsShown(DateTime endDate, string filtre, IHyperCube hyperCube, CounterCubeCriteria criteria) {

            bool showIt = false;
            if ("NONE".Equals(filtre)) {
                showIt = true;
            } else {
                bool showWarning = "WARNING".Equals(filtre);
                ICube cube = hyperCube.GetCube(criteria.CreateCubeKey(endDate));
                if (cube == null) {
                    return false;
                }

                foreach (ICounterDefinition counterDefinition in hyperCube.AllDefinitions) {
                    ICounter counter = cube.GetCounter(counterDefinition.Code);
                    if (counter == null) {
                        continue;
                    }

                    // Par défaut, on n'affiche que le niveau critique
                    double midValue = counter.GetValue(CounterStatType.Avg);
                    showIt = (counterDefinition.CriticalThreshold >= 0 && midValue > counterDefinition.CriticalThreshold) ||
                            (showWarning && counterDefinition.WarningThreshold >= 0 && midValue > counterDefinition.WarningThreshold);
                    if (showIt) {
                        break;
                    }
                }
            }

            return showIt;
        }

        /// <summary>
        /// Tableau de synthèse d'un cube de données.
        /// Présentation des compteurs (Compteur, dernière valeur, moyenne, minimum....)
        /// relatifs au cube de données.
        /// Il s'agit des données étendues.
        /// </summary>
        /// <param name="hyperCube">Hypercube contenant les données.</param>
        /// <param name="cube">Cube courant.</param>
        /// <param name="writer">Writer HTML.</param>
        /// <param name="link">Indique si un lien doit être créé pour la selection des graphiques.</param>
        /// <param name="context">Contexte courant de la requête.</param>
        private static void RenderCube(IHyperCube hyperCube, ICube cube, HtmlTextWriter writer, bool link, Context context) {
            if (cube == null) {
                throw new ArgumentNullException("cube");
            }

            ICounter requestCounter = cube.GetCounter(Analytics.ElapsedTime);
            long hits = (long)requestCounter.GetValue(CounterStatType.Hits);
            long firstMilliSeconds = cube.FirstHitMsec;
            long lastMilliSeconds = cube.LastHitMsec;

            // Informations sur le cube
            writer.Write(hits);
            writer.Write(" hits depuis " + Convert.ToString(new DateTime(firstMilliSeconds * 10000), TimeLevel.Second.CreateFormatProvider())); // OK
            if (firstMilliSeconds != lastMilliSeconds) {
                writer.Write(", soit ");
                writer.Write(hits * 1000 * 60 / (lastMilliSeconds - firstMilliSeconds));
                writer.WriteLine(" hits/mn");
            }

            // Informations sur les compteurs
            writer.WriteLine("<table cellspacing=\"0\" cellpadding=\"3\" border=\"0\" width=\"100%\">");
            writer.Write(HtmlPageRenderer.Indent);
            writer.Write("<tr>");
            writer.Write(HtmlPageRenderer.Indent2);
            writer.WriteLine("<th>Compteur</th>");
            writer.Write(HtmlPageRenderer.Indent2);
            writer.WriteLine("<th>Dernière valeur</th>");
            writer.Write(HtmlPageRenderer.Indent2);
            writer.WriteLine("<th>Moyenne</th>");
            writer.Write(HtmlPageRenderer.Indent2);
            writer.WriteLine("<th colspan=\"2\">Minimum</th>");
            writer.Write(HtmlPageRenderer.Indent2);
            writer.WriteLine("<th colspan=\"2\">Maximum</th>");

            writer.Write(HtmlPageRenderer.Indent);
            writer.WriteLine("</tr>");

            bool odd = true;

            foreach (ICounterDefinition counterDefinition in hyperCube.AllDefinitions) {
                ICounter counter = cube.GetCounter(counterDefinition.Code);
                if (counter != null && counter.GetValue(CounterStatType.Hits) > 0) {
                    writer.Write(HtmlPageRenderer.Indent);
                    writer.Write("<tr ");
                    writer.WriteLine((odd ? "class=\"odd\"" : string.Empty) + '>');
                    writer.Write(HtmlPageRenderer.Indent2);
                    writer.WriteLine("<td class=\"monitoring\">" + counterDefinition.Label + "</td>");
                    ToHtml(hyperCube, writer, cube.GetCounter(counterDefinition.Code), counterDefinition, link, context);
                    writer.Write(HtmlPageRenderer.Indent);
                    writer.WriteLine("</tr>");
                    odd = !odd;
                }
            }

            writer.WriteLine("</table>");
        }

        /// <summary>
        /// Réalise le rendu des labels.
        /// </summary>
        /// <param name="writer">Writer.</param>
        /// <param name="hyperCube">Hypercube contenant les valeurs.</param>
        /// <param name="context">Contexte de la requête.</param>
        /// <param name="requestName">Element demandé.</param>
        /// <returns>Liste des compteurs à rendre.</returns>
        private static Dictionary<string, string> RenderLabels(TextWriter writer, IHyperCube hyperCube, Context context, string requestName) {
            bool renderToHtml = writer is HtmlTextWriter;
            Dictionary<string, string> activeCounters = new Dictionary<string, string>();
            ICube globalCube = hyperCube.GetCube(new CounterCubeCriteria(requestName, TimeLevel.Hour).CreateCubeKey(context.EndDate));
            if (globalCube != null) {
                foreach (ICounterDefinition counterDefinition in hyperCube.AllDefinitions) {
                    ICounter counter = globalCube.GetCounter(counterDefinition.Code);
                    if (counter != null && counter.GetValue(CounterStatType.Hits) > 0) {
                        activeCounters.Add(counterDefinition.Code, counterDefinition.Code);

                        if (renderToHtml) {
                            writer.Write(HtmlPageRenderer.Indent2);
                            writer.Write("<th>");
                            string newSort = counterDefinition.Code;

                            // Le lien n'est actif que si le tri n'est pas déjà effectué
                            writer.Write(HtmlPageRenderer.GetLink(context.ChangeSort(newSort), counterDefinition.Label, !newSort.Equals(context.Sort)));
                            writer.WriteLine("</th>");
                        } else {
                            writer.Write(counterDefinition.Label);
                            writer.Write(";");
                        }
                    }
                }

                writer.WriteLine();
            }

            return activeCounters;
        }

        /// <summary>
        /// Rend un compteur sous forme HTML.
        /// </summary>
        /// <param name="hyperCube">Hypercube contenant les données.</param>
        /// <param name="writer">Writer HTML.</param>
        /// <param name="counter">Compteur.</param>
        /// <param name="counterDefinition">Définition du compteur.</param>
        /// <param name="link">Indique si un lien doit être créé pour la selection des graphiques.</param>
        /// <param name="context">Context courant de la requête.</param>
        private static void ToHtml(IHyperCube hyperCube, HtmlTextWriter writer, ICounter counter, ICounterDefinition counterDefinition, bool link, Context context) {
            StatToHtml(hyperCube, writer, counter, counterDefinition, link, context, CounterStatType.Last);
            StatToHtml(hyperCube, writer, counter, counterDefinition, link, context, CounterStatType.Avg);
            StatToHtml(hyperCube, writer, counter, counterDefinition, link, context, CounterStatType.Min);
            StatToHtml(hyperCube, writer, counter, counterDefinition, link, context, CounterStatType.Max);
        }

        /// <summary>
        /// Rend une statistique au format HTML.
        /// </summary>
        /// <param name="hyperCube">Hypercube contenant les données.</param>
        /// <param name="writer">Writer HTML.</param>
        /// <param name="counter">Compteur.</param>
        /// <param name="counterDefinition">Définition du compteur.</param>
        /// <param name="link">Indique si un lien doit être créé pour la selection des graphiques.</param>
        /// <param name="context">Contexte courant de la requête.</param>
        /// <param name="counterStatType">Type de statistique du compteur à rendre.</param>
        private static void StatToHtml(IHyperCube hyperCube, HtmlTextWriter writer, ICounter counter, ICounterDefinition counterDefinition, bool link, Context context, CounterStatType counterStatType) {
            writer.Write(HtmlPageRenderer.Indent2);
            writer.Write("<td class=\"monitoring\" align=\"right\" " + GetHtmlStyle(counterDefinition, counter.GetValue(counterStatType)) + '>');
            writer.Write(string.Format(CultureInfo.CurrentCulture, "{0:0.##}", counter.GetValue(counterStatType)));
            writer.WriteLine("</td>");

            if (counter.HasInfo(counterStatType)) {
                writer.Write(HtmlPageRenderer.Indent2);
                writer.Write("<td class=\"monitoring\">");
                writer.Write(HtmlPageRenderer.GetLink(context.Zoom(hyperCube.Name, counter.GetInfo(counterStatType)), counter.GetInfo(counterStatType), link));
                writer.WriteLine("</td>");
            }
        }
    }
}
