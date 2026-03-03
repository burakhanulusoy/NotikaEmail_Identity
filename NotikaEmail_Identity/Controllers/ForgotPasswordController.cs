using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using MailKit.Net.Smtp;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.Controllers
{
    public class ForgotPasswordController(UserManager<AppUser> _userManager, ILogger<ForgotPasswordController> _logger) : Controller
    {
        public IActionResult ForgetMyPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetMyPassword(ForgetPasswordViewModel model)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            _logger.LogInformation("🔑 ŞİFRE SIFIRLAMA TALEBİ: '{Email}' adresi için süreç başlatıldı. IP: {Ip}", model.Email, ipAddress);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                // Güvenlik: Kullanıcıya "Böyle biri yok" demek yerine sessizce hata dönüyoruz ama LOGLUYORUZ.
                // Bu sayede "User Enumeration" saldırılarını takip edebiliriz.
                _logger.LogWarning("⚠️ GEÇERSİZ E-POSTA: Sistemde kayıtlı olmayan '{Email}' adresi için şifre sıfırlama istendi. IP: {Ip}", model.Email, ipAddress);

                // Kullanıcıya yine de "Mail gönderildi" diyebiliriz (Güvenlik için) veya "Bulunamadı" diyebiliriz.
                ModelState.AddModelError("", "Bu email adresine ait bir kullanıcı bulunamadı.");
                return View(model);
            }

            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Token'ı loglamak güvenlik riski olabilir ama debug için ilk 10 karakterini loglayalım.
            _logger.LogDebug("🎟️ TOKEN ÜRETİLDİ: {Email} için token oluşturuldu. (Token başı: {TokenStart}...) ", model.Email, passwordResetToken.Substring(0, 10));

            var passwordResetTokenLink = Url.Action("ResetPassword", "ForgotPassword", new
            {
                userId = user.Id,
                token = passwordResetToken,
            }, HttpContext.Request.Scheme);


            // --- MAIL GÖNDERME İŞLEMİ (Hata yakalama eklendi) ---
            try
            {
                MimeMessage mimeMessage = new MimeMessage();
                MailboxAddress mailboxAddressFrom = new MailboxAddress("NotikaApp", "burakhanulusoy18@gmail.com");
                mimeMessage.From.Add(mailboxAddressFrom);

                MailboxAddress mailboxAddressTo = new MailboxAddress("User", model.Email);
                mimeMessage.To.Add(mailboxAddressTo);

                var bodybuilder = new BodyBuilder();
                bodybuilder.TextBody = $"Şifrenizi sıfırlamak için tıklayınız: {passwordResetTokenLink}";
                // Linki HTML olarak göndermek daha şık olur ama TextBody de çalışır.

                mimeMessage.Body = bodybuilder.ToMessageBody();
                mimeMessage.Subject = "NotikaApp - Şifre Sıfırlama Talebi";

                using (SmtpClient client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("burakhanulusoy18@gmail.com", "xhbicuiuxaffpwmz");
                    client.Send(mimeMessage);
                    client.Disconnect(true);
                }

                _logger.LogInformation("✉️ MAİL GÖNDERİLDİ: {Email} adresine sıfırlama linki başarıyla iletildi.", model.Email);
                ViewBag.SuccessMessage = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "🔥 MAİL HATASI: {Email} adresine mail atarken SMTP sunucusu hata verdi!", model.Email);
                ModelState.AddModelError("", "Mail gönderiminde bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.");
                return View(model);
            }
            // -----------------------------------------------------

            return View();
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId == null || token == null)
            {
                _logger.LogWarning("🚫 HATALI LİNK: Kullanıcı geçersiz veya eksik parametreli bir linke tıkladı.");
                return RedirectToAction("SignIn", "Login");
            }

            _logger.LogInformation("🔗 LİNKE TIKLANDI: Kullanıcı (ID: {UserId}) şifre belirleme ekranını açtı.", userId);

            // TempData veriyi bir sonraki Request'e taşır.
            TempData["userId"] = userId;
            TempData["token"] = token;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            // TempData okunduğunda silinir, tekrar okumak için Keep yapmak gerekebilir ama burada değişkene atıyoruz.
            var userId = TempData["userId"];
            var token = TempData["token"];
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (userId is null || token is null)
            {
                _logger.LogWarning("⌛ OTURUM ZAMAN AŞIMI: TempData kayboldu. Kullanıcı sayfada çok uzun süre beklemiş olabilir. IP: {Ip}", ipAddress);
                ModelState.AddModelError("", "Bağlantı süresi dolmuş veya sayfa yenilenmiş. Lütfen tekrar mail talep edin.");
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                _logger.LogError("❌ KRİTİK HATA: ID: {UserId} olan kullanıcı veritabanında bulunamadı!", userId);
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, token.ToString(), model.Password);

            if (!result.Succeeded)
            {
                // Hataları tek bir string'de birleştirip logluyoruz
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning("⚠️ SIFIRLAMA BAŞARISIZ: {Email} şifre değiştirirken hata aldı. Sebepler: {Errors}", user.Email, errors);

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                // Hata durumunda TempData silinmesin diye koruyoruz
                TempData.Keep("userId");
                TempData.Keep("token");

                return View(model);
            }

            // --- ROL GÜNCELLEME (Opsiyonel) ---
            // Şifresini sıfırlayan kullanıcıyı aktif kabul edip rolünü garantiye alıyoruz.
            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
                _logger.LogInformation("🛡️ ROL GÜNCELLENDİ: {Email} kullanıcısına 'User' rolü tanımlandı.", user.Email);
            }

            _logger.LogInformation("✅ ŞİFRE DEĞİŞTİ: {Email} şifresini başarıyla güncelledi. IP: {Ip}", user.Email, ipAddress);

            // Kullanıcıya bilgi verip Login'e atıyoruz
            TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz."; // Login sayfasında göstermek için

            return RedirectToAction("SignIn", "Login");
        }
    }
}