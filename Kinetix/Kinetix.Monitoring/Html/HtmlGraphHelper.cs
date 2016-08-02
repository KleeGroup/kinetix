using System;
using System.Drawing;
using System.IO;
using Kinetix.Monitoring.Counter;

namespace Kinetix.Monitoring.Html {
    /// <summary>
    /// Helper pour le rendu des graph et sparklines.
    /// </summary>
    internal static class HtmlGraphHelper {
        /// <summary>
        /// Réalise le rendu d'un cude.
        /// </summary>
        /// <param name="hyperCube">HyperCupe à rendre.</param>
        /// <param name="context">Context courant de la requête.</param>
        /// <param name="s">Stream de sortie.</param>
        internal static void RenderGraph(IHyperCube hyperCube, Context context, Stream s) {
            TimeLevel level = TimeLevel.ValueOf(context.Level);
            CounterCubeCriteria criteria = new CounterCubeCriteria(context.RequestName, level);

            if (context.Content.Equals("sparklines.png")) {
                HtmlGraphHelper.RenderGraphSparklines(context, hyperCube, criteria, s);
            } else {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Rendu d'un hypercube en sparklines.
        /// </summary>
        /// <param name="context">Context courant de la requête.</param>
        /// <param name="hyperCube">Hypercube contenant les données.</param>
        /// <param name="criteria">Critères de lecture de l'hypercube.</param>
        /// <param name="s">Stream de sortie.</param>
        private static void RenderGraphSparklines(Context context, IHyperCube hyperCube, CounterCubeCriteria criteria, Stream s) {
            long now = context.EndDate.Ticks;
            long timeStampInterval = criteria.Level.TimeStampInterval;

            decimal[] datas = new decimal[10];

            for (int i = 0; i < datas.Length; i++) {
                DateTime d = new DateTime(now - (i * timeStampInterval * 10000000));
                ICube cube = hyperCube.GetCube(criteria.CreateCubeKey(d));
                datas[datas.Length - 1 - i] = (cube == null) ? 0 : (decimal)cube.GetCounter(Analytics.ElapsedTime).GetValue(CounterStatType.Hits);
            }

            SparklinesBar sparklines = new SparklinesBar();
            sparklines.CreateChart(s, datas, Color.Blue, Color.Magenta);
        }
    }
}
