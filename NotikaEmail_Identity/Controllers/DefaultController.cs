using Microsoft.AspNetCore.Mvc;

namespace NotikaEmail_Identity.Controllers
{
    public class DefaultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Inbox()
        {
            return View();
        }

    }
}
