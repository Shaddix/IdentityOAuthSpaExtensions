using Microsoft.AspNetCore.Authentication;

namespace IdentityOAuthSpaExtensions.Services
{
    public class ExternalUserInfo
    {
        public string Id { get; set; }
        public AuthenticationTicket Ticket { get; set; }
        public string ProviderName { get; set; }
    }
}
