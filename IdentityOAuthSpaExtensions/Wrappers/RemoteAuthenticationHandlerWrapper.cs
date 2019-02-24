using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace IdentityOAuthSpaExtensions.Wrappers
{
    public class RemoteAuthenticationHandlerWrapper<TOptions> : IExternalAuthenticationWrapper
        where TOptions : RemoteAuthenticationOptions, new()
    {
        private readonly RemoteAuthenticationHandler<TOptions> _authHandler;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public RemoteAuthenticationHandlerWrapper(RemoteAuthenticationHandler<TOptions> authHandler,
            IHttpContextAccessor httpContextAccessor)
        {
            _authHandler = authHandler;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            var method = _authHandler.GetType()
                .GetMethod("HandleRemoteAuthenticateAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (Task<HandleRequestResult>) method.Invoke(_authHandler, new object[] { });
            return await result;
        }

        public virtual async Task<string> BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var httpContext = new DefaultHttpContext()
            {
                Request =
                {
                    Scheme = request.Scheme,
                    Host = request.Host,
                }
            };
            _authHandler.SetHttpContext(httpContext);

            var oldOptions = _authHandler.Options;
            var newOptions = _authHandler.Options.MemberwiseClone();
            var newCallbackPath = new Uri(redirectUri).PathAndQuery;
            newOptions.CallbackPath = newCallbackPath;
            var openIdConnectOptions = newOptions as OpenIdConnectOptions;
            if (openIdConnectOptions != null)
            {
                openIdConnectOptions.ResponseType = OpenIdConnectResponseType.CodeIdToken;
            }

            _authHandler.SetOptions(newOptions);

            await _authHandler.ChallengeAsync(properties);

            var url = httpContext.Response.Headers["Location"].ToString();
            var cookies = httpContext.Response.Headers["Set-Cookie"].ToString();
            cookies = cookies.Replace(oldOptions.CallbackPath.ToString(), "/");
            _httpContextAccessor.HttpContext.Response.Headers.Add("Set-Cookie", cookies);
            return url;
        }

        public ISecureDataFormat<AuthenticationProperties> StateDataFormat =>
            (ISecureDataFormat<AuthenticationProperties>) ((dynamic) Options).StateDataFormat;

        public virtual async Task<AuthenticationTicket> GetTicket(string code, string absoluteCallbackUri)
        {
            StubRequest(code, absoluteCallbackUri);
            var result = await HandleRemoteAuthenticateAsync();
            if (!result.Succeeded)
            {
                throw new Exception("Authentication failed", result.Failure);
            }

            return result.Ticket;
        }

        protected virtual void StubRequest(string code, string absoluteCallbackUrl)
        {
            var context = new DefaultHttpContext();
            var request = (DefaultHttpRequest) context.Request;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            var xsrfValue = Guid.NewGuid().ToString();
            var authenticationProperties = new AuthenticationProperties(new Dictionary<string, string>()
            {
                {".xsrf", xsrfValue},
                {OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, absoluteCallbackUrl}
            });
            var xsrfCookieName = Options.CorrelationCookie.Name + _authHandler.Scheme.Name + "." + xsrfValue;

            var cookies =
                _httpContextAccessor.HttpContext.Request.Cookies.ToDictionary(x => x.Key, x => x.Value);
            cookies[xsrfCookieName] = "N";
            request.Cookies = new RequestCookieCollection(cookies);

            request.Form = new FormCollection(new Dictionary<string, StringValues>()
            {
                {"code", code},
                {"state", StateDataFormat.Protect(authenticationProperties)},
            });
            _authHandler.SetHttpContext(context);
        }

        public TOptions Options => _authHandler.Options;
        
        public virtual AuthenticationProperties Unprotect(string state)
        {
            return StateDataFormat.Unprotect(state);
        }
    }
}