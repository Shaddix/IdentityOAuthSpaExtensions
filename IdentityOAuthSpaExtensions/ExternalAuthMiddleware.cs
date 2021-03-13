using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using IdentityOAuthSpaExtensions.Wrappers;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace IdentityOAuthSpaExtensions
{
    /// <summary>
    /// Handles Challenge ('/external-auth/challenge'),
    /// intermediate result page ('oauth-result.html') that sends Message to initial browser window
    /// and intercepts sign-in callbacks ('/signin-google', '/signin-twitter', etc.) 
    /// </summary>
    public class ExternalAuthMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// key is CallbackPath (e.g. '/signin-google' or '/signin-oidc'), value is ProviderName (e.g. 'Google', 'OpenIdConnect')
        /// </summary>
        private static Dictionary<string, string> _callbackPathToProviderNameDictionary = new();

        /// <summary>
        /// hash set to speed up checking whether provider exists in _callbackPathToProviderNameDictionary or not
        /// </summary>
        private static HashSet<string> _providersAddedToCallbackPath = new();

        /// <summary>
        /// If this cookie is present, we intercept requests to signin callbacks
        /// ('/signin-google', etc.).
        /// If it's not present, we don't intercept them, since it might be
        /// regular authentication over Identity UI
        /// </summary>
        private const string SpaAuthenticationCookieName = ".IdentityOauthSpa";

        public ExternalAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            HttpRequest request = context.Request;
            if (request.Path == "/external-auth/challenge")
            {
                await HandleChallenge(context);
                return;
            }

            if (request.Path.StartsWithSegments("/external-auth/oauth-result"))
            {
                await ReturnOAuthHtml(context);
                return;
            }

            if (_callbackPathToProviderNameDictionary.TryGetValue(
                request.Path.ToString(),
                out string providerName))
            {
                if (context.Request.Cookies.ContainsKey(SpaAuthenticationCookieName))
                {
                    await HandleSignInCallback(context, providerName);
                    context.Response.Cookies.Delete(SpaAuthenticationCookieName);
                    return;
                }
            }

            await _next(context);
        }

        private async Task HandleChallenge(HttpContext context)
        {
            var provider = context.Request.Query["provider"].ToString();

            SaveCallbackPathIfNeeded(context, provider);

            var challengeResult = new ChallengeResult(provider, null);

            await challengeResult.ExecuteResultAsync(new ActionContext(context,
                new RouteData(context.Request.RouteValues),
                new ActionDescriptor()));

            AdjustPathInCookies(context);
        }

        private void SaveCallbackPathIfNeeded(HttpContext context, string provider)
        {
            if (!_providersAddedToCallbackPath.Contains(provider))
            {
                var callbackPath = GetCallbackPath(context, provider);
                _callbackPathToProviderNameDictionary[callbackPath] = provider;
                _providersAddedToCallbackPath.Add(provider);
            }
        }

        private string GetCallbackPath(HttpContext context, string provider)
        {
            var authenticationOptionsMonitor = context.RequestServices
                .GetRequiredService<IOptionsMonitor<AuthenticationOptions>>();
            var schemes = authenticationOptionsMonitor.CurrentValue.Schemes.ToList();
            var authenticationScheme = schemes.First(x => x.Name == provider);
            var handler =
                context.RequestServices.GetRequiredService(authenticationScheme.HandlerType) as
                    IAuthenticationHandler;
            var options = handler.GetOptions();
            return options.CallbackPath.ToString();
        }

        private static string _oauthHtmlContent;

        private async Task ReturnOAuthHtml(HttpContext context)
        {
            if (string.IsNullOrEmpty(_oauthHtmlContent))
            {
                var assembly = GetType().Assembly;
                var resourceStream =
                    assembly.GetManifestResourceStream(
                        "IdentityOAuthSpaExtensions.oauth-result.html");
                _oauthHtmlContent = new StreamReader(resourceStream).ReadToEnd();
            }

            context.Response.Headers[HeaderNames.ContentType] = "text/html";
            await context.Response.WriteHtmlAsync(_oauthHtmlContent);
        }

        private async Task HandleSignInCallback(HttpContext context, string provider)
        {
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(body))
            {
                body = context.Request.QueryString.ToString();
            }

            var returnUrl =
                context.Request.Scheme + "://" +
                context.Request.Host + "/external-auth/oauth-result";
            var encodedBody = HttpUtility.UrlEncode(body);
            context.Response.Redirect(returnUrl + $"#provider={provider}&code=" + encodedBody);
        }

        private void AdjustPathInCookies(HttpContext context)
        {
            var cookies = context.Response.Headers[HeaderNames.SetCookie];
            var newCookies = new List<string>();
            foreach (var stringValue in cookies)
            {
                var cookie = SetCookieHeaderValue.Parse(stringValue);
                cookie.Path = "/connect/token";
                newCookies.Add(cookie.ToString());
            }

            var cookieToRedirectToOauthResult =
                new SetCookieHeaderValue(SpaAuthenticationCookieName, "1")
                {
                    Path = "/"
                };
            newCookies.Add(cookieToRedirectToOauthResult.ToString());
            context.Response.Headers[HeaderNames.SetCookie] =
                new StringValues(newCookies.ToArray());
        }
    }
}