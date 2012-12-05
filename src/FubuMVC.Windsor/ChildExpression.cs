using System;
using System.Collections;
using System.Text;
using Castle.MicroKernel;

namespace FubuMVC.Windsor
{
    public class ChildExpression
    {
        private readonly Type _serviceType;
        private ConfiguredInstance _objectDefInstance;
        private object _value;

        public ChildExpression(Type serviceType)
        {
            _serviceType = serviceType;
        }

        public Type ServiceType
        {
            get { return _serviceType; }
        }

        public void Is(object value)
        {
            _value = value;
        }

        public void Is(ConfiguredInstance objectDefInstance)
        {
            _objectDefInstance = objectDefInstance;
        }

        public object GetValue(IKernel kernel, IDictionary additionalParameters)
        {
            if (_value != null)
            {
                return _value;
            }
            var key = _objectDefInstance.Name;
            if (!kernel.HasComponent(key))
            {
                kernel.Register(_serviceType, _objectDefInstance);
            }
            return kernel.Resolve(key, _serviceType, additionalParameters);
        }
    }
}
