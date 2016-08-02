using System.Collections.Generic;

namespace Kinetix.Monitoring.Counter {
    /// <summary>
    /// Représente un graphe de points.
    /// </summary>
    internal sealed class HyperCubeRamCounterDataStore {
        private readonly int _maxValueCount;
        private readonly LinkedList<CubeKey> _keyList;
        private readonly IDictionary<CubeKey, Cube> _datas;
        private readonly TimeLevel _level;
        private readonly string _axis;
        private readonly string _databaseName;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="axis">Axe.</param>
        /// <param name="databaseName">Nom de la base de données.</param>
        /// <param name="level">TimeLevel.</param>
        /// <param name="maxValueCount">Capacity.</param>
        internal HyperCubeRamCounterDataStore(string axis, string databaseName, TimeLevel level, int maxValueCount) {
            this._axis = axis;
            this._databaseName = databaseName;
            this._level = level;
            this._maxValueCount = maxValueCount;

            _datas = new Dictionary<CubeKey, Cube>(maxValueCount);
            _keyList = new LinkedList<CubeKey>();
        }

        /// <summary>
        /// Ajoute une valeur au store.
        /// </summary>
        /// <param name="process">Processus.</param>
        internal void AddValue(CounterProcess process) {
            CubeKey key = new CubeKey(process.Date, _axis, _level);
            lock (this) {
                Cube cube = GetCube(key);
                if (cube == null) {
                    cube = new Cube(key, _databaseName);
                    _datas.Add(key, cube);
                    _keyList.AddLast(key);
                    if (_datas.Count > _maxValueCount) {
                        CubeKey firstKey = _keyList.First.Value;
                        _keyList.RemoveFirst();
                        _datas.Remove(firstKey);
                    }
                }

                cube.AddValue(process);
            }
        }

        /// <summary>
        /// Exécute le néttoyage.
        /// </summary>
        /// <param name="collection">Liste des cubes modifiés depuis le dernier garbage.</param>
        internal void RunStorage(ICollection<Cube> collection) {
            foreach (Cube cube in _datas.Values) {
                if (cube.IsExpired && cube.IsModified) {
                    collection.Add(cube);
                    cube.IsModified = false;
                }
            }
        }

        /// <summary>
        /// Retourne un cube.
        /// </summary>
        /// <param name="key">Clef.</param>
        /// <returns>Cube.</returns>
        internal Cube GetCube(CubeKey key) {
            Cube cube;
            _datas.TryGetValue(key, out cube);
            return cube;
        }
    }
}
