using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;

namespace NotikaEmail_Identity.Controllers
{
    public class RegisterController(UserManager<AppUser> _userManger) : Controller
    {
        public IActionResult SignUp()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SignUp(RegisterUserViewModel model)
        {

            

            if(model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Lütfen şifreleri aynı girin.");
                return View(model);
            }




            //yapma nedenim create user içine appuser türünde istiyor!!!
            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.UserName,
                Name = model.Name,
                Surname = model.Surname
            };


            var result = await _userManger.CreateAsync(user, model.Password);

            if(!result.Succeeded)
            {
                foreach(var item in result.Errors)
                {

                    ModelState.AddModelError("", item.Description);
                }
                return View(model);
            }


            return RedirectToAction("SignIn", "Login");
            


        }




    }
}
