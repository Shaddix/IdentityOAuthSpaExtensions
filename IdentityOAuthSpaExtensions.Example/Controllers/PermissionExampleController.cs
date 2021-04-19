using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityOAuthSpaExtensions.Example.Permissions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IdentityOAuthSpaExtensions.Example.Controllers
{
    [Route("api/permissions")]
    [ApiController]
    public class PermissionExampleController : ControllerBase
    {
        [HttpGet("documents")]
        [Authorize]
        [PermissionAuthorize(Permission.DocumentManagement)]
        public ActionResult<IEnumerable<string>> GetDocuments()
        {
            return new string[] {"doc1", "doc2"};
        }

        [HttpGet("users")]
        [Authorize]
        [PermissionAuthorize(Permission.UserManagement)]
        public ActionResult<IEnumerable<string>> GetUsers()
        {
            return new string[] {"user1", "user2"};
        }
    }
}