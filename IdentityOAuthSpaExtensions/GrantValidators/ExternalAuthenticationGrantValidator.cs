﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityOAuthSpaExtensions.GrantValidators
{
    /// <summary>
    /// Authenticates users via external OAuth providers.
    /// Uses special "external" GrantType (http://docs.identityserver.io/en/release/topics/extension_grants.html).
    /// </summary>
    public class ExternalAuthenticationGrantValidator<TUser, TKey> : IExtensionGrantValidator
        where TUser : IdentityUser<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        private readonly ExternalAuthService _externalAuthService;
        private readonly UserManager<TUser> _userManager;
        private readonly IOptionsMonitor<ExternalAuthOptions> _options;
        private readonly ILogger<ExternalAuthenticationGrantValidator<TUser, TKey>> _logger;

        public ExternalAuthenticationGrantValidator(
            ExternalAuthService externalAuthService,
            UserManager<TUser> userManager,
            IOptionsMonitor<ExternalAuthOptions> options,
            ILogger<ExternalAuthenticationGrantValidator<TUser, TKey>> logger
        )
        {
            _externalAuthService = externalAuthService;
            _userManager = userManager;
            _options = options;
            _logger = logger;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var oAuthCode = context.Request.Raw.Get("code");
            var providerName = context.Request.Raw.Get("provider");
            if (string.IsNullOrEmpty(oAuthCode)
                || string.IsNullOrEmpty(providerName))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest);
                return;
            }

            var externalUserInfo = await _externalAuthService.GetExternalUserInfo(providerName, oAuthCode);

            if (externalUserInfo == null)
            {
                context.Result = GetExternalUserNotFound();
                return;
            }

            try
            {
                var user = await _userManager.FindByLoginAsync(
                    providerName,
                    externalUserInfo.Id);
                if (user != null)
                {
                    context.Result = CreateValidationResultForUser(user);
                }
                else
                {
                    context.Result = await CreateResultForLocallyNotFoundUser(providerName, externalUserInfo);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(
                    $"Exception while authorizing by external provider '{providerName}:{externalUserInfo.Id}', {e}");
                context.Result = GetExternalUserNotFound();
            }
        }

        /// <summary>
        /// Provides GrantValidationResult for the case when user was successfully authenticated by external OAuth provider, but no corresponding user was found locally
        /// (i.e. there's no corresponding entries in AspNetUserLogins table).
        /// You could override that method to auto-create local users.
        /// </summary>
        /// <param name="providerName">OAuth provider name</param>
        /// <param name="externalUserId">User Id provided by external OAuth server</param>
        /// <returns>GrantValidationResult</returns>
        protected virtual async Task<GrantValidationResult> CreateResultForLocallyNotFoundUser(string providerName,
            ExternalUserInfo externalUserInfo)
        {
            if (!_options.CurrentValue.CreateUserIfNotFound)
            {
                return GetExternalUserNotFound();
            }

            var user = CreateNewUser(externalUserInfo);
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                throw new Exception("User creation failed " + createResult);
            }

            var addLoginResult = await _userManager.AddLoginAsync(user,
                new UserLoginInfo(providerName, externalUserInfo.Id, ""));
            if (!addLoginResult.Succeeded)
            {
                throw new Exception("Adding external login failed " + addLoginResult);
            }

            return CreateValidationResultForUser(user);
        }

        protected virtual TUser CreateNewUser(ExternalUserInfo externalUserInfo)
        {
            return new TUser()
            {
                UserName = externalUserInfo.Id,
            };
        }

        protected virtual GrantValidationResult CreateValidationResultForUser(TUser user)
        {
            return new GrantValidationResult(
                user.Id.ToString(),
                GrantType,
                new[]
                {
                    new Claim(JwtClaimTypes.Id, user.Id.ToString())
                });
        }

        protected virtual GrantValidationResult GetExternalUserNotFound()
        {
            return new GrantValidationResult(
                TokenRequestErrors.UnauthorizedClient,
                "login_external_UserNotFound");
        }

        public string GrantType => ExternalAuthExtensions.GrantType;
    }
}