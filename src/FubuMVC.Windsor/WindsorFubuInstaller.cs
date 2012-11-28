using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using FubuCore;
using FubuMVC.Core.Runtime;

namespace FubuMVC.Windsor
{
    public class WindsorFubuInstaller: IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<HttpRequestWrapper>().UsingFactoryMethod(c => BuildRequestWrapper()).LifestyleTransient());
            container.Register(Component.For<HttpContextBase>()
                .ImplementedBy<HttpContextWrapper>().DependsOn(new Hashtable() { { "httpContext", BuildContextWrapper() } } ));
            container.Register(Component.For<IServiceLocator>().ImplementedBy<WindsorServiceLocator>().LifestyleTransient());
            container.Register(Component.For<ISessionState>().ImplementedBy<SimpleSessionState>().LifestyleTransient());
        }

        public HttpContext BuildContextWrapper()
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current;
                }
            }
            catch (HttpException)
            {
                
            }

            return null;
        }

        public static HttpRequestWrapper BuildRequestWrapper()
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    return new HttpRequestWrapper(HttpContext.Current.Request);
                }
            }
            catch (HttpException)
            {

            }

            return null;
        }
    }
}
