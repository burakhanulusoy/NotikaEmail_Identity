using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.RoleNames;

namespace NotikaEmail_Identity.Areas.Admin.Controllers
{
    [Area(Roles.Admin)]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
