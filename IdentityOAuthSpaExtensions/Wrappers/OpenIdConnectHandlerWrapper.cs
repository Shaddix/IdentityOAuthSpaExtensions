using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;

namespace IdentityOAuthSpaExtensions.Wrappers
{
    public class OpenIdConnectHandlerWrapper : RemoteAuthenticationHandlerWrapper<OpenIdConnectOptions>
    {
        public OpenIdConnectHandlerWrapper(RemoteAuthenticationHandler<OpenIdConnectOptions> authHandler,
            IHttpContextAccessor httpContextAccessor) : base(authHandler, httpContextAccessor)
        {
        }
    }
}