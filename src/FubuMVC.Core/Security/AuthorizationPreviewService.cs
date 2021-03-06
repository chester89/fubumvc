using System;
using System.Linq.Expressions;
using System.Reflection;
using FubuCore;
using FubuCore.Reflection;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.Querying;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Urls;

namespace FubuMVC.Core.Security
{

    public interface IChainAuthorizor
    {
        AuthorizationRight Authorize(BehaviorChain chain, object model);
    }

    public class ChainAuthorizor : IChainAuthorizor
    {
        private readonly IFubuRequest _request;
        private readonly IServiceLocator _services;
        private readonly ITypeResolver _types;

        public ChainAuthorizor(IFubuRequest request, IServiceLocator services, ITypeResolver types)
        {
            _request = request;
            _services = services;
            _types = types;
        }


        public AuthorizationRight Authorize(BehaviorChain chain, object model)
        {
            if (model != null)
            {
                _request.Set(_types.ResolveType(model), model);
            }

            var endpoint = _services.GetInstance<IEndPointAuthorizor>(chain.UniqueId.ToString());
            return endpoint.IsAuthorized(_request);
        }
    }

    public class AuthorizationPreviewService : ChainInterrogator<AuthorizationRight>, IAuthorizationPreviewService
    {
        private readonly IChainAuthorizor _authorizor;

        public AuthorizationPreviewService(IChainResolver resolver, IChainAuthorizor authorizor) : base(resolver)
        {
            _authorizor = authorizor;
        }

        protected override AuthorizationRight createResult(object model, BehaviorChain chain)
        {
            return _authorizor.Authorize(chain, model);
        }

        public bool IsAuthorized(object model)
        {
            return For(model) == AuthorizationRight.Allow;
        }

        public bool IsAuthorized(object model, string category)
        {
            return For(model, category) == AuthorizationRight.Allow;
        }

        public bool IsAuthorized<TController>(Expression<Action<TController>> expression)
        {
            return IsAuthorized(typeof (TController), ReflectionHelper.GetMethod(expression));
        }

        public bool IsAuthorizedForNew<T>()
        {
            return IsAuthorizedForNew(typeof (T));
        }

        public bool IsAuthorizedForNew(Type entityType)
        {
            return forNew(entityType) == AuthorizationRight.Allow;
        }
        
        public bool IsAuthorized(Type handlerType, MethodInfo method)
        {
            return For(handlerType, method) == AuthorizationRight.Allow;
        }
    }
}