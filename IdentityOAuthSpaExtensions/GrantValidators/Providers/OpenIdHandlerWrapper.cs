using System;
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
    public class OpenIdHandlerWrapper : IExternalAuthenticator
    {
        private readonly OpenIdConnectHandler _authHandler;

        public OpenIdHandlerWrapper(OpenIdConnectHandler authHandler)
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
                (Task<AuthenticationTicket>)method.Invoke(_authHandler, new object[] { identity, properties, tokens });
            return await result;
        }

        public async Task<OAuthTokenResponse> ExchangeCodeAsync(string code, string redirectUrl)
        {
            var method = _authHandler.GetType()
                .GetMethod("ExchangeCodeAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (Task<OAuthTokenResponse>)method.Invoke(_authHandler, new object[] { code, redirectUrl });
            return await result;
        }

        public virtual async Task<string> BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            var url = await BuildChallengeUrlForIdConnect(_authHandler, properties, redirectUri);
            return url;
        }

        public ISecureDataFormat<AuthenticationProperties> StateDataFormat => Options.StateDataFormat;

        private async Task<string> BuildChallengeUrlForIdConnect(OpenIdConnectHandler idConnectHandler,
            AuthenticationProperties properties, string redirectUri)
        {
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once LocalVariableHidesMember
            var Options = idConnectHandler.Options;

            var configuration =
                await idConnectHandler.Options.ConfigurationManager.GetConfigurationAsync(
                    new CancellationToken());

            var message = new OpenIdConnectMessage
            {
                ClientId = Options.ClientId,
                EnableTelemetryParameters = !Options.DisableTelemetry,
                IssuerAddress = configuration?.AuthorizationEndpoint ?? string.Empty,
                RedirectUri = redirectUri,
                Resource = Options.Resource,
                ResponseType = OpenIdConnectResponseType.CodeIdToken,
                Prompt = properties.GetParameter<string>(OpenIdConnectParameterNames.Prompt) ?? Options.Prompt,
                Scope = string.Join(" ", properties.GetParameter<ICollection<string>>(OpenIdConnectParameterNames.Scope) ?? Options.Scope),
            };
            if (!string.Equals(Options.ResponseType, OpenIdConnectResponseType.Code, StringComparison.Ordinal) ||
                !string.Equals(Options.ResponseMode, OpenIdConnectResponseMode.Query, StringComparison.Ordinal))
            {
                message.ResponseMode = Options.ResponseMode;
            }

            if (idConnectHandler.Options.ProtocolValidator.RequireNonce)
            {
                message.Nonce = idConnectHandler.Options.ProtocolValidator.GenerateNonce();
            }

            properties.Items.Add(OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, properties.RedirectUri);
            message.State = Options.StateDataFormat.Protect(properties);

            return message.CreateAuthenticationRequestUrl();
        }

        public OpenIdConnectOptions Options => _authHandler.Options;
    }
}