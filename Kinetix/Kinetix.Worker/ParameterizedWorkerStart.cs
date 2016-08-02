using System.Runtime.InteropServices;
using System.Threading;

namespace Kinetix.Worker {

    /// <summary>
    /// Delegate des fonctions de démarrage d'un worker.
    /// </summary>
    /// <param name="stoppingEvent">Handle d'arrêt.</param>
    /// <param name="obj">Paramètre d'entrée.</param>
    [ComVisible(false)]
    public delegate void ParameterizedWorkerStart(WaitHandle stoppingEvent, object obj);
}
