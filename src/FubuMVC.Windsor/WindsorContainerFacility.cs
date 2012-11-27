using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using FubuMVC.Core.Bootstrapping;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuMVC.Core.Runtime;

namespace FubuMVC.Windsor
{
    public class WindsorContainerFacility: IContainerFacility
    {
        private readonly IWindsorContainer _container;

        public WindsorContainerFacility(IWindsorContainer container)
        {
            _container = container;
        }

        public IServiceFactory BuildFactory()
        {
            throw new NotImplementedException();
        }

        public void Register(Type serviceType, ObjectDef def)
        {
            throw new NotImplementedException();
        }

        public void Inject(Type abstraction, Type concretion)
        {
            throw new NotImplementedException();
        }
    }
}
