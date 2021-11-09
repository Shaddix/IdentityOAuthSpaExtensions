using System.Collections.Generic;
using IdentityOAuthSpaExtensions.Services;
using IdentityServer4.Models;

namespace IdentityOAuthSpaExtensions.Example.IdentityServer
{
    public class Config
    {
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "client",
                    AllowOfflineAccess = true,
                    AllowedGrantTypes = new[]
                    {
                        GrantType.ClientCredentials,
                        ExternalAuthExtensions.GrantType
                    },
                    ClientSecrets = { new Secret("secret".Sha256()) },
                    AllowedScopes = { "api1" }
                },
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource> { new ApiResource("api1", "My API") };
        }
    }
}
