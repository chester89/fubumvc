using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Routing;
using Bottles;
using Bottles.Diagnostics;
using Bottles.Environment;
using FubuCore;
using FubuCore.Binding;
using FubuMVC.Core.Bootstrapping;
using FubuMVC.Core.Http;
using FubuMVC.Core.Packaging;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.ObjectGraph;
using FubuMVC.Core.Routing;
using FubuMVC.Core.Runtime;

namespace FubuMVC.Core
{
    public interface IApplicationSource
    {
        FubuApplication BuildApplication();
    }

    public class FubuRuntime
    {
        private readonly IContainerFacility _facility;
        private readonly IServiceFactory _factory;
        private readonly IList<RouteBase> _routes;

        public FubuRuntime(IServiceFactory factory, IContainerFacility facility, IList<RouteBase> routes)
        {
            _factory = factory;
            _facility = facility;
            _routes = routes;
        }

        public IServiceFactory Factory
        {
            get { return _factory; }
        }

        public IContainerFacility Facility
        {
            get { return _facility; }
        }

        public IList<RouteBase> Routes
        {
            get { return _routes; }
        }
    }

    // PLEASE NOTE:  This code is primarily tested with the StoryTeller suite for Packaging
    public class FubuApplication : IContainerFacilityExpression
    {
        private readonly Lazy<IContainerFacility> _facility;
        private readonly List<Action<IPackageFacility>> _packagingDirectives = new List<Action<IPackageFacility>>();
        private readonly Lazy<FubuRegistry> _registry;
        private readonly List<Action<FubuRegistry>> _registryModifications = new List<Action<FubuRegistry>>();
        private Func<IContainerFacility> _facilitySource;
        private FubuMvcPackageFacility _fubuFacility;

        private FubuApplication(Func<FubuRegistry> registryBuilder)
        {
            _registry = new Lazy<FubuRegistry>(registryBuilder);
            _facility = new Lazy<IContainerFacility>(() => _facilitySource());
        }

        public IContainerFacility Facility
        {
            get
            {
                if (!_facility.IsValueCreated)
                {
                    throw new InvalidOperationException(
                        "Application has not yet been bootstrapped.  This operation is only valid after bootstrapping the application");
                }


                return _facility.Value;
            }
        }

        FubuApplication IContainerFacilityExpression.ContainerFacility(IContainerFacility facility)
        {
            return registerContainerFacilitySource(() => facility);
        }

        FubuApplication IContainerFacilityExpression.ContainerFacility(Func<IContainerFacility> facilitySource)
        {
            return registerContainerFacilitySource(facilitySource);
        }

        private FubuApplication registerContainerFacilitySource(Func<IContainerFacility> facilitySource)
        {
            _facilitySource = facilitySource;
            return this;
        }

        public static IContainerFacilityExpression For(Func<FubuRegistry> registry)
        {
            return new FubuApplication(registry);
        }

        public static IContainerFacilityExpression For(FubuRegistry registry)
        {
            return new FubuApplication(() => registry);
        }

        public static IContainerFacilityExpression For<T>() where T : FubuRegistry, new()
        {
            return For(() => new T());
        }


        [SkipOverForProvenance]
        public FubuRuntime Bootstrap()
        {
            SetupNamingStrategyForHttpHeaders();

            _fubuFacility = new FubuMvcPackageFacility();

            IServiceFactory factory = null;
            BehaviorGraph graph = null;

            // TODO -- I think Bottles probably needs to enforce a "tell me the paths"
            // step maybe
            PackageRegistry.GetApplicationDirectory = FubuMvcPackageFacility.GetApplicationPath;
            BottleFiles.ContentFolder = FubuMvcPackageFacility.FubuContentFolder;
            BottleFiles.PackagesFolder = FileSystem.Combine("bin", FubuMvcPackageFacility.FubuPackagesFolder);


            PackageRegistry.LoadPackages(x =>
            {
                x.Facility(_fubuFacility);
                _packagingDirectives.Each(d => d(x));


                x.Bootstrap(log =>
                {
                    // container facility has to be spun up here
                    var containerFacility = _facility.Value;

                    // Need to do this to make the provenance for bottles come out right
                    _registry.Value.Config.Pop();

                    applyRegistryModifications();

                    applyFubuExtensionsFromPackages();

                    graph = buildBehaviorGraph();

                    bakeBehaviorGraphIntoContainer(graph, containerFacility);

                    // factory HAS to be spun up here.
                    factory = containerFacility.BuildFactory();

                    return factory.GetAll<IActivator>();
                });
            });

            FubuMvcPackageFacility.Restarted = DateTime.Now;

            PackageRegistry.AssertNoFailures(() => {
                throw new FubuException(0, FubuApplicationDescriber.WriteDescription());
            });

            var routes = buildRoutes(factory, graph);
            routes.Each(r => RouteTable.Routes.Add(r));

            return new FubuRuntime(factory, _facility.Value, routes);
        }

        public static void SetupNamingStrategyForHttpHeaders()
        {
            BindingContext.AddNamingStrategy(HttpRequestHeaders.HeaderDictionaryNameForProperty);
        }

        private void bakeBehaviorGraphIntoContainer(BehaviorGraph graph, IContainerFacility containerFacility)
        {
            graph.As<IRegisterable>().Register(containerFacility.Register);

            // Important to register itself
            containerFacility.Register(typeof (IContainerFacility), ObjectDef.ForValue(containerFacility));
        }

        private BehaviorGraph buildBehaviorGraph()
        {
            var graph = BehaviorGraphBuilder.Build(_registry.Value);

            return graph;
        }

        private void applyRegistryModifications()
        {
            _registryModifications.Each(m => m(_registry.Value));
        }

        private IList<RouteBase> buildRoutes(IServiceFactory factory, BehaviorGraph graph)
        {
            var routes = new List<RouteBase>();

            // Build route objects from route definitions on graph + add packaging routes
            factory.Get<IRoutePolicy>().BuildRoutes(graph, factory).Each(routes.Add);

            return routes;
        }

        private void applyFubuExtensionsFromPackages()
        {
            PackageRegistry.Diagnostics.EachLog((o, l) => {
                if (o is IPackageInfo)
                {
                    _registry.Value.Config.Push(o.As<IPackageInfo>());
                    var assemblies = l.FindChildren<Assembly>();

                    try
                    {
                        FubuExtensionFinder.ApplyExtensions(_registry.Value, assemblies);
                    }
                    catch (Exception e)
                    {
                        l.MarkFailure(e);
                    }

                    _registry.Value.Config.Pop();
                }
            });

        }

        public FubuApplication Packages(Action<IPackageFacility> configure)
        {
            _packagingDirectives.Add(configure);
            return this;
        }

        public FubuApplication ModifyRegistry(Action<FubuRegistry> modifications)
        {
            _registryModifications.Add(modifications);
            return this;
        }

    }

    public interface IContainerFacilityExpression
    {
        FubuApplication ContainerFacility(IContainerFacility facility);
        FubuApplication ContainerFacility(Func<IContainerFacility> facilitySource);
    }
}