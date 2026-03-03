using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.DTOs.UserDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.Services.UserServices;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize(Roles = "Admin, User")]
    public class ProfileController(UserManager<AppUser> _userManager,
        IUserService _userService,
        SignInManager<AppUser> _signInManager,
        IWebHostEnvironment _hostEnvironment, // Dosya yolu için eklendi
        ILogger<ProfileController> _logger) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var userActive = await _userManager.FindByNameAsync(User.Identity.Name);
            if (userActive == null) return RedirectToAction("SignIn", "Login");

            var userSettings = await _userService.GetByIdAsync(userActive.Id);

            // Kullanıcı sadece profilini görüntülediğinde INFO değil DEBUG kullanırız (Log kirliliğini önlemek için)
            _logger.LogDebug("👤 PROFİL GÖRÜNTÜLENDİ: {UserEmail} profil sayfasına baktı.", userActive.Email);

            return View(userSettings);
        }

        public async Task<IActionResult> UpdateProfil()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var userInfo = await _userService.GetByIdAsync(user.Id);
            return View(userInfo);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfil(UpdateUserDto dto)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // 1. Şifre Kontrolü (Kritik Güvenlik Adımı)
            bool passwordCheck = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);

            if (!passwordCheck)
            {
                // GÜVENLİK UYARISI: Şifresini yanlış giren biri var (Brute force veya saldırı olabilir)
                _logger.LogWarning("🛑 PROFİL GÜNCELLEME HATASI: {UserEmail} mevcut şifresini YANLIŞ girdi. IP: {Ip}", user.Email, ipAddress);

                ModelState.AddModelError("", "Mevcut şifreniz hatalı, lütfen kontrol ediniz.");
                return View(dto);
            }

            // 2. Dosya Yükleme İşlemi
            if (dto.ImageFile is not null)
            {
                var extension = Path.GetExtension(dto.ImageFile.FileName).ToLowerInvariant();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                // Dosya Uzantı Kontrolü
                if (!allowedExtensions.Contains(extension))
                {
                    // ŞÜPHELİ DOSYA LOGU (Shell Upload girişimi olabilir)
                    _logger.LogWarning("⚠️ YASAKLI DOSYA: {UserEmail} yasaklı uzantı ({Extension}) yüklemeye çalıştı! IP: {Ip}", user.Email, extension, ipAddress);

                    ModelState.AddModelError(string.Empty, "Lütfen sadece resim formatında (.jpg, .jpeg, .png, .gif) bir dosya seçin!");
                    return View(dto);
                }

                try
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "UserImagess");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid() + extension;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.ImageFile.CopyToAsync(fileStream);
                    }

                    dto.ImageUrl = "/UserImagess/" + uniqueFileName;

                    _logger.LogInformation("📸 FOTOĞRAF GÜNCELLENDİ: {UserEmail} yeni profil resmi yükledi. Dosya: {FileName}", user.Email, uniqueFileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🔥 DOSYA YÜKLEME HATASI: {UserEmail} resim yüklerken sistem hatası oluştu.", user.Email);
                    ModelState.AddModelError("", "Resim yüklenirken bir hata oluştu.");
                    return View(dto);
                }
            }
            else
            {
                // Eğer yeni resim yüklenmediyse, eski resim yolunu korumak gerekebilir.
                // Servis katmanınız bunu hallediyorsa sorun yok, ancak DTO'da ImageUrl null giderse eski resmi silebilir.
                // Buraya dikkat etmek gerekir. Genelde HiddenInput ile eski URL taşınır.
            }

            // BAŞARILI GÜNCELLEME LOGU
            await _userService.UpdateAsync(dto);

            _logger.LogInformation("✅ PROFİL GÜNCELLENDİ: {UserEmail} bilgilerini değiştirdi. IP: {Ip}", user.Email, ipAddress);

            ViewBag.IsSuccess = true;
            return View(dto);
        }

        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.name = user.Name + " " + user.Surname;
            ViewBag.job = user.Job;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    if (error.Code == "PasswordMismatch")
                    {
                        // ŞİFRE DEĞİŞTİRME HATASI LOGU
                        _logger.LogWarning("🛑 ŞİFRE DEĞİŞİM HATASI: {UserEmail} mevcut şifresini yanlış girdiği için reddedildi. IP: {Ip}", user.Email, ipAddress);
                        ModelState.AddModelError(string.Empty, "Mevcut şifrenizi yanlış girdiniz.");
                    }
                    else
                    {
                        // Şifre politikasına uymama vb. durumlar
                        _logger.LogWarning("⚠️ ŞİFRE POLİTİKASI: {UserEmail} şifre değiştirirken hata aldı: {ErrorCode} - {Desc}", user.Email, error.Code, error.Description);
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                }
                return View(model);
            }

            // KRİTİK LOG: Şifre değişti!
            // Security Stamp güncellemek, kullanıcının diğer cihazlardaki oturumlarını düşürür (Güvenlik için iyidir).
            await _userManager.UpdateSecurityStampAsync(user);

            _logger.LogInformation("🔐 ŞİFRE DEĞİŞTİRİLDİ: {UserEmail} şifresini başarıyla güncelledi. IP: {Ip}. Oturum kapatılıyor.", user.Email, ipAddress);

            await _signInManager.SignOutAsync();
            return RedirectToAction("SignIn", "Login");
        }
    }
}