using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.DTOs.UserDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.Services.UserServices;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize]
    public class ProfileController(UserManager<AppUser> _userManager,IUserService _userService,SignInManager<AppUser> _signInManager) : Controller
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

        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.name = user.Name + "" + user.Surname;
            ViewBag.job = user.Job;


            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {

            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if(!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    // Identity'nin "Eski şifre yanlış" hatasını yakalıyoruz
                    if (error.Code == "PasswordMismatch")
                    {
                        ModelState.AddModelError(string.Empty, "Mevcut şifrenizi yanlış girdiniz.");
                    }
                    else
                    {
                        // Diğer olası Identity hatalarını olduğu gibi basıyoruz
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                }
                return View(model);
            }

            await _signInManager.SignOutAsync();

            return RedirectToAction("SignIn", "Login");

        }




    }
}
