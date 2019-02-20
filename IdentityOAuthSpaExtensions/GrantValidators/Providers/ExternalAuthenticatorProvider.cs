using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace IdentityOAuthSpaExtensions.GrantValidators.Providers
{
    public class ExternalAuthenticatorProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOptionsMonitor<AuthenticationOptions> _authenticationOptionsMonitor;

        public ExternalAuthenticatorProvider(
            IHttpContextAccessor httpContextAccessor,
            IOptionsMonitor<AuthenticationOptions> authenticationOptionsMonitor)
        {
            _httpContextAccessor = httpContextAccessor;
            _authenticationOptionsMonitor = authenticationOptionsMonitor;
        }

        public async Task<IExternalAuthenticator> GetAuthenticator(string providerName)
        {
            var authOptions = _authenticationOptionsMonitor.CurrentValue;
            providerName = providerName.ToLower();
            var scheme = authOptions.Schemes.FirstOrDefault(x => x.Name.ToLower() == providerName);
            if (scheme == null)
            {
                throw new InvalidOperationException(
                    $"Auth Provider for '{providerName}' was not found");
            }

            var httpContext = _httpContextAccessor.HttpContext;
            var provider = httpContext.RequestServices.GetService(scheme.HandlerType);

            if (provider is IAuthenticationHandler authHandler)
            {
                await authHandler.InitializeAsync(
                    scheme.Build(),
                    httpContext);

                if (provider is PolicySchemeHandler policySchemeHandler)
                {
                    if (!string.IsNullOrEmpty(policySchemeHandler.Options.ForwardChallenge))
                    {
                        return await GetAuthenticator(policySchemeHandler.Options.ForwardChallenge);
                    }
                }

                if (IsOAuthHandler(authHandler))
                {
                    return new OAuthHandlerWrapper(authHandler);
                }

                if (authHandler is OpenIdConnectHandler openIdConnectHandler)
                {
                    return new OpenIdHandlerWrapper(openIdConnectHandler);
                }

                throw new InvalidOperationException(
                    $"Auth Provider is of invalid type '{provider.GetType()}'. Only OAuthHandler and OpenIdConnectHandler providers are supported");
            }

            throw new InvalidOperationException(
                $"Auth Provider is of invalid type '{provider.GetType()}'. Only IAuthenticationHandler providers are supported");
        }

        private bool IsOAuthHandler(IAuthenticationHandler provider)
        {
            return IsDescendantOf(provider, "Microsoft.AspNetCore.Authentication.OAuth.OAuthHandler");
        }
        private bool IsDescendantOf(object obj, string typeName)
        {
            var type = obj.GetType();
            while (type != null && !type.FullName.StartsWith(typeName))
                type = type.BaseType;

            if (type == null)
                return false;

            return true;
        }
    }
}