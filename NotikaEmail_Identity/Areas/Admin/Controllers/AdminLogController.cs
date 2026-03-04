using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.RoleNames;

namespace NotikaEmail_Identity.Areas.Admin.Controllers
{
    [Authorize(Roles ="Admin")]
    [Area(Roles.Admin)]
    public class AdminLogController(SeqLogService _logService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var logs = await _logService.GetSystemLogsAsync();
            return View(logs);
        }
    }
}
