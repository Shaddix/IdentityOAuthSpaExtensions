using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace IdentityOAuthSpaExtensions.Wrappers
{
    public interface IExternalAuthenticationWrapper
    {
        Task<string> BuildChallengeUrl(AuthenticationProperties properties, string redirectUri);
        AuthenticationProperties Unprotect(string state);
        Task<AuthenticationTicket> GetTicket(string code, string absoluteCallbackUri);
    }
}