using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.ComponentActivator;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.ModelBuilder;

namespace FubuMVC.Windsor
{
    public class WindsorActivator: DefaultComponentActivator
    {
        public WindsorActivator(ComponentModel model, IKernel kernel, ComponentInstanceDelegate onCreation, ComponentInstanceDelegate onDestruction)
            : base(model, kernel, onCreation, onDestruction)
        {
        }

        protected override object InternalCreate(CreationContext context)
        {
            return base.InternalCreate(context);
        }

        protected override void InternalDestroy(object instance)
        {
            throw new NotImplementedException();
        }
    }

    public class SingletonEqualizer: IContributeComponentModelConstruction
    {
        public void ProcessModel(IKernel kernel, ComponentModel model)
        {
            model.LifestyleType = LifestyleType.Singleton;
        }
    }
}
