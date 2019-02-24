using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityOAuthSpaExtensions.Wrappers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace IdentityOAuthSpaExtensions.Services
{
    public class ExternalAuthService
    {
        private readonly ExternalAuthenticatorProvider _externalAuthenticatorProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LinkGenerator _linkGenerator;

        public ExternalAuthService(ExternalAuthenticatorProvider externalAuthenticatorProvider,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator)
        {
            _externalAuthenticatorProvider = externalAuthenticatorProvider;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
        }

        public virtual async Task<string> GetChallengeLink(string provider, string returnUrl)
        {
            var absoluteCallbackUri = GetCallbackUrl(provider);

            var props = new AuthenticationProperties
            {
                RedirectUri = returnUrl,
                Items =
                {
                    {"returnUrl", absoluteCallbackUri},
                    {"scheme", provider},
                }
            };

            var providerInstance = await _externalAuthenticatorProvider.GetAuthenticator(provider);
            var uri = await providerInstance.BuildChallengeUrl(props, absoluteCallbackUri);

            return uri;
        }

        private string GetCallbackUrl(string provider)
        {
            var context = _httpContextAccessor.HttpContext;
            var url = _linkGenerator.GetPathByAction(context,
                nameof(ExternalAuthController.ChallengeCallbackGet),
                nameof(ExternalAuthController).Replace("Controller", ""),
                new
                {
                    provider = provider.ToLower()
                });
            var request = context.Request;

            var absoluteCallbackUri = new UriBuilder(request.Scheme, request.Host.Host, request.Host.Port ?? 80, url)
                .ToString();
            return absoluteCallbackUri;
        }


        public virtual async Task<AuthenticationProperties> Unprotect(string provider, string state)
        {
            var providerInstance = await _externalAuthenticatorProvider.GetAuthenticator(provider);
            var authOptions = providerInstance.Unprotect(state);
            return authOptions;
        }

        public virtual async Task<string> GetExternalUserId(string provider, string code)
        {
            var userInfo = await GetExternalUserInfo(provider, code);
            return userInfo.Id;
        }

        public virtual async Task<ExternalUserInfo> GetExternalUserInfo(string provider, string code)
        {
            var absoluteCallbackUri = GetCallbackUrl(provider);

            var providerInstance = await _externalAuthenticatorProvider.GetAuthenticator(provider);
            var ticket = await providerInstance.GetTicket(code, absoluteCallbackUri);
           
            var userIdClaim = ticket.Principal.FindFirst(JwtClaimTypes.Subject) ??
                              ticket.Principal.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            return new ExternalUserInfo
            {
                Id = userIdClaim.Value,
                ProviderName = provider,
                Ticket = ticket,
            };
        }
    }
}