using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.ComponentActivator;
using Castle.MicroKernel.Context;
using FubuMVC.Core.Behaviors;

namespace FubuMVC.Windsor
{
    public static class WindsorExtensions
    {
        public static void Register(this IKernel kernel, Type serviceType, ConfiguredInstance configuredInstance)
        {
            IDictionary extendedProperties = new Dictionary<string, object>
        {
            {"configuredInstance", configuredInstance}
        };
            ComponentModel model = kernel.ComponentModelBuilder.BuildModel(
                new ComponentName(configuredInstance.Name, true),
                new [] { serviceType },
                configuredInstance.PluggedType,
                extendedProperties
                );
            model.LifestyleType = HttpRuntime.AppDomainAppVirtualPath != null
                                    ? LifestyleType.PerWebRequest
                                    : LifestyleType.Transient;
            model.CustomComponentActivator = typeof(ConfiguredInstanceActivator);
            model.InspectionBehavior = PropertiesInspectionBehavior.All;
            ((IKernelInternal)kernel).AddCustomComponent(model);
        }
    }

    public class ConfiguredInstanceActivator: AbstractComponentActivator
    {
        public ConfiguredInstanceActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction) : base(model, kernel, onCreation, onDestruction)
        {
        }

        protected override object InternalCreate(CreationContext context)
        {
            var configuredInstance = Model.ExtendedProperties["configuredInstance"] as ConfiguredInstance;
            if (configuredInstance == null)
                throw new InvalidOperationException("configuredInstance was not defined in ExtendedProperties");

            var reflection = Kernel.Resolve<IReflection>();
            //null out converter on creation context to enable it to offer up additional parameters as candidates
            //for dependency resolution
            reflection.SetConverterFieldToNull(context);

            configuredInstance.Dependencies.Each(
                x => { context.AdditionalArguments[x.Key] = x.Value.GetValue(Kernel, context.AdditionalArguments); });

            object instance = this.Create(context, null);
            SetUpProperties(instance, context);
            return instance;
        }

        protected override void InternalDestroy(object instance)
        {
            throw new NotImplementedException();
        }

        protected virtual void SetUpProperties(object instance, CreationContext context)
        {
            foreach (PropertySet property in Model.Properties)
            {
                object value = null;

                //IActionBehaviors are only allowed to come off the creation context, don't want to resolve any through default mechanism
                if (property.Dependency.TargetType == typeof(IActionBehavior))
                {
                    if (context.CanResolve(context, context.Handler, Model, property.Dependency))
                    {
                        value = context.Resolve(context, context.Handler, Model, property.Dependency);
                    }
                }
                else if (Kernel.Resolver.CanResolve(context, context.Handler, Model, property.Dependency))
                {
                    value = Kernel.Resolver.Resolve(context, context.Handler, Model, property.Dependency);
                }

                if (value == null) continue;

                MethodInfo setMethod = property.Property.GetSetMethod();

                try
                {
                    setMethod.Invoke(instance, new[] { value });
                }
                catch (Exception ex)
                {
                    String message = String.Format("Error setting property {0} on type {1}, Component id is {2}. See inner exception for more information.",
                            setMethod.Name, instance.GetType().FullName, Model.Name);
                    throw new ComponentActivatorException(message, ex, Model);
                }
            }
        }
    }

    public interface IReflection
    {
        void SetConverterFieldToNull(Object instance);
    }

    public class Reflection: IReflection
    {
        private readonly FieldInfo _creationContextConverterField = typeof(CreationContext).GetField("converter", BindingFlags.Instance | BindingFlags.NonPublic);

        public void SetConverterFieldToNull(object instance)
        {
            _creationContextConverterField.SetValue(instance, null);
        }
    }
}
