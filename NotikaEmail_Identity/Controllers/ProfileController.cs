using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.DTOs.UserDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.Services.UserServices;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize]
    public class ProfileController(UserManager<AppUser> _userManager,
        IUserService _userService,
        SignInManager<AppUser> _signInManager,
        ILogger<ProfileController> _logger) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var userActive = await _userManager.FindByNameAsync(User.Identity.Name);

            var userSettings = await _userService.GetByIdAsync(userActive.Id);

            return View(userSettings);
        }


        public async Task<IActionResult> UpdateProfil()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var userInfo = await _userService.GetByIdAsync(user.Id);

            return View(userInfo);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfil(UpdateUserDto  dto)
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            bool passwordCheck = await _userManager.CheckPasswordAsync(user,dto.CurrentPassword);

            if(!passwordCheck)
            {
                // GÜVENLİK UYARISI: Şifresini yanlış giren biri var
                _logger.LogWarning("Güvenlik Uyarısı: {UserEmail} adlı kullanıcı profilini güncellemeye çalışırken mevcut şifresini HATALI girdi.", user.Email);

                ModelState.AddModelError("", "Şifreniz hatalı lütfen kontrol ediniz");
                return View(dto);
            }

            if (dto.ImageFile is not null)
            {
                // 1. Gelen dosyanın uzantısını alıp küçük harfe çeviriyoruz
                var extension = Path.GetExtension(dto.ImageFile.FileName).ToLowerInvariant();

                // 2. İzin verdiğimiz uzantıların bir listesi
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                // 3. Eğer gelen uzantı bizim listede yoksa işlemi durdur ve hata dön
                if (!allowedExtensions.Contains(extension))
                {
                    // ŞÜPHELİ DOSYA LOGU
                    _logger.LogWarning("Sistem Uyarısı: {UserEmail} adlı kullanıcı yasaklı bir dosya uzantısı ({Extension}) yüklemeye çalıştı!", user.Email, extension);

                    ModelState.AddModelError(string.Empty, "Lütfen sadece resim formatında (.jpg, .jpeg, .png, .gif) bir dosya seçin!");
                    return View(dto);
                }

                // Uzantı sorunsuzsa normal kayıt işlemine devam ediyoruz
                var currentDirectory = Directory.GetCurrentDirectory();
                var saveDirectory = Path.Combine(currentDirectory, "wwwroot", "UserImagess");

                if (!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }

                var ImageName = Guid.NewGuid() + extension;
                var saveLocation = Path.Combine(saveDirectory, ImageName);

                using var stream = new FileStream(saveLocation, FileMode.Create);
                await dto.ImageFile.CopyToAsync(stream);

                dto.ImageUrl = "/UserImagess/" + ImageName;
                // DOSYA YÜKLEME LOGU
                _logger.LogInformation("Sistem İşlemi: {UserEmail} adlı kullanıcı profil resmini başarıyla güncelledi. Yeni Dosya: {ImageName}", user.Email, ImageName);

            }


            // BAŞARILI GÜNCELLEME LOGU
            _logger.LogInformation("Sistem İşlemi: {UserEmail} adlı kullanıcı profil bilgilerini başarıyla güncelledi.", user.Email);

            await _userService.UpdateAsync(dto);

            ViewBag.IsSuccess = true;
            return View(dto);

        }

        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.name = user.Name + "" + user.Surname;
            ViewBag.job = user.Job;


            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {

            if(!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if(!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    // Identity'nin "Eski şifre yanlış" hatasını yakalıyoruz
                    if (error.Code == "PasswordMismatch")
                    {
                        // ŞİFRE DEĞİŞTİRME HATASI LOGU
                        _logger.LogWarning("Güvenlik Uyarısı: {UserEmail} kullanıcısı şifresini değiştirmeye çalıştı ancak mevcut şifresini yanlış girdi.", user.Email);
                        ModelState.AddModelError(string.Empty, "Mevcut şifrenizi yanlış girdiniz.");
                    }
                    else
                    {
                        _logger.LogWarning("Sistem Uyarısı: {UserEmail} kullanıcısının şifre değişimi kural hatası nedeniyle başarısız oldu: {ErrorCode}", user.Email, error.Code);
                        // Diğer olası Identity hatalarını olduğu gibi basıyoruz
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                }
                return View(model);
            }

            // KRİTİK LOG: Şifre değişti!
            _logger.LogInformation("Sistem İşlemi: {UserEmail} adlı kullanıcı şifresini başarıyla DEĞİŞTİRDİ. Oturum kapatılıyor.", user.Email);
            await _signInManager.SignOutAsync();

            return RedirectToAction("SignIn", "Login");

        }




    }
}
