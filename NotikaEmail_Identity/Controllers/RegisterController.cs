using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.Services.SendEmailServices;

namespace NotikaEmail_Identity.Controllers
{
    public class RegisterController(UserManager<AppUser> _userManager,
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
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // 1. Şifre Eşleşme Kontrolü
            if (model.Password != model.ConfirmPassword)
            {
                // LOG: Basit kullanıcı hatası veya bot denemesi
                _logger.LogWarning("⚠️ KAYIT BAŞARISIZ (Şifre Uyumsuz): {UserEmail} için girilen şifreler uyuşmuyor. IP: {Ip}", model.Email, ipAddress);

                ModelState.AddModelError("", "Girdiğiniz şifreler birbiriyle uyuşmuyor.");
                return View(model);
            }

            Random rnd = new Random();
            int activationCode = rnd.Next(100000, 999999);

            var user = new AppUser()
            {
                Email = model.Email,
                UserName = model.UserName,
                Name = model.Name,
                Surname = model.Surname,
                CreatedDate = DateTime.Now,
                Job = "Belirtilmemiş",
                AboutMe = "Belirtilmemiş",
                ActivationCode = activationCode,
                EmailConfirmed = false, // Açıkça belirtmekte fayda var
                IsActive= false
                
            };

            // 2. Kullanıcı Oluşturma (Identity)
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                // Hataları birleştirip tek satırda logluyoruz ki okuması kolay olsun
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));

                _logger.LogWarning("🛑 KAYIT HATASI (Identity): {UserEmail} oluşturulamadı. Hatalar: {Errors} | IP: {Ip}", model.Email, errors, ipAddress);

                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
                return View(model);
            }

            // 3. Rol Atama
            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                _logger.LogError("🔥 ROL HATASI: {UserEmail} oluşturuldu ancak 'User' rolü atanamadı! IP: {Ip}", user.Email, ipAddress);
                // Burada işlemi durdurmuyoruz, kullanıcı oluştu ama yetkisi yok. Admin panelinden düzeltilebilir.
            }

            // BAŞARILI DB KAYDI LOGU
            _logger.LogInformation("✅ YENİ ÜYE KAYDI: {UserEmail} ({UserName}) sisteme eklendi. IP: {Ip}", user.Email, user.UserName, ipAddress);

            // 4. Aktivasyon Maili Gönderme
            try
            {
                _sendEmail.SendEmail(user.Email, activationCode);

                _logger.LogInformation("✉️ MAİL GÖNDERİLDİ: {UserEmail} adresine aktivasyon kodu ({Code}) yollandı.", user.Email, activationCode);
            }
            catch (Exception ex)
            {
                // Mail gitmezse kullanıcı giriş yapamaz, bu yüzden bu hata Kritiktir.
                _logger.LogCritical(ex, "❌ MAİL HATASI: {UserEmail} oluşturuldu ama aktivasyon maili gidemedi! Kod: {Code}", user.Email, activationCode);

                // Kullanıcıya hissettirmemek için veya uyarmak için mesaj ekleyebiliriz
                // ModelState.AddModelError("", "Kayıt oldunuz ancak aktivasyon maili gönderilemedi. Lütfen destekle iletişime geçin.");
            }

            TempData["EmailMove"] = model.Email;

            return RedirectToAction("UserActivation", "Activation");
        }
    }
}