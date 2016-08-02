using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Kinetix.Monitoring.Html {
    /// <summary>
    /// Sparklines sous forme de barres.
    /// </summary>
    internal sealed class SparklinesBar : SparklinesAbstract {
        /// <summary>
        /// Constructeur.
        /// </summary>
        internal SparklinesBar()
            : base() {
        }

        /// <summary>
        /// Type Mime de l'image.
        /// </summary>
        protected internal override string MimeType {
            get {
                return "image/png";
            }
        }

        /// <summary>
        /// Création d'une image du sparklines.
        /// </summary>
        /// <param name="s">Stream.</param>
        /// <param name="data">Array of Number Objects to graph.</param>
        /// <param name="highColor">Color for above average data points (or null).</param>
        /// <param name="lastColor">Color for last data point (or null).</param>
        internal override void CreateChart(Stream s, decimal[] data, Color highColor, Color lastColor) {
            using (Bitmap bitmap = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb)) {
                if (data != null && data.Length > 0) {
                    using (Graphics g = Graphics.FromImage(bitmap)) {
                        float d = SparklinesAbstract.GetDivisor(data, this.Height);
                        int a = SparklinesAbstract.GetAvg(data);
                        int w = (this.Width - (this.Spacing * data.Length)) / data.Length;

                        int x = 0;
                        int y = 0;
                        int c = 0;

                        Brush mainBrush = new SolidBrush(this.MainColor);

                        foreach (decimal n in data) {
                            int h = (int)((float)n / d);
                            Brush brush;
                            if (c == (data.Length - 1) && lastColor != null) {
                                brush = new SolidBrush(lastColor);
                            } else if ((int)n < a || (highColor == null)) {
                                brush = mainBrush;
                            } else {
                                brush = new SolidBrush(highColor);
                            }

                            g.FillRectangle(brush, x, y + (this.Height - h), w, (int)n / d);
                            x += w + this.Spacing;
                            c++;
                        }
                    }
                }

                MemoryStream m = new MemoryStream();
                bitmap.Save(m, ImageFormat.Png);
                byte[] buffer = m.ToArray();
                s.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
