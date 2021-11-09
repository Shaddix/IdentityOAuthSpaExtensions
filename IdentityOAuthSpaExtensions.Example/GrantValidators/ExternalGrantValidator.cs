using IdentityOAuthSpaExtensions.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityOAuthSpaExtensions.Example.GrantValidators
{
    public class ExternalGrantValidator : ExternalAuthenticationGrantValidator<IdentityUser, string>
    {
        public ExternalGrantValidator(
            ExternalAuthService externalAuthService,
            UserManager<IdentityUser> userManager,
            IOptionsMonitor<ExternalAuthOptions> options,
            ILogger<ExternalAuthenticationGrantValidator<IdentityUser, string>> logger
        ) : base(externalAuthService, userManager, options, logger) { }
    }
}
