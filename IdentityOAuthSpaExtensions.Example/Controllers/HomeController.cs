using Microsoft.AspNetCore.Mvc;

namespace IdentityOAuthSpaExtensions.Example.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}