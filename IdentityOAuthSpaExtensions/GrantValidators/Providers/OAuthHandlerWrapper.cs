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
            if (_authHandler is OpenIdConnectHandler idConnectHandler)
            {
                var url = await BuildChallengeUrlForIdConnect(idConnectHandler, properties, redirectUri);
                return url;
            }

            _authHandler.ChallengeAsync(properties).GetAwaiter().GetResult();
            var method = _authHandler.GetType()
                .GetMethod("BuildChallengeUrl", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (string) method.Invoke(_authHandler, new object[] {properties, redirectUri});
            return result;
        }

        private async Task<string> BuildChallengeUrlForIdConnect(OpenIdConnectHandler idConnectHandler,
            AuthenticationProperties properties, string redirectUri)
        {
            var configuration =
                await idConnectHandler.Options.ConfigurationManager.GetConfigurationAsync(
                    new CancellationToken());
            var idConnectMessage = new OpenIdConnectMessage();
            idConnectMessage.ClientId = idConnectHandler.Options.ClientId;
            idConnectMessage.EnableTelemetryParameters = !idConnectHandler.Options.DisableTelemetry;
            idConnectMessage.IssuerAddress = configuration?.AuthorizationEndpoint ?? string.Empty;
            idConnectMessage.RedirectUri = redirectUri;
            idConnectMessage.Resource = idConnectHandler.Options.Resource;
            idConnectMessage.ResponseType = idConnectHandler.Options.ResponseType;
            idConnectMessage.Prompt = properties.GetParameter<string>("prompt") ?? idConnectHandler.Options.Prompt;
            idConnectMessage.Scope = string.Join(" ",
                (IEnumerable<string>) (properties.GetParameter<ICollection<string>>("scope") ??
                                       idConnectHandler.Options.Scope));
            if (idConnectHandler.Options.ProtocolValidator.RequireNonce)
            {
                idConnectMessage.Nonce = idConnectHandler.Options.ProtocolValidator.GenerateNonce();
                //idConnectHandler.WriteNonceCookie(idConnectMessage2.Nonce);
            }
            return idConnectMessage.CreateAuthenticationRequestUrl();
        }

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