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
                _logger.LogWarning("⚠️ ERİŞİM REDDİ: Aktivasyon sayfasına doğrudan erişilmeye çalışıldı veya oturum süresi doldu. IP: {Ip}", HttpContext.Connection.RemoteIpAddress);
                return RedirectToAction("SignUp", "Register");
            }

            string email = TempData["EmailMove"].ToString();
            ViewBag.Email = email;

            _logger.LogInformation("⏳ AKTİVASYON BAŞLADI: {UserEmail} kullanıcısı doğrulama ekranında.", email);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserActivation(int userCodeParameter, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // KRİTİK KONTROL: Eğer email manipüle edildiyse ve user null dönerse sistem patlamasın.
            if (user == null)
            {
                _logger.LogCritical("⛔ GÜVENLİK İHLALİ: Olmayan bir email ({Email}) ile aktivasyon POST işlemi denendi! IP: {Ip}", email, HttpContext.Connection.RemoteIpAddress);
                return RedirectToAction("SignUp", "Register");
            }

            var code = user.ActivationCode;

            if (code != userCodeParameter)
            {
                // HATA LOGU (Seq'te Sarı Renk)
                // IP adresini ekledik ki aynı IP'den sürekli hata geliyorsa saldırı olduğunu anlayalım.
                _logger.LogWarning("❌ HATALI KOD: {UserEmail} yanlış aktivasyon kodu girdi. Girilen: {WrongCode} | Beklenen (Gizli): *** | IP: {Ip}", email, userCodeParameter, HttpContext.Connection.RemoteIpAddress);

                ModelState.AddModelError("", "Aktivasyon kodunuz hatalı lütfen kontrol edin.");
                ViewBag.Email = email;
                return View();
            }

            user.EmailConfirmed = true;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                // SİSTEM HATASI LOGU (Seq'te Kırmızı Renk)
                string errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                _logger.LogError("🔥 VERİTABANI HATASI: {UserEmail} aktivasyonu sırasında DB güncellemesi başarısız oldu. Hatalar: {Errors}", email, errors);

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                ViewBag.Email = user.Email;
                return View();
            }

            // Rol Atama İşlemi
            await _userManager.AddToRoleAsync(user, "User");

            // BAŞARILI İŞLEM LOGU (Mavi/Yeşil)
            _logger.LogInformation("✅ HESAP AKTİF: {UserEmail} doğrulandı ve 'User' rolü atandı. Hoş geldin! IP: {Ip}", email, HttpContext.Connection.RemoteIpAddress);

            return RedirectToAction("SignIn", "Login");
        }

        public IActionResult RequestNewCode()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestNewCode(string email)
        {
            // Kullanıcı boş mu kontrolü
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("⚠️ GEÇERSİZ İSTEK: Boş email ile kod talep edildi. IP: {Ip}", HttpContext.Connection.RemoteIpAddress);
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // ŞÜPHELİ İŞLEM LOGU (User Enumeration Attack tespiti için önemli)
                _logger.LogWarning("🕵️‍♂️ ŞÜPHELİ İŞLEM: Sistemde olmayan '{TargetEmail}' adresi için aktivasyon kodu istendi. IP: {Ip}", email, HttpContext.Connection.RemoteIpAddress);

                // Güvenlik gereği "Böyle bir kullanıcı yok" demek yerine genel bir hata verebiliriz 
                // ama şimdilik senin yapını koruyorum:
                ModelState.AddModelError("", "Bu e-posta adresine ait kullanıcı bulunamadı.");
                return View();
            }

            if (user.EmailConfirmed)
            {
                // BİLGİ LOGU
                _logger.LogInformation("ℹ️ GEREKSİZ İSTEK: {UserEmail} zaten onaylı olduğu halde tekrar kod istedi.", email);

                ModelState.AddModelError("", "Hesabınız zaten onaylanmış. Giriş yapabilirsiniz.");
                return View();
            }

            Random rnd = new Random();
            int newCode = rnd.Next(100000, 999999);
            user.ActivationCode = newCode;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                try
                {
                    _sendEmail.SendEmail(user.Email, newCode);
                    TempData["EmailMove"] = user.Email;

                    // YENİ KOD GÖNDERİM LOGU
                    _logger.LogInformation("🔄 KOD YENİLENDİ: {UserEmail} için yeni aktivasyon kodu üretildi ve yollandı. IP: {Ip}", email, HttpContext.Connection.RemoteIpAddress);

                    return RedirectToAction("UserActivation");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "📮 MAİL HATASI: {UserEmail} adresine aktivasyon kodu gönderilemedi!", email);
                    ModelState.AddModelError("", "Mail gönderiminde bir hata oluştu.");
                    return View();
                }
            }
            else
            {
                _logger.LogError("🔥 DB HATASI: {UserEmail} için yeni kod veritabanına yazılamadı.", email);
                return View();
            }
        }
    }
}