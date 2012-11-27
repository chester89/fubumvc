using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FubuCore.Binding;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Runtime;

namespace FubuMVC.Windsor
{
    public class WindsorServiceFactory: IServiceFactory
    {
        public IActionBehavior BuildBehavior(ServiceArguments arguments, Guid behaviorId)
        {
            throw new NotImplementedException();
        }

        public T Get<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAll<T>()
        {
            throw new NotImplementedException();
        }
    }
}
