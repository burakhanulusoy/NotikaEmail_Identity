using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Services.SendEmailServices;
using NotikaEmail_Identity.Services.UserServices;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.Controllers
{
    
    public class ActivationController(UserManager<AppUser> _userManager,IUserService _userService,ISendEmail _sendEmail) : Controller
    {
        public async Task<IActionResult> UserActivation()
        {


            if (TempData["EmailMove"] is null)
            {
                return RedirectToAction("SignUp", "Register");
            }

            string email = TempData["EmailMove"].ToString();
            ViewBag.Email = email;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserActivation(int userCodeParameter,string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            var code = user.ActivationCode;

            if(code != userCodeParameter)
            {
                ModelState.AddModelError("", "Aktivasyon kodunuz hatalı lütfen kontrol edin.");
                ViewBag.Email = email;
                return View();
            }


            user.EmailConfirmed = true;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                ViewBag.Email = user.Email;
                return View();
                
            }




            return RedirectToAction("SignIn", "Login");


        }
            


        public IActionResult RequestNewCode()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RequestNewCode(string email)
        {
            
            var user = await _userManager.FindByEmailAsync (email);
            if (user == null)
            {
                ModelState.AddModelError("", "Bu e-posta adresine ait kullanıcı bulunamadı.");
                return View();
            }

            if (user.EmailConfirmed)
            {
                ModelState.AddModelError("", "Hesabınız zaten onaylanmış. Giriş yapabilirsiniz.");
                return View();
            }

            Random rnd = new Random();
            int newCode = rnd.Next(100000, 999999);
            user.ActivationCode = newCode;
            await _userManager.UpdateAsync(user);


            _sendEmail.SendEmail(user.Email, newCode);
            TempData["EmailMove"] = user.Email;
            return RedirectToAction("UserActivation");


        }




    }
}
