using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NotikaEmail_Identity.DTOs.MessageDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.Services.CategoryServices;
using NotikaEmail_Identity.Services.MessageServices;
using PagedList.Core;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize]
    public class DefaultController(IMessageService _messageService,
                                   UserManager<AppUser> _userManager,
                                   ICategoryService _categoryService,
                                   IWebHostEnvironment _hostEnvironment,
                                   ILogger<DefaultController> _logger,
                                   SignInManager<AppUser> signInManager) : Controller
    {

        public async Task<IActionResult> Inbox(int page = 1,int pageSize=2)
        {




            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("SignIn", "Login");
            }




            var messages=await _messageService.GetAllFiterWithSenderAsync(x=>x.ReceiverId==user.Id);

            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(),page, pageSize);

            return View(values);
        }


        
        public async Task<IActionResult> SendBox(int page = 1 ,int pageSize = 12)
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("SignIn", "Login");
            }

            var messages = await _messageService.GetAllFiterWithSenderAsync(x => x.SenderId == user.Id);

            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            return View(values);


        }

        public async Task<IActionResult> ViewEmail(int id)
        {
            var messageToUpdate = await _messageService.GetByIdAsync(id);
            if (!messageToUpdate.IsRead)
            {
                messageToUpdate.IsRead = true;
                await _messageService.UpdateAsync(messageToUpdate);
                // MESAJ OKUNDU LOGU (Sadece ilk defa okunduğunda log atarız, gereksiz kalabalık olmasın)
                _logger.LogInformation("Bilgi: {MessageId} ID'li mesaj ilk kez okundu olarak işaretlendi.", id);
            }

            var message = await _messageService.GetByIdWithReceiverAsync(id);

            return View(message);
        }


        public async Task<IActionResult> ReceiverViewEmail(int id)
        {


            var message = await _messageService.GetByIdWithReceiverAsync(id);

            return View(message);

        }


        private async Task GetCategories()
        {
            var categories=await _categoryService.GetAllAsync();

            ViewBag.categories = (from category in categories
                                  select new SelectListItem
                                  {
                                      Text = category.Name,
                                      Value = category.Id.ToString()


                                  }).ToList();




        }




        
        public async Task<IActionResult> SendMessage(string? email)
        {
           await  GetCategories();

            var model = new SendMessageViewModel();

         
            if (!string.IsNullOrEmpty(email))
            {
                model.ReceiverEmail = email;
            }

            return View(model);


        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageViewModel dto)
        {
             await  GetCategories();

            var sender = await _userManager.FindByNameAsync(User.Identity.Name);


            

            var receiver = await _userManager.FindByEmailAsync(dto.ReceiverEmail);

            if(receiver is null)
            {
                // ŞÜPHELİ İŞLEM LOGU: Olmayan birine mail atmaya çalışıyor!
                _logger.LogWarning("Şüpheli İşlem: {SenderEmail} adlı kullanıcı sistemde olmayan bir adrese ({ReceiverEmail}) mesaj göndermeye çalıştı.", sender.Email, dto.ReceiverEmail);


                ModelState.AddModelError("", "Bu maile sahip bir mail bulunamadı");
                return View(dto);
            }




            string? attachedFilePath = null; // Başlangıçta null atıyoruz

            // DOSYA YÜKLEME İŞLEMİ
            if (dto.AttachedFile != null && dto.AttachedFile.Length > 0)
            {
                // 1. wwwroot içinde "attachments" adında bir klasör yolu belirliyoruz
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "attachments");

                // 2. Eğer böyle bir klasör yoksa, kod bizim yerimize oluştursun
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 3. Aynı isimde dosyalar çakışmasın diye başına benzersiz bir kimlik (Guid) ekliyoruz
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.AttachedFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 4. Dosyayı sunucuya kopyalıyoruz
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.AttachedFile.CopyToAsync(fileStream);
                }

                // 5. Veritabanına kaydedilecek yolu belirliyoruz (Başına / koyuyoruz ki link olarak çalışsın)
                attachedFilePath = "/attachments/" + uniqueFileName;
            }






            var message = new CreateMessageDto()
            {
                Subject = dto.Subject,
                MessageDetail = dto.MessageDetail,
                ReceiverId = receiver.Id,
                SenderId = sender.Id,
                SendDate = DateTime.Now,
                CategoryId = dto.MessageCategoryId,
                IsRead = false,
                AttachedFilePath = attachedFilePath
            };


            await _messageService.CreateAsync(message);
            // BAŞARILI MESAJ GÖNDERİM LOGU (Ek dosya var mı yok mu onu bile yazdırdık!)
            _logger.LogInformation("Sistem İşlemi: {SenderEmail} adlı kullanıcı, {ReceiverEmail} adresine mesaj gönderdi. Konu: '{Subject}' | Ek Dosya: {HasAttachment}",
                sender.Email, receiver.Email, dto.Subject, attachedFilePath != null ? "Var" : "Yok");

            return RedirectToAction("SendBox");



        }

        public IActionResult DownloadAttachment(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogWarning("Hatalı İstek: Bir kullanıcı boş bir dosya yolunu indirmeye çalıştı.");
                return NotFound("Dosya yolu bulunamadı.");

            }

            // Dosya yolundan sadece ismini alıyoruz (Örn: ae1038...-tahlil.pdf)
            var fileName = System.IO.Path.GetFileName(filePath);

            // wwwroot/attachments/dosya-adi dizinini oluşturuyoruz (IWebHostEnvironment _env kullanıyoruz)
            var path = System.IO.Path.Combine(_hostEnvironment.WebRootPath, "attachments", fileName);

            // Eğer klasörde gerçekten dosya yoksa hata fırlat
            if (!System.IO.File.Exists(path))
            {
                // ÇOK KRİTİK LOG: Dosya veritabanında var ama klasörde yok (Biri silmiş olabilir!)
                _logger.LogError("Sistem Hatası: İndirilmek istenen '{FileName}' adlı dosya sunucu klasöründe bulunamadı! (Path: {Path})", fileName, path);
                return NotFound("Dosya sunucuda bulunamadı.");

            }

            // Dosyayı byte olarak okuyup tarayıcıya "octet-stream" formatında yolluyoruz
            // octet-stream tarayıcıya şunu söyler: "Bu bir indirme dosyasıdır, sakın sekmede açmaya çalışma!"
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);

            // DOSYA İNDİRME LOGU
            _logger.LogInformation("Sistem İşlemi: '{FileName}' adlı ek dosya sunucudan başarıyla indirildi.", fileName);

            return File(fileBytes, "application/octet-stream", fileName);
        }



        public async Task<IActionResult> PeopleISentMessagesTo()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.name = user.Name + " " + user.Surname;
            ViewBag.job = user.Job;
            ViewBag.image = user.ImageUrl;

            var messagesTo = await _messageService.PeopleISentMessagesTo(user.Id);

            return View(messagesTo);
        }





        public async Task<IActionResult> Logout()
        {

            await signInManager.SignOutAsync();
            return RedirectToAction("SignIn","Login");


        }





    }
}
