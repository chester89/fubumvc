using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;
using FubuCore;

namespace FubuMVC.Windsor
{
    public class WindsorServiceLocator: IServiceLocator
    {
        private readonly IWindsorContainer _container;

        public WindsorServiceLocator(IWindsorContainer container)
        {
            _container = container;
        }

        public T GetInstance<T>()
        {
            return _container.Resolve<T>();
        }

        public object GetInstance(Type type)
        {
            return _container.Resolve(type);
        }

        public T GetInstance<T>(string name)
        {
            return _container.Resolve<T>(name);
        }
    }
}
