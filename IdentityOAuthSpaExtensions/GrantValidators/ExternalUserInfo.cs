using Microsoft.AspNetCore.Authentication;

namespace IdentityOAuthSpaExtensions.GrantValidators
{
    public class ExternalUserInfo
    {
        public string Id { get; set; }
        public AuthenticationTicket Ticket { get; set; }
    }
}