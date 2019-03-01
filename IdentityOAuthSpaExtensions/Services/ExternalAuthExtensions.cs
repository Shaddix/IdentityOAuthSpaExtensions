﻿using System;
using IdentityOAuthSpaExtensions.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityOAuthSpaExtensions.Services
{
    public static class ExternalAuthExtensions
    {
        public const string GrantType = "external";

        public static void ConfigureExternalAuth(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<ExternalAuthOptions> configureOptions = null)
        {
            services.Configure<ExternalAuthOptions>(configureOptions ??  (options => { }));
            services.AddTransient<ExternalAuthService>();
            services.AddTransient<ExternalAuthenticatorProvider>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc()
                .AddApplicationPart(typeof(ExternalAuthController).Assembly)
                .AddControllersAsServices();
        }
    }
}