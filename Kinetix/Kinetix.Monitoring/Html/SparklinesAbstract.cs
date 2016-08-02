using System;
using System.Drawing;
using System.IO;

namespace Kinetix.Monitoring.Html {
    /// <summary>
    /// Abstraction des sparklines.
    /// </summary>
    internal abstract class SparklinesAbstract {
        private readonly int _width;
        private readonly int _height;
        private readonly int _spacing;
        private readonly Color _mainColor;

        /// <summary>
        /// Constructeur.
        /// </summary>
        protected SparklinesAbstract()
            : this(50, 12, 1, Color.DarkGray) {
        }

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="width">Largeur de l'image (en pixels).</param>
        /// <param name="height">Hauteur de l'image (en pixels).</param>
        /// <param name="spacing">Espace entre les éléments (en pixels).</param>
        /// <param name="mainColor">Couleur principale du graphique.</param>
        private SparklinesAbstract(int width, int height, int spacing, Color mainColor) {
            this._height = height;
            this._mainColor = mainColor;
            this._spacing = spacing;
            this._width = width;
        }

        /// <summary>
        /// Type Mime de l'image.
        /// </summary>
        protected internal abstract string MimeType {
            get;
        }

        /// <summary>
        /// Couleur principale du graphique.
        /// </summary>
        protected Color MainColor {
            get {
                return _mainColor;
            }
        }

        /// <summary>
        /// Hauteur de l'image (en pixels).
        /// </summary>
        protected int Height {
            get {
                return _height;
            }
        }

        /// <summary>
        /// Largeur de l'image (en pixels).
        /// </summary>
        protected int Width {
            get {
                return _width;
            }
        }

        /// <summary>
        /// Espace entre les éléments (en pixels).
        /// </summary>
        protected int Spacing {
            get {
                return _spacing;
            }
        }

        /// <summary>
        /// Création d'une image du sparklines.
        /// </summary>
        /// <param name="s">Stream.</param>
        /// <param name="data">Array of Number Objects to graph.</param>
        /// <param name="highColor">Color for above average data points (or null).</param>
        /// <param name="lastColor">Color for last data point (or null).</param>
        internal abstract void CreateChart(Stream s, decimal[] data, Color highColor, Color lastColor);

        /// <summary>
        /// Retourne la valeur moyenne d'un tableau de valeur.
        /// </summary>
        /// <param name="data">Liste des valeurs à moyenner.</param>
        /// <returns>Moyenne.</returns>
        protected static int GetAvg(decimal[] data) {
            int total = 0;
            foreach (decimal d in data) {
                total += (int)d;
            }

            return total / data.Length;
        }

        /// <summary>
        /// Retourne le diviseur à appliquer pour faire rentrer toutes
        /// les valeurs dans la hauteur heigth du graphique.
        /// </summary>
        /// <param name="data">Liste des données à afficher.</param>
        /// <param name="height">Hauteur du graphique.</param>
        /// <returns>Diviseur à appliquer sur les valeurs.</returns>
        protected static float GetDivisor(decimal[] data, int height) {
            float max = float.MinValue;
            float min = float.MaxValue;
            foreach (decimal d in data) {
                min = Math.Min(min, (float)d);
                max = Math.Max(max, (float)d);
            }

            if (max <= min) {
                return 1;
            }

            return (max - min) / (height - 1);
        }
    }
}
