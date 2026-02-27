using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.Services.SendEmailServices;







namespace NotikaEmail_Identity.Controllers
{
    public class RegisterController(UserManager<AppUser> _userManger ,ISendEmail _sendEmail) : Controller
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


            Random rnd=new Random();
            int activationCode = rnd.Next(100000,999999);




            //yapma nedenim create user içine appuser türünde istiyor!!!
            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.UserName,
                Name = model.Name,
                Surname = model.Surname,
                CreatedDate = DateTime.Now,
                Job = "Belirtilmemiş",
                AboutMe="Belirtilmemiş",
                ActivationCode=activationCode

            };

            var result = await _userManger.CreateAsync(user, model.Password);

            if(!result.Succeeded)
            {
               
                foreach (var item in result.Errors)
                {

                    ModelState.AddModelError("", item.Description);
                }
                return View(model);
            }

            _sendEmail.SendEmail(user.Email, activationCode);

            TempData["EmailMove"] = model.Email;

            return RedirectToAction("UserActivation", "Activation");
            


        }




    }
}
