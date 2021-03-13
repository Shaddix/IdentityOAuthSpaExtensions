using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

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
                .GetMethod("HandleRemoteAuthenticateAsync",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (Task<HandleRequestResult>) method.Invoke(_authHandler, new object[] { });
            return await result;
        }

        public virtual async Task<AuthenticationTicket> GetTicket(string code,
            string absoluteCallbackUri)
        {
            StubRequest(code);
            var result = await HandleRemoteAuthenticateAsync();
            if (!result.Succeeded)
            {
                throw new Exception("Authentication failed", result.Failure);
            }

            return result.Ticket;
        }

        protected virtual void StubRequest(string body)
        {
            var currentRequest = _httpContextAccessor.HttpContext.Request;

            var context = new DefaultHttpContext();
            var request = context.Request;
            var cookies =
                currentRequest.Cookies.ToDictionary(x => x.Key, x => x.Value);
            request.Cookies = new RequestCookieCollection(cookies);
            request.Host = currentRequest.Host;
            request.Scheme = currentRequest.Scheme;
            request.PathBase = "";
            if (body.StartsWith("?"))
            {
                request.Method = "GET";
                request.QueryString = QueryString.FromUriComponent(body);
            }
            else
            {
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                NameValueCollection formDataNameValueCollection =
                    HttpUtility.ParseQueryString(body);
                Dictionary<string, StringValues> formDataDictionary =
                    new Dictionary<string, StringValues>();
                foreach (string key in formDataNameValueCollection.AllKeys)
                {
                    formDataDictionary[key] = formDataNameValueCollection[key];
                }

                request.Form = new FormCollection(formDataDictionary);
            }

            _authHandler.SetHttpContext(context);
        }

        public TOptions Options => _authHandler.Options;
    }
}