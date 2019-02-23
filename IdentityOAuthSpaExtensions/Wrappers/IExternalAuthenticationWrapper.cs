using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace IdentityOAuthSpaExtensions.Wrappers
{
    public interface IExternalAuthenticationWrapper
    {
        Task<string> BuildChallengeUrl(AuthenticationProperties properties, string redirectUri);
        ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; }
        Task<AuthenticationTicket> GetTicket(string code, string absoluteCallbackUri);
    }
}