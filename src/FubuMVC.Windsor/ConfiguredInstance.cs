using System;
using System.Collections.Generic;
using System.Linq;
using FubuMVC.Core.Registration.ObjectGraph;

namespace FubuMVC.Windsor
{
    public class ConfiguredInstance: IDependencyVisitor
    {
        private readonly ObjectDef _def;
        private readonly IDictionary<Type, ChildExpression> _dependencies = new Dictionary<Type, ChildExpression>();

        public ConfiguredInstance(ObjectDef def)
        {
            _def = def;
        }

        public IDictionary<Type, ChildExpression> Dependencies
        {
            get { return _dependencies; }
        }

        public string Name { get; set; }

        public Type PluggedType
        {
            get { return _def.Type; }
        }

        public ChildExpression Child(Type serviceType)
        {
            var dependency = new ChildExpression(serviceType);
            _dependencies.Fill(serviceType, dependency);
            return dependency;
        }

        void IDependencyVisitor.Value(ValueDependency dependency)
        {
            Child(dependency.DependencyType).Is(dependency.Value);
        }

        void IDependencyVisitor.Configured(ConfiguredDependency dependency)
        {
            if (dependency.Definition.Value != null)
            {
                Child(dependency.DependencyType).Is(dependency.Definition.Value);
            }
            else
            {
                var child = new ConfiguredInstance(dependency.Definition);
                Child(dependency.DependencyType).Is(child);
            }
        }

        void IDependencyVisitor.List(ListDependency dependency)
        {
            var elements = dependency.Items.Select(instanceFor).ToArray();

            ChildArray(dependency.DependencyType).Contains(elements);
        }

        private ConfiguredInstance instanceFor(ObjectDef def)
        {
            return def.Value != null
                       ? (Instance)new ObjectInstance(def.Value)
                       : new ObjectDefInstance(def);
        }
    }
}