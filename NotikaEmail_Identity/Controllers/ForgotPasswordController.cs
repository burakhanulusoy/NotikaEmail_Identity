using Microsoft.AspNetCore.Mvc;

namespace NotikaEmail_Identity.Controllers
{
    public class ForgotPasswordController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
