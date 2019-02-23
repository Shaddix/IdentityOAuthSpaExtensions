using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using IdentityOAuthSpaExtensions.Services;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityOAuthSpaExtensions
{
    [Route("/external-auth")]
    [ApiController]
    public class ExternalAuthController : ControllerBase
    {
        private readonly ExternalAuthService _externalAuthService;

        public ExternalAuthController(ExternalAuthService externalAuthService)
        {
            _externalAuthService = externalAuthService;
        }

        [HttpGet("challenge")]
        [AllowAnonymous]
        public async Task<IActionResult> Challenge(string provider, string returnUrl)
        {
            // We can't just use return Challenge(props, provider);
            // because it will try to authorize the user on return
            // but we don't have cookies authentication configured (we are SPA)
            returnUrl = returnUrl ?? Url.Action(nameof(OAuthResult), "ExternalAuth", null, Request.Scheme);

            var url = await _externalAuthService.GetChallengeLink(provider, returnUrl);
            return Redirect(url);
        }

        [HttpGet("oauth-result")]
        public async Task<IActionResult> OAuthResult()
        {
            var assembly = this.GetType().Assembly;
            var resourceStream = assembly.GetManifestResourceStream("IdentityOAuthSpaExtensions.oauth-result.html");
            return File(resourceStream, "text/html");
        }

        [HttpGet("callback-{provider}")]
        [AllowAnonymous]
        public async Task<IActionResult> ChallengeCallbackGet(string provider, string state, string code)
        {
            return await ChallengeCallback(provider, state, code);
        }

        [HttpPost("callback-{provider}")]
        [AllowAnonymous]
        public async Task<IActionResult> ChallengeCallbackPost(string provider, [FromForm] string state,
            [FromForm] string code)
        {
            return await ChallengeCallback(provider, state, code);
        }

        public async Task<IActionResult> ChallengeCallback(string provider, string state, string code)
        {
            //later we could do:
            //var userId = await _externalAuthService.GetExternalUserInfo(provider, code);
            var authOptions = await _externalAuthService.Unprotect(provider, state);

            var uriBuilder = new UriBuilder(authOptions.RedirectUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["code"] = code;
            query["provider"] = provider;
            uriBuilder.Query = query.ToString();

            return Redirect(uriBuilder.ToString());
        }
    }
}