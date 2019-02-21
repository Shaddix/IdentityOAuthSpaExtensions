using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace IdentityOAuthSpaExtensions.GrantValidators.Providers
{
    public interface IExternalAuthenticator
    {
        Task<string> BuildChallengeUrl(AuthenticationProperties properties, string redirectUri);
        ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; }
        Task<AuthenticationTicket> GetTicket(string code, string absoluteCallbackUri);
    }
}