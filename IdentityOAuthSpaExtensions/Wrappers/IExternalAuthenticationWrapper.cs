using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace IdentityOAuthSpaExtensions.Wrappers
{
    public interface IExternalAuthenticationWrapper
    {
        Task<AuthenticationTicket> GetTicket(string code, string absoluteCallbackUri);
    }
}