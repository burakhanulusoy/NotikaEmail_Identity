using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.Services.SendEmailServices;







namespace NotikaEmail_Identity.Controllers
{
    public class RegisterController(UserManager<AppUser> _userManger ,
        ISendEmail _sendEmail,
        ILogger<RegisterController> _logger) : Controller
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
                // LOG: Şifre uyuşmazlığı (Potansiyel hatalı giriş veya bot denemesi)
                _logger.LogWarning("Kayıt Denemesi Başarısız: {UserEmail} adresi için girilen şifreler birbiriyle uyuşmuyor.", model.Email);

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
                // LOG: Identity kurallarına takılan kayıt denemesi (Şifre zayıf olabilir, mail zaten vardır vb.)
                _logger.LogWarning("Sistem Uyarısı: {UserEmail} için yeni kullanıcı kaydı oluşturulamadı. Hata Sayısı: {ErrorCount}",
                    model.Email, result.Errors.Count());

                foreach (var item in result.Errors)
                {

                    ModelState.AddModelError("", item.Description);
                }
                return View(model);
            }

            // LOG: Efsanevi Başarı Logu!
            _logger.LogInformation("Sistem İşlemi: Yeni bir kullanıcı başarıyla sisteme kayıt oldu. Kullanıcı: {UserName} ({UserEmail}). Aktivasyon kodu gönderiliyor.",
                user.UserName, user.Email);


            try
            {
                _sendEmail.SendEmail(user.Email, activationCode);
                _logger.LogInformation("Bilgi: {UserEmail} adresine aktivasyon kodu ({Code}) başarıyla mail atıldı.", user.Email, activationCode);
            }
            catch (Exception ex)
            {
                // LOG: Mail sunucusu hatası (Çok kritik!)
                _logger.LogError(ex, "Sistem Hatası: {UserEmail} adresine aktivasyon maili gönderilirken bir sorun oluştu!", user.Email);
            }

            TempData["EmailMove"] = model.Email;

            return RedirectToAction("UserActivation", "Activation");
            


        }




    }
}
