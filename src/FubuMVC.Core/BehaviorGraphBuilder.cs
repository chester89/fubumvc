using System;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Diagnostics;
using System.Collections.Generic;
using FubuMVC.Core.Registration.Nodes;

namespace FubuMVC.Core
{
    internal static class BehaviorGraphBuilder
    {
        // Need to track the ConfigLog
        public static BehaviorGraph Import(FubuRegistry registry, BehaviorGraph parent, ConfigLog log)
        {
            var graph = BehaviorGraph.ForChild(parent);
            startBehaviorGraph(registry, graph);
            var config = registry.Config;

            config.RunActions(ConfigurationType.Settings, graph);
            config.RunActions(ConfigurationType.Discovery, graph);

            config.Imports.Each(import => import.ImportInto(graph, log));

            config.RunActions(ConfigurationType.Explicit, graph);
            config.RunActions(ConfigurationType.Policy, graph);
            config.RunActions(ConfigurationType.Reordering, graph);
            config.RunActions(ConfigurationType.Attributes, graph);
            config.RunActions(ConfigurationType.ModifyRoutes, graph);
            config.RunActions(ConfigurationType.InjectNodes, graph);
            config.RunActions(ConfigurationType.Conneg, graph);

            return graph;
        }

        public static BehaviorGraph Build(FubuRegistry registry)
        {
            var graph = new BehaviorGraph();
            startBehaviorGraph(registry, graph);
            var config = registry.Config;

            var log = new ConfigLog();
            graph.Services.AddService(log);
            log.Import(config);

            config.Add(new SystemServicesPack());
            config.Add(new DefaultConfigurationPack());

            // TODO -- log the provenance of all
            config.AllServiceRegistrations().Each(x => {
                x.Apply(graph.Services);
            });

            config.RunActions(ConfigurationType.Settings, graph);
            config.RunActions(ConfigurationType.Discovery, graph);

            config.UniqueImports().Each(import => import.ImportInto(graph, log));

            config.RunActions(ConfigurationType.Explicit, graph);
            config.RunActions(ConfigurationType.Policy, graph);
            config.RunActions(ConfigurationType.Attributes, graph);
            config.RunActions(ConfigurationType.ModifyRoutes, graph);
            config.RunActions(ConfigurationType.InjectNodes, graph);
            config.RunActions(ConfigurationType.Conneg, graph);
            config.RunActions(ConfigurationType.Attachment, graph);
            config.RunActions(ConfigurationType.Navigation, graph);
            config.RunActions(ConfigurationType.ByNavigation, graph);
            config.RunActions(ConfigurationType.Reordering, graph);
            config.RunActions(ConfigurationType.Instrumentation, graph);


            graph.Services.AddService(config);

            return graph;
        }


        // TODO -- try to eliminate the duplication from above
        public static IEnumerable<string> ConfigurationOrder()
        {
            return new string[]
                   {
                       ConfigurationType.Settings,
                       ConfigurationType.Discovery,
                       ConfigurationType.Explicit,
                       ConfigurationType.Policy,
                       ConfigurationType.Attributes,
                       ConfigurationType.ModifyRoutes,
                       ConfigurationType.InjectNodes,
                       ConfigurationType.Conneg,
                       ConfigurationType.Attachment,
                       ConfigurationType.Navigation,
                       ConfigurationType.ByNavigation,
                       ConfigurationType.Reordering,
                       ConfigurationType.Instrumentation
                   };
        } 

        

        private static void startBehaviorGraph(FubuRegistry registry, BehaviorGraph graph)
        {
            graph.ApplicationAssembly = registry.ApplicationAssembly;
            registry.Config.Add(new DiscoveryActionsConfigurationPack());
        }


    }
}