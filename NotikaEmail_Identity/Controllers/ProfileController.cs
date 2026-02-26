using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.DTOs.UserDtos;
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


        public async Task<IActionResult> UpdateProfil()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var userInfo = await _userService.GetByIdAsync(user.Id);

            return View(userInfo);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfil(UpdateUserDto  dto)
        {

            await _userService.UpdateAsync(dto);

            return RedirectToAction("Index");

        }





    }
}
