using Microsoft.AspNetCore.Mvc;

namespace Sample.WebClient.Controllers
{
    public class AuthorizationController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
