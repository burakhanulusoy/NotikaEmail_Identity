using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using MailKit.Net.Smtp;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.Controllers
{
    // LOG DÜZELTMESİ: DefaultController yerine ForgotPasswordController yapıldı.
    public class ForgotPasswordController(UserManager<AppUser> _userManager, ILogger<ForgotPasswordController> _logger) : Controller
    {
        public IActionResult ForgetMyPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetMyPassword(ForgetPasswordViewModel model)
        {
            _logger.LogInformation("Şifre Sıfırlama Talebi: {Email} adresi için süreç başlatıldı.", model.Email);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                _logger.LogWarning("Güvenlik: Sistemde {Email} adresine ait kullanıcı bulunamadı.", model.Email);
                ModelState.AddModelError("", "Bu email adresine ait bir kullanıcı bulunamadı.");
                return View(model);
            }

            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            _logger.LogInformation("Token Üretildi: {Email} için token: {Token}", model.Email, passwordResetToken);

            var passwordResetTokenLink = Url.Action("ResetPassword", "ForgotPassword", new
            {
                userId = user.Id,
                token = passwordResetToken,
            }, HttpContext.Request.Scheme);

            MimeMessage mimeMessage = new MimeMessage();
            MailboxAddress mailboxAddressFrom = new MailboxAddress("NotikaApp", "burakhanulusoy18@gmail.com");
            mimeMessage.From.Add(mailboxAddressFrom);

            MailboxAddress mailboxAddressTo = new MailboxAddress("User", model.Email);
            mimeMessage.To.Add(mailboxAddressTo);

            var bodybuilder = new BodyBuilder();
            bodybuilder.TextBody = passwordResetTokenLink;

            mimeMessage.Body = bodybuilder.ToMessageBody();
            mimeMessage.Subject = "Şifre Değişiklik talebi";

            SmtpClient client = new SmtpClient();
            client.Connect("smtp.gmail.com", 587, false);
            client.Authenticate("burakhanulusoy18@gmail.com", "vgmsjisxwqiyyflm");
            client.Send(mimeMessage);
            client.Disconnect(true);

            _logger.LogInformation("Mail Gönderildi: {Email} adresine link başarıyla iletildi.", model.Email);

            ViewBag.SuccessMessage = "Şifre sıfırlama bağlantısı başarıyla gönderildi.";

            return View();
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            _logger.LogInformation("Linke Tıklandı: UserId {UserId} olan kullanıcı şifre ekranını açtı.", userId);

            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var userId = TempData["userId"];
            var token = TempData["token"];

            if (userId is null || token is null)
            {
                _logger.LogWarning("Hata: TempData içerisindeki UserId veya Token kayıp.");
                ModelState.AddModelError("", "Bağlantı süresi dolmuş veya geçersiz. Lütfen tekrar mail isteyin.");
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                _logger.LogWarning("Hata: Şifresi değiştirilecek kullanıcı (ID: {UserId}) bulunamadı.", userId);
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, token.ToString(), model.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Başarısız: {Email} şifresini güncelleyemedi.", user.Email);

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                TempData.Keep("userId");
                TempData.Keep("token");

                return View(model);
            }

            // --- ROL EKLEME KISMI ---
            // Eğer rolü yoksa ekler, varsa hata vermez pas geçer. UpdateAsync'e gerek yoktur.
            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation("Rol Ataması: {Email} adlı kullanıcıya 'User' rolü eklendi.", user.Email);
            // ------------------------

            _logger.LogInformation("Başarılı: {Email} şifresini başarıyla değiştirdi.", user.Email);
            ViewBag.SuccessMessage = "Şifreniz başarıyla güncellendi. Giriş sayfasına yönlendiriliyorsunuz...";

            return RedirectToAction("SignIn", "Login");
        }
    }
}