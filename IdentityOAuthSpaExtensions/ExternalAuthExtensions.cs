using System;
using IdentityOAuthSpaExtensions.Services;
using IdentityOAuthSpaExtensions.Wrappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityOAuthSpaExtensions
{
    /// <summary>
    /// Helpers to integrate into ASP.Net Core pipeline
    /// </summary>
    public static class ExternalAuthExtensions
    {
        /// <summary>
        /// External authentication Grant type for IdentityServer 
        /// </summary>
        public const string GrantType = "external";

        /// <summary>
        /// Adds services for External authentication.
        /// </summary>
        public static void ConfigureExternalAuth(
            this IServiceCollection services,
            Action<ExternalAuthOptions> configureOptions = null
        )
        {
            services.Configure<ExternalAuthOptions>(configureOptions ?? (options => { }));
            services.AddTransient<ExternalAuthService>();
            services.AddTransient<ExternalAuthenticatorProvider>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        /// <summary>
        /// Enable routes and interceptors for supporting External authentication in SPA.
        /// </summary>
        /// <param name="app"></param>
        public static void UseExternalAuth(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExternalAuthMiddleware>();
        }
    }
}
