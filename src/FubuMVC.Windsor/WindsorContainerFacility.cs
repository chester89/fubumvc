using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FubuCore.Binding;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Bootstrapping;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuMVC.Core.Runtime;

namespace FubuMVC.Windsor
{
    public class WindsorContainerFacility: IContainerFacility, IServiceFactory
    {
        private readonly IWindsorContainer _container;
        private readonly IWindsorInstaller _installer;
        private Action<Type, ObjectDef> _registration;

        public WindsorContainerFacility(IWindsorContainer container)
        {
            _container = container;
            container.Kernel.ComponentModelBuilder.AddContributor(new SingletonEqualizer());
            _installer = new WindsorFubuInstaller();
            _registration = (serviceType, definition) =>
                                {
                                    //if (definition.Value == null)
                                    //{
                                    //    //_registry.For(serviceType).Add(new ObjectDefInstance(definition));
                                    //    _installer.
                                    //}
                                    //else
                                    //{
                                    //    _registry.For(serviceType).Add(new ObjectInstance(definition.Value)
                                    //    {
                                    //        Name = definition.Name
                                    //    });
                                    //}

                                    if (ServiceRegistry.ShouldBeSingleton(serviceType) || ServiceRegistry.ShouldBeSingleton(definition.Type) || definition.IsSingleton)
                                    {
                                        container.Register(Component.For(serviceType).LifestyleSingleton());
                                    }
                                };
        }

        public IServiceFactory BuildFactory()
        {
            _container.Install(_installer);

            _registration = (serviceType, def) =>
            {
                if (def.Value != null)
                {
                    _container.Register(Component.For(serviceType).Instance(def.Value));
                }
                else
                {
                    //_container.Configure(x => x.For(serviceType).Add(new ObjectDefInstance(def)));
                    //_container.Register(Component.For(serviceType)
                    //    .Instance(new WindsorActivator(_container.Kernel.ComponentModelBuilder., _container.Kernel, null, null)));
                }


            };

            return this;
        }

        public void Register(Type serviceType, ObjectDef def)
        {
            _registration(serviceType, def);
        }

        public void Inject(Type abstraction, Type concretion)
        {
            _container.Register(Component.For(abstraction).ImplementedBy(concretion));
        }

        public IActionBehavior BuildBehavior(ServiceArguments arguments, Guid behaviorId)
        {
            throw new NotImplementedException();
        }

        public T Get<T>()
        {
            return _container.Resolve<T>();
        }

        public IEnumerable<T> GetAll<T>()
        {
            return _container.ResolveAll<T>();
        }
    }
}
