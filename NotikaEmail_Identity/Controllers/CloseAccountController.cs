using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize(Roles ="Admin,User")]
    public class CloseAccountController(UserManager<AppUser> _userManager) : Controller
    {
        public async Task<IActionResult> Close()
        {

            Random random = new Random();
            string newCode = random.Next(100000, 999999).ToString(); 



            var user = await _userManager.FindByNameAsync(User.Identity.Name);


            MimeMessage mimeMessage = new MimeMessage();

            MailboxAddress mailboxAddressFrom = new MailboxAddress("NotikaApp", "burakhanulusoy18@gmail.com");//kimden gidecek 

            mimeMessage.From.Add(mailboxAddressFrom);

            MailboxAddress mailboxAddressTo = new MailboxAddress("User", user.Email);

            mimeMessage.To.Add(mailboxAddressTo);


            var bodyBuilder = new BodyBuilder();

            bodyBuilder.TextBody = "Hesabınızı kapatmak için gelen onay kodu :" + newCode;

            TempData["code"]=newCode;

            mimeMessage.Body = bodyBuilder.ToMessageBody();

            mimeMessage.Subject = "Mail notika Hesap kapama kodu";

            SmtpClient client = new SmtpClient();//transfer protokulı

            client.Connect("smtp.gmail.com", 587, false);

            client.Authenticate("burakhanulusoy18@gmail.com", "jdargwuprysuesew");

            client.Send(mimeMessage);

            client.Disconnect(true);



            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Close(CloseAccountViewModel model)
        {
            // TempData'yı silinmekten korumak için Keep veya Peek kullanmalıyız.
            // Peek: Okur ama silinmesi için işaretlemez.
            var tempCode = TempData.Peek("code")?.ToString();

            if (string.IsNullOrEmpty(tempCode))
            {
                // Kod session süresi bittiği için silinmiş olabilir
                TempData["Error"] = "Süre aşımı. Lütfen kodu tekrar isteyin.";
                return RedirectToAction("Close"); // Sayfayı yeniletip tekrar mail attırabiliriz veya anasayfaya
            }

            // Model validasyonu ve Kod karşılaştırması
            // model.Code null gelirse patlamaması için kontrol ekledik
            if (!ModelState.IsValid || model.Code != tempCode)
            {
                ModelState.AddModelError("", "Hatalı aktivasyon kodu girdiniz.");
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            // Kullanıcıyı pasife çek
            user.IsActive = true;
            // Eğer veritabanında kod tutuyorsanız onu da temizleyebilirsiniz: user.ActivationCode = null;

            await _userManager.UpdateAsync(user);

            // Oturumu kapat
            // await _signInManager.SignOutAsync(); // (Eğer SignInManager inject ettiyseniz burası lazım)

            return RedirectToAction("SignIn", "Login");
        }
    }
}
