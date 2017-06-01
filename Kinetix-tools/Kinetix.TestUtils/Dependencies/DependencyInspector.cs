using System;
using System.Collections;
using System.Linq;
using Castle.MicroKernel;
using Castle.MicroKernel.ModelBuilder.Inspectors;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Microsoft.Practices.Unity;

namespace Kinetix.TestUtils.Dependencies {

    /// <summary>
    /// Inspecteur de dépendances.
    /// Permet de détecter les dépendances circulaires.
    /// Usage :
    ///  - instancier un inspecteur
    ///  - enregistrer les dépendances
    ///  - vérifier la résolution d'un type donné.
    /// </summary>
    public class DependencyInspector {

        private readonly WindsorContainer _container;

        private DependencyInspector() {
            _container = CreateContainer();
        }

        public static DependencyInspector New() {
            return new DependencyInspector();
        }

        public DependencyInspector RegisterFromUnity(IUnityContainer unityContainer) {

            /* Recopie les mapping de Unity dans le conteneur Windsor. */
            foreach (var register in unityContainer.Registrations) {
                var name = register.RegisteredType + ":" + register.MappedToType;
                _container.Register(Component.For(register.RegisteredType).ImplementedBy(register.MappedToType).Named(name));
            }

            return this;
        }

        public void CheckDependency<T>() {
            this.CheckDependency(typeof(T));
        }

        public void CheckDependency(Type t) {
            _container.Resolve(t);
        }

        private WindsorContainer CreateContainer() {
            var kernel = new DefaultKernel();

            /* Ajout un chargeur pour les classes concrètes. */
            kernel.Register(Component.For<ILazyComponentLoader>().ImplementedBy<ConcreteClassLoader>());

            /* Désactive l'injection des propriétés. */
            kernel.ComponentModelBuilder.RemoveContributor(
                kernel.ComponentModelBuilder.Contributors.OfType<PropertiesDependenciesModelInspector>().Single());

            var container = new WindsorContainer(kernel, new ZeroInstaller());
            return container;
        }

        public class ConcreteClassLoader : ILazyComponentLoader {

            public IRegistration Load(string name, Type service, IDictionary arguments) {
                if (service == null) {
                    return null;
                }

                if (!service.IsInterface && !service.IsAbstract) {
                    return Component.For(service).Named(name + Guid.NewGuid());
                }

                return null;
            }
        }

        public class ZeroInstaller : IComponentsInstaller {

            public void SetUp(IWindsorContainer container, IConfigurationStore store) {
                // RAS
            }
        }
    }
}
