using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace IdentityOAuthSpaExtensions.GrantValidators.Providers
{
    public class OAuthHandlerWrapper : IExternalAuthenticator
    {
        private readonly IAuthenticationHandler _authHandler;

        public OAuthHandlerWrapper(IAuthenticationHandler authHandler)
        {
            _authHandler = authHandler;
        }

        public async Task<AuthenticationTicket> CreateTicketAsync(
            ClaimsIdentity identity,
            AuthenticationProperties properties,
            OAuthTokenResponse tokens)
        {
            var method = _authHandler.GetType()
                .GetMethod("CreateTicketAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var result =
                (Task<AuthenticationTicket>) method.Invoke(_authHandler, new object[] {identity, properties, tokens});
            return await result;
        }

        public async Task<OAuthTokenResponse> ExchangeCodeAsync(string code, string redirectUrl)
        {
            var method = _authHandler.GetType()
                .GetMethod("ExchangeCodeAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (Task<OAuthTokenResponse>) method.Invoke(_authHandler, new object[] {code, redirectUrl});
            return await result;
        }

        public virtual async Task<string> BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            var method = _authHandler.GetType()
                .GetMethod("BuildChallengeUrl", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (string) method.Invoke(_authHandler, new object[] {properties, redirectUri});
            return result;
        }

        public ISecureDataFormat<AuthenticationProperties> StateDataFormat => Options.StateDataFormat;

        public OAuthOptions Options
        {
            get
            {
                var property = _authHandler.GetType().GetProperty("Options");
                var options = (OAuthOptions) property.GetMethod.Invoke(_authHandler, new object[] { });
                return options;
            }
        }
    }
}