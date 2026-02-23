using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;

namespace NotikaEmail_Identity.Controllers
{
    public class LoginController(SignInManager<AppUser> _signInManager,UserManager<AppUser> _userManager) : Controller
    {


        public IActionResult SignIn()
        { 
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SignIn(LoginUserViewModel model)
        {

            var user=await _userManager.FindByEmailAsync(model.Email);

            if( user is null )
            {

                ModelState.AddModelError("","Bu emaile kayıtlı hesap bulunamadı.");
                return View(model);
            }

            //beni hatırla,her yablıs gırdıgınde sayac artıyor ona gor ehesap kılutlebır 10 dk
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Email veya şifre hatalı.");
                return View(model);
            }


            return RedirectToAction("Index", "Home");


        }







    }
}
