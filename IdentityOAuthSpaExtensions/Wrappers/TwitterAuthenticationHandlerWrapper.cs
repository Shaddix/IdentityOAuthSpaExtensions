using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace IdentityOAuthSpaExtensions.Wrappers
{
    public class TwitterAuthenticationHandlerWrapper : RemoteAuthenticationHandlerWrapper<TwitterOptions>
    {
        public class CodeContainer
        {
            public string OAuthToken { get; set; }
            public string OAuthVerifier { get; set; }
        }

        private readonly TwitterHandler _authHandler;

        public TwitterAuthenticationHandlerWrapper(TwitterHandler authHandler,
            IHttpContextAccessor httpContextAccessor) : base(authHandler, httpContextAccessor)
        {
            _authHandler = authHandler;
        }

        public override AuthenticationProperties Unprotect(string state)
        {
            return Options.StateDataFormat.Unprotect(state).Properties;
        }

        protected override void StubRequest(string code, string absoluteCallbackUrl)
        {
            var deserializedToken = JsonConvert.DeserializeObject<CodeContainer>(code);

            var context = new DefaultHttpContext();
            var request = context.Request;
            var authenticationProperties = new AuthenticationProperties(new Dictionary<string, string>()
            {
                {OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, absoluteCallbackUrl}
            });

            var state = new Microsoft.AspNetCore.Authentication.Twitter.RequestToken()
            {
                Token = deserializedToken.OAuthToken,
                TokenSecret = deserializedToken.OAuthVerifier,
                CallbackConfirmed = true,
                Properties = authenticationProperties,
            };
            request.Cookies = new RequestCookieCollection(new Dictionary<string, string>()
            {
                {Options.StateCookie.Name, Options.StateDataFormat.Protect(state)}
            });
            request.Query = new QueryCollection(new Dictionary<string, StringValues>()
            {
                {"oauth_token", deserializedToken.OAuthToken},
                {"oauth_verifier", deserializedToken.OAuthVerifier},
            });
//            request.Form = new FormCollection(new Dictionary<string, StringValues>()
//            {
//                {"state", Options.StateDataFormat.Protect(state)},
//            });
            _authHandler.SetHttpContext(context);
        }
    }
}