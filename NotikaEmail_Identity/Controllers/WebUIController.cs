using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize]
    public class WebUIController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
