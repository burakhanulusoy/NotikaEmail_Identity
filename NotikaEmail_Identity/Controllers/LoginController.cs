using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using System.Security.Claims;

namespace NotikaEmail_Identity.Controllers
{
    public class LoginController(SignInManager<AppUser> _signInManager,UserManager<AppUser> _userManager) : Controller
    {


        public IActionResult SignIn()
        { 



            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SignIn(LoginUserViewModel model)
        {

            var user=await _userManager.FindByEmailAsync(model.Email);

            if( user is null )
            {

                ModelState.AddModelError("","Bu emaile kayıtlı hesap bulunamadı.");
                return View(model);
            }

            bool hasPassword = await _userManager.HasPasswordAsync(user);
            
            if (!hasPassword)
            {
                ModelState.AddModelError("", "Bu hesap Google ile oluşturulmuş ve henüz bir şifre belirlenmemiş. Lütfen 'Şifremi unuttum' diyerek şifre oluşturun veya Google butonu ile giriş yapın.");
                return View(model);
            }
            //beni hatırla,her yablıs gırdıgınde sayac artıyor ona gor ehesap kılutlebır 10 dk
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

            if(!result.Succeeded)
            {
                ModelState.AddModelError("", "Email veya şifre hatalı.");
                return View(model);
            }

            if (user.EmailConfirmed is false)
            {
                ModelState.AddModelError("", "Kayıtlı emailiniz aktifleştirilmemiş lütfen kontrol edin.");
                return View(model);
            }




            return RedirectToAction("Inbox", "Default");


        }




        // 1. KULLANICIYI GOOGLE'A GÖNDEREN METOT
        [HttpGet]
        public IActionResult GoogleLogin()
        {
            // Google'dan dönüş yapacağı adresi belirliyoruz (Aşağıdaki GoogleResponse metodu)
            string redirectUrl = Url.Action("GoogleResponse", "Login");

            // Identity'nin Google için hazırladığı özellikleri alıyoruz
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);

            // Kullanıcıyı Google'ın giriş ekranına fırlatıyoruz
            return new ChallengeResult("Google", properties);
        }

        // 2. GOOGLE'DAN DÖNÜŞTE KARŞILAYAN METOT
        [HttpGet]
        public async Task<IActionResult> GoogleResponse()
        {
            // Google'dan gelen kullanıcı bilgilerini (Email, Ad vb.) okuyoruz
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return RedirectToAction("SignIn"); // Bir hata olduysa geri dön
            }

            // 1. İHTİMAL: Bu adam daha önce Google ile giriş yapmış mı?
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return RedirectToAction("Inbox", "Default"); // Direkt içeri al
            }

            // 2. İHTİMAL: İlk defa Google ile geliyor! (Kayıt + Giriş)
            if (!signInResult.Succeeded)
            {
                // Google'dan gerekli tüm bilgileri detaylıca çekiyoruz
                string email = info.Principal.FindFirstValue(ClaimTypes.Email);
                string firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                string lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);

                // İsim veya soyisim boş gelirse diye güvenlik önlemi (Name claim'i ad+soyadı birleşik verir)
                if (string.IsNullOrEmpty(firstName)) firstName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? "Kullanıcı";
                if (string.IsNullOrEmpty(lastName)) lastName = "";

                if (email != null)
                {
                    // Mail adresinin @ işaretinden önceki kısmını Kullanıcı Adı (UserName) yapıyoruz
                    string generatedUserName = email.Split('@')[0];

                    // Veritabanımızda bu mailde biri var mı bakıyoruz
                    var user = await _userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        // Yoksa arka planda ona otomatik, gerçek bilgileriyle ve şifresiz bir hesap açıyoruz!
                        user = new AppUser
                        {
                            Email = email,
                            UserName = generatedUserName, // Örn: ahmet.yilmaz
                            Name = firstName,             // Örn: Ahmet
                            Surname = lastName,           // Örn: Yılmaz
                            EmailConfirmed = true,        // Google'dan geldiği için doğrulama yapmıyoruz
                            CreatedDate = DateTime.Now,
                            PhoneNumber = "Belirtilmemiş",
                            Job = "Belirtilmemiş",
                            AboutMe = "Belirtilmemiş",
                            City="Belirtilmemiş",
                            ActivationCode = 123456       // Gerekli bir alan olduğu için formalite
                        };

                        var createResult = await _userManager.CreateAsync(user);
                        if (createResult.Succeeded)
                        {
                            // Kullanıcıyı oluşturduk, şimdi Google hesabı ile eşleştiriyoruz
                            await _userManager.AddLoginAsync(user, info);
                        }
                    }
                    else
                    {
                        // Kullanıcı var ama daha önce Google bağlamamışsa, hesabını Google ile eşleştir
                        await _userManager.AddLoginAsync(user, info);
                    }

                    // Son olarak adamı oturum açmış şekilde sisteme alıyoruz
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Inbox", "Default");
                }
            }

            // Herhangi bir aksilikte SignIn'e geri fırlat
            return RedirectToAction("SignIn");
        }







    }
}
