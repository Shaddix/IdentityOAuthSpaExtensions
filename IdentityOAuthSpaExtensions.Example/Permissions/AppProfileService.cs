using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace IdentityOAuthSpaExtensions.Example.Permissions
{
    public class AppProfileService : ProfileService<IdentityUser>
    {
        public AppProfileService(
            UserManager<IdentityUser> userManager,
            IUserClaimsPrincipalFactory<IdentityUser> claimsFactory,
            ILogger<ProfileService<IdentityUser>> logger
        ) : base(userManager, claimsFactory, logger) { }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await base.GetProfileDataAsync(context);

            context.IssuedClaims.Add(
                new Claim(ClaimType.Permission, Permission.UserManagement.ToString())
            );
        }
    }
}
