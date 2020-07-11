using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace IdentityOAuthSpaExtensions.Wrappers
{
    public class RequestCookieCollection : Dictionary<string, string>, IRequestCookieCollection
    {
        public RequestCookieCollection(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        public new ICollection<string> Keys => base.Keys.ToList();
    }
}