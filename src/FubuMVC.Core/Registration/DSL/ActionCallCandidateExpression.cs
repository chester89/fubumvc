using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FubuCore;
using FubuCore.Util;

namespace FubuMVC.Core.Registration.DSL
{
    public class ActionCallCandidateExpression
    {
        private readonly ConfigGraph _configuration;

        public ActionCallCandidateExpression(ConfigGraph configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Find Actions on classes that end on 'Controller' from the main application assembly
        /// </summary>
        public ActionCallCandidateExpression IncludeClassesSuffixedWithController()
        {
            return FindBy(x => x.IncludeClassesSuffixedWithController());
        }

        /// <summary>
        /// Create an adhoc policy for discovering actions 
        /// </summary>
        /// <param name="configuration"></param>
        public ActionCallCandidateExpression FindBy(Action<ActionSource> configuration)
        {
            var source = new ActionSource();
            configuration(source);

            return FindWith(source);
        }


        /// <summary>
        /// Find actions on the specified type
        /// </summary>
        public ActionCallCandidateExpression IncludeType<T>()
        {
            return FindWith(new SingleTypeActionSource(typeof (T), new ActionMethodFilter()));
        }

        /// <summary>
        /// Find actions through an <see cref="IActionSource"/> instance.
        /// </summary>
        public ActionCallCandidateExpression FindWith<T>() where T : IActionSource, new()
        {
            return FindWith(new T());
        }

        /// <summary>
        /// Find actions with the provided <see cref="IActionSource"/> instance.
        /// </summary>
        public ActionCallCandidateExpression FindWith(IActionSource actionSource)
        {
            _configuration.Add(actionSource);
            return this;
        }

        /// <summary>
        /// Finds actions in concrete classes that are suffixed with either "Endpoint" or
        /// "Endpoints" in the main application assembly
        /// </summary>
        /// <returns></returns>
        public ActionCallCandidateExpression IncludeClassesSuffixedWithEndpoint()
        {
            return FindWith(new EndpointActionSource());
        }
    }
}