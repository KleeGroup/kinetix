using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper;
using MapTuple = System.Tuple<System.Type, System.Type>;

namespace Kinetix.ComponentModel {

    /// <summary>
    /// Moteur de beans.
    ///
    /// Publie des méthodes pour :
    /// - copier un bean dans un autre
    /// - cloner un bean.
    /// </summary>
    public static class BeanEngine {

        private static readonly BeanEngineCore _instance = new BeanEngineCore();

        /// <summary>
        /// Recopie un bean source dans un bean destination.
        /// </summary>
        /// <typeparam name="TSource">Type du bean source.</typeparam>
        /// <typeparam name="TDestination">Type du bean destination.</typeparam>
        /// <param name="source">Bean source.</param>
        /// <returns>Bean destination.</returns>
        public static TDestination Map<TSource, TDestination>(TSource source) {

            /* Vérifie que le mapping existe. */
            _instance.EnsureMapping<TSource, TDestination>();

            /* Exécute le mapping. */
            TDestination destination = Mapper.Map<TSource, TDestination>(source);

            return destination;
        }

        /// <summary>
        /// Recopie un bean source dans un bean destination.
        /// </summary>
        /// <typeparam name="TSource">Type du bean source.</typeparam>
        /// <typeparam name="TDestination">Type du bean destination.</typeparam>
        /// <param name="source">Bean source.</param>
        /// <param name="destination">Bean destination.</param>
        /// <returns>Bean destination.</returns>
        public static TDestination Map<TSource, TDestination>(TSource source, TDestination destination) {

            /* Vérifie que le mapping existe. */
            _instance.EnsureMapping<TSource, TDestination>();

            /* Exécute le mapping. */
            Mapper.Map<TSource, TDestination>(source, destination);

            return destination;
        }

        /// <summary>
        /// Définit un mapping avec une exception sur un membre.
        /// </summary>
        /// <typeparam name="TSource">Type du bean source.</typeparam>
        /// <typeparam name="TDestination">Type du bean destination.</typeparam>
        /// <param name="sourceMember">Obtient le membre de la source.</param>
        /// <param name="destinationMember">Obtient le membre de la destination.</param>
        public static void DefineMemberMapping<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember) {
            _instance.DefineMemberMapping(sourceMember, destinationMember);
        }

        /// <summary>
        /// Instance du singleton interne.
        /// </summary>
        private class BeanEngineCore {

            /// <summary>
            /// Verrou d'accès à _mapTupleSet.
            /// </summary>
            private static readonly object _setLock = new object();

            /// <summary>
            /// Ensemble des tuples de mapping déjà créés.
            /// </summary>
            private readonly HashSet<MapTuple> _mapTupleSet = new HashSet<MapTuple>();

            /// <summary>
            /// Créé le mapping entre deux types s'il n'existe pas déjà.
            /// </summary>
            /// <typeparam name="TSource">Type source.</typeparam>
            /// <typeparam name="TDestination">Type destination.</typeparam>
            public void EnsureMapping<TSource, TDestination>() {

                /* Tuple représenant le mapping de deux types. */
                var tuple = new MapTuple(typeof(TSource), typeof(TDestination));

                /* Lazy création du mapping. */
                lock (_setLock) {
                    if (!_mapTupleSet.Contains(tuple)) {

                        /* Création du mapping. */
                        Mapper.CreateMap<TSource, TDestination>();

                        /* Ajout du tuple dans le set. */
                        _mapTupleSet.Add(tuple);
                    }
                }
            }

            /// <summary>
            /// Définit un mapping avec une exception sur un membre.
            /// </summary>
            /// <typeparam name="TSource">Type du bean source.</typeparam>
            /// <typeparam name="TDestination">Type du bean destination.</typeparam>
            /// <param name="sourceMember">Obtient le membre de la source.</param>
            /// <param name="destinationMember">Obtient le membre de la destination.</param>
            public void DefineMemberMapping<TSource, TDestination>(Expression<Func<TSource, object>> sourceMember, Expression<Func<TDestination, object>> destinationMember) {
                /* Tuple représenant le mapping de deux types. */
                var tuple = new MapTuple(typeof(TSource), typeof(TDestination));

                /* Lazy création du mapping. */
                lock (_setLock) {
                    if (!_mapTupleSet.Contains(tuple)) {

                        /* Création du mapping. */
                        Mapper.CreateMap<TSource, TDestination>()
                            .ForMember(destinationMember, opt => opt.MapFrom(sourceMember));

                        /* Ajout du tuple dans le set. */
                        _mapTupleSet.Add(tuple);
                    }
                }
            }
        }
    }
}
