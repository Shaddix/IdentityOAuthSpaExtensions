using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IdentityOAuthSpaExtensions.Example
{
    public class IdentityContext : ApiAuthorizationDbContext<IdentityUser>
    {
        public IdentityContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions) :
            base(options, operationalStoreOptions)
        {
        }
    }
}