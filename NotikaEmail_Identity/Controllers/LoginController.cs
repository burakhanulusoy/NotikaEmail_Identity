using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.RoleNames;
using System.Security.Claims;

namespace NotikaEmail_Identity.Controllers
{
    public class LoginController(SignInManager<AppUser> _signInManager,
        UserManager<AppUser> _userManager, 
        ILogger<LoginController> _logger
        ) : Controller
    {
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(LoginUserViewModel model)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Giriş denemesi başladığı an logluyoruz (Debug seviyesi yeterli, console'u boğmasın)
            _logger.LogDebug("🔑 GİRİŞ DENEMESİ: {Email} giriş butonuna bastı. IP: {Ip}", model.Email, ipAddress);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null)
            {
                // Güvenlik: Olmayan mailleri deneyenleri tespit etmek için Warning
                _logger.LogWarning("⚠️ GEÇERSİZ KULLANICI: Sistemde kayıtlı olmayan '{Email}' adresiyle giriş denendi. IP: {Ip}", model.Email, ipAddress);

                ModelState.AddModelError("", "Bu emaile kayıtlı hesap bulunamadı.");
                return View(model);
            }

            bool hasPassword = await _userManager.HasPasswordAsync(user);

            if (!hasPassword)
            {
                _logger.LogWarning("🚫 ŞİFRESİZ ERİŞİM DENEMESİ: {Email} (Google/Face hesaplı) şifre ile girmeye çalıştı.", user.Email);
                ModelState.AddModelError("", "Bu hesap Google/Facebook ile oluşturulmuş ve henüz bir şifre belirlenmemiş. Lütfen 'Şifremi unuttum' diyerek şifre oluşturun veya Sosyal Medya butonu ile giriş yapın.");
                return View(model);
            }

            // Lockout (Kilitleme) mekanizması aktifleştirildi (4. parametre true yapıldı)
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (user.EmailConfirmed is false)
                {
                    _logger.LogWarning("⏳ ONAYSIZ GİRİŞ: {Email} şifresi doğru ama maili onaylamamış. Giriş engellendi.", user.Email);
                    await _signInManager.SignOutAsync(); // İçeri almadan atıyoruz
                    ModelState.AddModelError("", "Kayıtlı emailiniz aktifleştirilmemiş lütfen kontrol edin.");
                    return View(model);
                }

                if (user.IsActive is true)
                {
                    _logger.LogWarning("⏳ ONAYSIZ GİRİŞ: {Email} Pasif edilmiş hesap. Giriş engellendi.", user.Email);
                    await _signInManager.SignOutAsync(); // İçeri almadan atıyoruz
                    ModelState.AddModelError("", "Kayıtlı hesabınız kapatılmıştır lütfen kontrol edin.");
                    return View(model);
                }


                var roles=await  _userManager.GetRolesAsync(user);
             
                if (roles.Contains(Roles.Admin))
                {


                    _logger.LogInformation("✅ BAŞARILI GİRİŞ ADMİN: {UserEmail} sisteme giriş yaptı. IP: {Ip}", user.Email, ipAddress);
                    return RedirectToAction("Index", "Dashboard",new {area="Admin"});

                }

                if (roles.Contains(Roles.User))
                {
                    _logger.LogInformation("✅ BAŞARILI GİRİŞ USER: {UserEmail} sisteme giriş yaptı. IP: {Ip}", user.Email, ipAddress);
                    return RedirectToAction("Inbox", "Default");

                }


                return RedirectToAction("SigIn", "Login");



            }

