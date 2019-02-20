using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace IdentityOAuthSpaExtensions.GrantValidators.Providers
{
    public interface IExternalAuthenticator
    {
        Task<AuthenticationTicket> CreateTicketAsync(
            ClaimsIdentity identity,
            AuthenticationProperties properties,
            OAuthTokenResponse tokens);

        Task<OAuthTokenResponse> ExchangeCodeAsync(string code, string redirectUrl);
        Task<string> BuildChallengeUrl(AuthenticationProperties properties, string redirectUri);
        ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; }
    }
}