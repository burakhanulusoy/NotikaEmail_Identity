using Microsoft.AspNetCore.Mvc;

namespace NotikaEmail_Identity.Controllers
{
    public class NotComponentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
