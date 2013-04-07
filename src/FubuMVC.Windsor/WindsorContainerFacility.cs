using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FubuCore;
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

        public WindsorContainerFacility(IWindsorContainer container)
        {
            _container = container;
            _installer = new WindsorFubuInstaller();
        }

        public IServiceFactory BuildFactory()
        {
            _container.Install(_installer);
            return this;
        }

        public void Register(Type serviceType, ObjectDef def)
        {
            var component = Component.For(serviceType).ImplementedBy(def.Type);
            
            if (ServiceRegistry.ShouldBeSingleton(serviceType) || ServiceRegistry.ShouldBeSingleton(def.Type) || def.IsSingleton)
            {
                component.LifestyleSingleton();
            }
            else
            {
                component.LifestyleTransient();
            }
            if (!def.Name.IsEmpty())
            {
                component = component.Named(def.Name);
            }
            _container.Register(component);
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

        public void Dispose()
        {
            _container.Dispose();
        }
    }
}
