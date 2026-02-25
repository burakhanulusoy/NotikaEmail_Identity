using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Services.UserServices;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize]
    public class ProfileController(UserManager<AppUser> _userManager,IUserService _userService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var userActive = await _userManager.FindByNameAsync(User.Identity.Name);

            var userSettings = await _userService.GetByIdAsync(userActive.Id);




            return View(userSettings);
        }
    }
}
