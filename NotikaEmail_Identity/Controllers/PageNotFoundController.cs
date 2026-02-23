using Microsoft.AspNetCore.Mvc;

namespace NotikaEmail_Identity.Controllers
{
    public class PageNotFoundController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