            if (result.IsLockedOut)
            {
                _logger.LogCritical("🔒 HESAP KİLİTLENDİ: {Email} çok fazla hatalı deneme yaptığı için geçici olarak kilitlendi. IP: {Ip}", user.Email, ipAddress);
                ModelState.AddModelError("", "Çok fazla hatalı giriş yaptınız. Hesabınız geçici olarak kilitlendi. Lütfen 5 dakika sonra tekrar deneyiniz.");
                return View(model);
            }
            else
            {
                // HATA LOGU (Seq'te sarı renkte dikkat çekecek!)
                _logger.LogWarning("🛑 HATALI ŞİFRE: {UserEmail} yanlış şifre girdi. IP: {Ip}", model.Email, ipAddress);
                ModelState.AddModelError("", "Email veya şifre hatalı.");
                return View(model);
            }
        }






















        // ==========================================
        // 1. GOOGLE LOGIN SÜRECİ
        // ==========================================
        [HttpGet]
        public IActionResult GoogleLogin()
        {
            string redirectUrl = Url.Action("GoogleResponse", "Login");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            _logger.LogInformation("🌐 GOOGLE YÖNLENDİRME: Bir kullanıcı Google girişine tıkladı.");
            return new ChallengeResult("Google", properties);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (info == null)
            {
                _logger.LogError("❌ GOOGLE HATASI: ExternalLoginInfo null döndü. Google tarafında veya callback url'de sorun olabilir.");
                return RedirectToAction("SignIn");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            // 1. İHTİMAL: Zaten bağlı mı?
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                _logger.LogInformation("✅ GOOGLE İLE GİRİŞ: {Email} başarıyla giriş yaptı. ProviderKey: {Key}", email, info.ProviderKey);
                return RedirectToAction("Inbox", "Default");
            }

            if (signInResult.IsLockedOut)
            {
                _logger.LogWarning("🔒 KİLİTLİ HESAP (GOOGLE): {Email} kilitli olduğu için Google ile de giremedi.", email);
                return RedirectToAction("SignIn");
            }

            // 2. İHTİMAL: İlk defa geliyor veya hesap var ama Google bağlı değil.
            string firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? info.Principal.FindFirstValue(ClaimTypes.Name) ?? "GoogleUser";
            string lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    // --- YENİ KULLANICI OLUŞTURMA ---
                    string generatedUserName = email.Split('@')[0]; // Örn: ahmet123

                    user = new AppUser
                    {
                        Email = email,
                        UserName = generatedUserName,
                        Name = firstName,
                        Surname = lastName,
                        EmailConfirmed = true, // Google zaten onayladı
                        CreatedDate = DateTime.Now,
                        PhoneNumber = "Belirtilmemiş",
                        Job = "Belirtilmemiş",
                        AboutMe = "Google ile kayıt oldu.",
                        City = "Belirtilmemiş",
                        ActivationCode = 0 // Onaylı olduğu için önemsiz
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddLoginAsync(user, info); // Google'ı bağla
                        await _userManager.AddToRoleAsync(user, "User"); // Rol ata

                        _logger.LogInformation("🆕 YENİ KAYIT (GOOGLE): {Email} için otomatik hesap oluşturuldu ve giriş yapıldı. IP: {Ip}", email, ipAddress);

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Inbox", "Default");
                    }
                    else
                    {
                        string errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        _logger.LogError("🔥 KAYIT HATASI (GOOGLE): {Email} oluşturulurken hata çıktı: {Errors}", email, errors);
                    }
                }
                else
                {
                    // --- HESAP VAR, EŞLEŞTİRME ---
                    var addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (addLoginResult.Succeeded)
                    {
                        _logger.LogInformation("🔗 HESAP EŞLEŞTİRİLDİ: {Email} mevcut hesabını Google ile bağladı.", email);
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Inbox", "Default");
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ EŞLEŞTİRME HATASI: {Email} hesabı Google ile bağlanamadı.", email);
                    }
                }
            }
            return RedirectToAction("SignIn");
        }

        // ==========================================
        // 2. FACEBOOK LOGIN SÜRECİ
        // ==========================================
        [HttpGet]
        public IActionResult FacebookLogin()
        {
            string redirectUrl = Url.Action("FacebookResponse", "Login");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Facebook", redirectUrl);
            _logger.LogInformation("📘 FACEBOOK YÖNLENDİRME: Bir kullanıcı Facebook girişine tıkladı.");
            return new ChallengeResult("Facebook", properties);
        }

        [HttpGet]
        public async Task<IActionResult> FacebookResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            if (info == null)
            {
                _logger.LogError("❌ FACEBOOK HATASI: ExternalLoginInfo null döndü.");
                return RedirectToAction("SignIn");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            // 1. İHTİMAL: Zaten bağlı mı?
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                _logger.LogInformation("✅ FACEBOOK İLE GİRİŞ: {Email} başarıyla giriş yaptı.", email ?? "Emailsiz Kullanıcı");
                return RedirectToAction("Inbox", "Default");
            }

            // 2. İHTİMAL: Yeni Kayıt / Eşleştirme
            string firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? info.Principal.FindFirstValue(ClaimTypes.Name) ?? "FacebookUser";
            string lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

            // Facebook bazen email vermez (Sadece telefonla kayıtlıysa). Bu durumda fake email üretmek zorundayız.
            if (string.IsNullOrEmpty(email))
            {
                email = $"{info.ProviderKey}@facebook.user";
                _logger.LogWarning("⚠️ E-POSTA YOK: Facebook'tan email gelmedi. Geçici ID atandı: {FakeEmail}", email);
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // YENİ KAYIT
                string generatedUserName = email.Contains("@") ? email.Split('@')[0] : "user" + new Random().Next(1000, 9999);

                user = new AppUser
                {
                    Email = email,
                    UserName = generatedUserName,
                    Name = firstName,
                    Surname = lastName,
                    EmailConfirmed = true,
                    CreatedDate = DateTime.Now,
                    PhoneNumber = "Belirtilmemiş",
                    Job = "Belirtilmemiş",
                    AboutMe = "Facebook ile kayıt oldu.",
                    City = "Belirtilmemiş",
                    ActivationCode = 0
                };

                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    await _userManager.AddLoginAsync(user, info);
                    await _userManager.AddToRoleAsync(user, "User");

                    _logger.LogInformation("🆕 YENİ KAYIT (FACEBOOK): {Email} için hesap oluşturuldu. IP: {Ip}", email, ipAddress);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Inbox", "Default");
                }
            }
            else
            {
                // VAR OLANI BAĞLA
                await _userManager.AddLoginAsync(user, info);

                _logger.LogInformation("🔗 HESAP EŞLEŞTİRİLDİ: {Email} Facebook ile bağlandı.", email);

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Inbox", "Default");
            }

            return RedirectToAction("SignIn");
        }
    }
}