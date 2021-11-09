using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityOAuthSpaExtensions.Wrappers;

namespace IdentityOAuthSpaExtensions.Services
{
    public class ExternalAuthService
    {
        private readonly ExternalAuthenticatorProvider _externalAuthenticatorProvider;

        public ExternalAuthService(ExternalAuthenticatorProvider externalAuthenticatorProvider)
        {
            _externalAuthenticatorProvider = externalAuthenticatorProvider;
        }

        public virtual async Task<string> GetExternalUserId(string provider, string code)
        {
            var userInfo = await GetExternalUserInfo(provider, code);
            return userInfo.Id;
        }

        public virtual async Task<ExternalUserInfo> GetExternalUserInfo(
            string provider,
            string code
        )
        {
            var providerInstance = await _externalAuthenticatorProvider.GetAuthenticator(provider);
            var ticket = await providerInstance.GetTicket(code, "");

            var userIdClaim =
                ticket.Principal.FindFirst(JwtClaimTypes.Subject)
                ?? ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new Exception("Unknown userid");

            return new ExternalUserInfo
            {
                Id = userIdClaim.Value,
                ProviderName = provider,
                Ticket = ticket,
            };
        }
    }
}
