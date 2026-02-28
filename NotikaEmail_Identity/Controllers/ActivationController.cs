using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Services.SendEmailServices;
using NotikaEmail_Identity.Services.UserServices;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.Controllers
{
    
    public class ActivationController(UserManager<AppUser> _userManager,
        IUserService _userService,
        ISendEmail _sendEmail,
        ILogger<ActivationController> _logger) : Controller
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

                // HATA LOGU (Seq'te Sarı Renkte Uyarı Verir)
                _logger.LogWarning("Güvenlik Uyarısı: {UserEmail} kullanıcısı hatalı aktivasyon kodu girdi! Girilen Kod: {DenemeKodu}", email, userCodeParameter);

                ModelState.AddModelError("", "Aktivasyon kodunuz hatalı lütfen kontrol edin.");
                ViewBag.Email = email;
                return View();
            }


            user.EmailConfirmed = true;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {

                // SİSTEM HATASI LOGU (Seq'te Kırmızı Renk)
                _logger.LogError("Sistem Hatası: {UserEmail} kullanıcısının mail onaylama işlemi veritabanına kaydedilirken hata oluştu!", email);

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                ViewBag.Email = user.Email;
                return View();
                
            }

            // BAŞARILI İŞLEM LOGU (İşte asıl görmek istediğimiz mavi/yeşil log)
            _logger.LogInformation("Sistem İşlemi: {UserEmail} adlı kullanıcı e-posta adresini başarıyla doğruladı ve hesabını aktifleştirdi.", email);


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
                // ŞÜPHELİ İŞLEM LOGU (Sistemde olmayan bir maile kod istenirse)
                _logger.LogWarning("Şüpheli İşlem: Sistemde kayıtlı olmayan bir e-posta adresi ({UserEmail}) için yeni aktivasyon kodu talep edildi.", email);

                ModelState.AddModelError("", "Bu e-posta adresine ait kullanıcı bulunamadı.");
                return View();
            }

            if (user.EmailConfirmed)
            {
                // BİLGİ LOGU
                _logger.LogInformation("Bilgi: {UserEmail} kullanıcısı hesabı zaten onaylı olduğu halde yeni aktivasyon kodu talep etti.", email);

                ModelState.AddModelError("", "Hesabınız zaten onaylanmış. Giriş yapabilirsiniz.");
                return View();
            }

            Random rnd = new Random();
            int newCode = rnd.Next(100000, 999999);
            user.ActivationCode = newCode;
            await _userManager.UpdateAsync(user);


            _sendEmail.SendEmail(user.Email, newCode);
            TempData["EmailMove"] = user.Email;

            // YENİ KOD GÖNDERİM LOGU
            _logger.LogInformation("Sistem İşlemi: {UserEmail} adlı kullanıcıya başarıyla YENİ bir aktivasyon kodu gönderildi.", email);

            return RedirectToAction("UserActivation");


        }




    }
}
