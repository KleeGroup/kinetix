using System;
using System.IO;
using System.Web;
using Kinetix.Monitoring.Counter;
using Kinetix.Monitoring.Manager;

namespace Kinetix.Monitoring.Html {
    /// <summary>
    /// Manager pour le suivi des requêtes HTTP.
    /// </summary>
    public sealed class RequestManager : IManager, IManagerDescription {
        /// <summary>
        /// Nom du compteur d'exceptions système.
        /// </summary>
        public const string CounterExceptionSystemCount = "EXCEPTION_SYSTEM_COUNT";

        /// <summary>
        /// Nom du compteur d'exceptions utilisateur.
        /// </summary>
        public const string CounterExceptionUserCount = "EXCEPTION_USER_COUNT";

        /// <summary>
        /// Nom du compteur de taille de page.
        /// </summary>
        public const string CounterHtmlSize = "HTML_SIZE";

        /// <summary>
        /// Nom du compteur de taille de viewState.
        /// </summary>
        public const string CounterViewStateSize = "VIEWSTATE_SIZE";

        /// <summary>
        /// Base de données gérant les requêtes.
        /// </summary>
        public const string RequestHyperCube = "REQUESTDB";

        /// <summary>
        /// Constructeur.
        /// </summary>
        public RequestManager() {
            Analytics.Instance.CreateCounter("Exception système (%)", CounterExceptionSystemCount, 0, 10, 50);
            Analytics.Instance.CreateCounter("Exception utilisateur (%)", CounterExceptionUserCount, 50, 100, 60);
            Analytics.Instance.CreateCounter("Taille page (o)", CounterHtmlSize, 30000, 100000, 70);
            Analytics.Instance.CreateCounter("Taille ViewState (o)", CounterViewStateSize, 30000, 100000, 75);
            Analytics.Instance.OpenDataBase(RequestHyperCube, this);
        }

        /// <summary>
        /// Retourne la taille de la réponse.
        /// </summary>
        public static long ResponseLength {
            get {
                ResponseSizeFilter filter = HttpContext.Current.Response.Filter as ResponseSizeFilter;
                return (filter == null) ? 0 : filter.WriteBytes;
            }
        }

        /// <summary>
        /// Nom du manager.
        /// </summary>
        public string Name {
            get {
                return "Statistiques des requêtes";
            }
        }

        /// <summary>
        /// Retourne un objet décrivant le service.
        /// </summary>
        public IManagerDescription Description {
            get {
                return this;
            }
        }

        /// <summary>
        /// Image du manager.
        /// </summary>
        public string Image {
            get {
                return "ruler.png";
            }
        }

        /// <summary>
        /// Image.
        /// </summary>
        byte[] IManagerDescription.ImageData {
            get {
                return IR.Ruler_png;
            }
        }

        /// <summary>
        /// Priorité d'affichage du manager.
        /// </summary>
        int IManagerDescription.Priority {
            get {
                return 10;
            }
        }

        /// <summary>
        /// Type mime de l'image.
        /// </summary>
        public string ImageMimeType {
            get {
                return "image/png";
            }
        }

        /// <summary>
        /// Pose un hook sur la réponse.
        /// </summary>
        public static void InitResponseFilter() {
            if (HttpContext.Current.Request.Path.EndsWith(".aspx", StringComparison.Ordinal)) {
                HttpContext.Current.Response.Filter = new ResponseSizeFilter(HttpContext.Current.Response.Filter);
            }
        }

        /// <summary>
        /// Extension de la méthode toString().
        /// Permet à chaque Manager de présenter son propre état.
        /// </summary>
        /// <param name="writer">Writer HTML.</param>
        public void ToHtml(System.Web.UI.HtmlTextWriter writer) {
            HtmlPageRenderer.ToHtml(RequestHyperCube, writer);
        }

        /// <summary>
        /// Libération des ressources consommées par le manager lors du undeploy.
        /// Exemples : connexions, thread, flux.
        /// </summary>
        public void Close() {
        }

        /// <summary>
        /// Stream de calcul de la taille du flux de réponse.
        /// </summary>
        private class ResponseSizeFilter : Stream {
            private readonly Stream _responseFilter;
            private long _writeBytes;

            /// <summary>
            /// Crée une nouvelle instance.
            /// </summary>
            /// <param name="responseFilter">Filtre initial de la réponse.</param>
            public ResponseSizeFilter(Stream responseFilter) {
                _responseFilter = responseFilter;
            }

            /// <summary>
            /// Indique si le flux peut être lu.
            /// </summary>
            public override bool CanRead {
                get {
                    return _responseFilter.CanRead;
                }
            }

            /// <summary>
            /// Indique si le flux peut être accédé par bloc.
            /// </summary>
            public override bool CanSeek {
                get {
                    return _responseFilter.CanSeek;
                }
            }

            /// <summary>
            /// Indique si le flux peut être écrit.
            /// </summary>
            public override bool CanWrite {
                get {
                    return _responseFilter.CanRead;
                }
            }

            /// <summary>
            /// Retourne le nombre d'octets écrit dans le flux.
            /// </summary>
            public override long Length {
                get {
                    return _responseFilter.Length;
                }
            }

            /// <summary>
            /// Retourne le nombre d'octets écrit dans le flux.
            /// </summary>
            public long WriteBytes {
                get {
                    return _writeBytes;
                }
            }

            /// <summary>
            /// Obtient ou définit la position dans le flux.
            /// </summary>
            public override long Position {
                get {
                    return _responseFilter.Position;
                }

                set {
                    _responseFilter.Position = value;
                }
            }

            /// <summary>
            /// Lit le flux.
            /// </summary>
            /// <param name="buffer">Buffer de lecture.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Taille.</param>
            /// <returns>Nb octets lu.</returns>
            public override int Read(byte[] buffer, int offset, int count) {
                return _responseFilter.Read(buffer, offset, count);
            }

            /// <summary>
            /// Seek.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="origin">Origine.</param>
            /// <returns>Position.</returns>
            public override long Seek(long offset, SeekOrigin origin) {
                return _responseFilter.Seek(offset, origin);
            }

            /// <summary>
            /// Vide le flux.
            /// </summary>
            public override void Flush() {
                _responseFilter.Flush();
            }

            /// <summary>
            /// Définit la longueur.
            /// </summary>
            /// <param name="value">Longueur.</param>
            public override void SetLength(long value) {
                _responseFilter.SetLength(value);
            }

            /// <summary>
            /// Ecrit dans le flux.
            /// </summary>
            /// <param name="buffer">Buffer d'entrée.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Nombre d'octets.</param>
            public override void Write(byte[] buffer, int offset, int count) {
                _responseFilter.Write(buffer, offset, count);
                _writeBytes += count;
            }
        }
    }
}
