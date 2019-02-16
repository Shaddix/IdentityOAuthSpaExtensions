using IdentityOAuthSpaExtensions.GrantValidators;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace IdentityOAuthSpaExtensions.Example.GrantValidators
{
    public class ExternalGrantValidator : ExternalAuthenticationGrantValidator<IdentityUser, string>
    {
        public ExternalGrantValidator(
            ExternalAuthService externalAuthService,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalAuthenticationGrantValidator<IdentityUser, string>> logger)
            : base(externalAuthService, userManager, logger)
        {
        }
    }
}