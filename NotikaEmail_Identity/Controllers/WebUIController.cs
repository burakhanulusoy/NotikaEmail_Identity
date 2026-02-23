using Microsoft.AspNetCore.Mvc;

namespace NotikaEmail_Identity.Controllers
{
    public class WebUIController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
