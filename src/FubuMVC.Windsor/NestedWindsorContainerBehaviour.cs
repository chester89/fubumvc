using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FubuCore.Binding;
using FubuMVC.Core.Behaviors;

namespace FubuMVC.Windsor
{
    public class NestedWindsorContainerBehavior : IActionBehavior, IDisposable
    {
        private readonly ServiceArguments _arguments;
        private readonly Guid _behaviorId;
        private readonly IWindsorContainer _container;
        private IWindsorContainer _nested;

        public NestedWindsorContainerBehavior(IWindsorContainer container, ServiceArguments arguments, Guid behaviorId)
        {
            _container = container;
            _arguments = arguments;
            _behaviorId = behaviorId;
        }

        public void Invoke()
        {
            StartInnerBehavior().Invoke();
        }

        public IActionBehavior StartInnerBehavior()
        {
            _nested = _container.GetChildContainer("default");
            _arguments.EachService((type, value) => _nested.Register(Component.For(type).Instance(value)));
            var behavior = _nested.Resolve<IActionBehavior>(_behaviorId.ToString());
            return behavior;
        }

        public void InvokePartial()
        {
            // This should never be called
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            _nested.Dispose();
        }
    }
}
