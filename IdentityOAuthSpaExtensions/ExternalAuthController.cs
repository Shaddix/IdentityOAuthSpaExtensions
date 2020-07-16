using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using IdentityOAuthSpaExtensions.Services;
using IdentityOAuthSpaExtensions.Wrappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IdentityOAuthSpaExtensions
{
    /// <summary>
    /// Controller to handle external authentication
    /// </summary>
    [Route("/external-auth")]
    [ApiController]
    public class ExternalAuthController : ControllerBase
    {
        private readonly ExternalAuthService _externalAuthService;
        private readonly ILogger<ExternalAuthController> _logger;

        public ExternalAuthController(ExternalAuthService externalAuthService, ILogger<ExternalAuthController> logger)
        {
            _externalAuthService = externalAuthService;
            _logger = logger;
        }

        /// <summary>
        /// Redirects the user to external authentication page
        /// </summary>
        /// <param name="provider">External authentication provider name (e.g. 'google' or 'facebook')</param>
        /// <param name="returnUrl">URL to return to</param>
        /// <returns>Redirect result</returns>
        [HttpGet("challenge")]
        [AllowAnonymous]
        public async Task<IActionResult> Challenge(string provider, string returnUrl)
        {
            // We can't just use return Challenge(props, provider);
            // because it will try to authorize the user on return
            // but we don't have cookies authentication configured (we are SPA)
            returnUrl ??= Url.Action(nameof(OAuthResult), "ExternalAuth", null, Request.Scheme);

            var url = await _externalAuthService.GetChallengeLink(provider, returnUrl);
            return Redirect(url);
        }

        private async Task<IActionResult> ChallengeCallbackTwitter()
        {
            var oauth_token = Request.Query["oauth_token"];
            var oauth_verifier = Request.Query["oauth_verifier"];
            var state = Request.Cookies["__TwitterState"];
            var code = JsonConvert.SerializeObject(new TwitterAuthenticationHandlerWrapper.CodeContainer
            {
                OAuthToken = oauth_token,
                OAuthVerifier = oauth_verifier,
            });
            return await ChallengeCallback("Twitter", state, code);
        }

        /// <summary>
        /// Return URL from external authentication.
        /// This URL should be included in the list of return URLs at external authentication provider side.
        /// </summary>
        /// <param name="provider">External authentication provider name (e.g. 'google' or 'facebook')</param>
        /// <param name="state">Auth state</param>
        /// <param name="code">AuthCode</param>
        /// <returns>Redirect result to main SPA if everything is ok; error otherwise</returns>
        [HttpGet("callback-{provider}")]
        [AllowAnonymous]
        public async Task<IActionResult> ChallengeCallbackGet(string provider, string state, string code)
        {
            if (provider.ToLower() == "twitter")
            {
                return await ChallengeCallbackTwitter();
            }

            return await ChallengeCallback(provider, state, code);
        }

        /// <summary>
        /// Return URL from external authentication in case of POST returns.
        /// This URL should be included in the list of return URLs at external authentication provider side.
        /// </summary>
        /// <param name="provider">External authentication provider name (e.g. 'google' or 'facebook')</param>
        /// <param name="state">Auth state</param>
        /// <param name="code">AuthCode</param>
        /// <returns>Redirect result to main SPA if everything is ok; error otherwise</returns>
        [HttpPost("callback-{provider}")]
        [AllowAnonymous]
        public async Task<IActionResult> ChallengeCallbackPost(string provider, [FromForm]
            string state,
            [FromForm]
            string code)
        {
            return await ChallengeCallback(provider, state, code);
        }

        private async Task<IActionResult> ChallengeCallback(string provider, string state, string code)
        {
            //later we could do:
            //var userId = await _externalAuthService.GetExternalUserInfo(provider, code);
            var authOptions = await _externalAuthService.Unprotect(provider, state);

            var encodedUrl = $"code={HttpUtility.UrlEncode(code)}&provider={HttpUtility.UrlEncode(provider)}";
            return Redirect(authOptions.RedirectUri + "#" + encodedUrl);
        }

        /// <summary>
        /// Returns static HTML that will trigger the authentication callback on main SPA
        /// </summary>
        [HttpGet("oauth-result")]
        public async Task<IActionResult> OAuthResult()
        {
            var assembly = this.GetType().Assembly;
            var resourceStream = assembly.GetManifestResourceStream("IdentityOAuthSpaExtensions.oauth-result.html");
            return File(resourceStream, "text/html");
        }
    }
}