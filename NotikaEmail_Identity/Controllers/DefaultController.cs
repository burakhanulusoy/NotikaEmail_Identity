using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NotikaEmail_Identity.DTOs.MessageDtos;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Models;
using NotikaEmail_Identity.Services.CategoryServices;
using NotikaEmail_Identity.Services.MessageServices;
using PagedList.Core;
using System.Threading.Tasks;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize(Roles = "Admin, User")]

    public class DefaultController(IMessageService _messageService,
                                   UserManager<AppUser> _userManager,
                                   ICategoryService _categoryService,
                                   IWebHostEnvironment _hostEnvironment,
                                   ILogger<DefaultController> _logger,
                                   SignInManager<AppUser> signInManager) : Controller
    {

        public async Task<IActionResult> Inbox(int page = 1,int pageSize=12)
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
                _logger.LogInformation("Bilgi: {MessageId} ID'li mesaj  okundu olarak işaretlendi.", id);
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




        
        public async Task<IActionResult> SendMessage(string? email,int? messageId)
        {
           await  GetCategories();

            var model = new SendMessageViewModel();

            if (messageId.HasValue)
            {
                var draftMessage = await _messageService.GetByIdMessageForDraftAsync(messageId.Value);

                if (draftMessage != null)
                {
                    // EĞER ALICI VARSA ata, YOKSA boş geç (Hata almamak için kontrol)
                    if (draftMessage.Receiver != null)
                    {
                        model.ReceiverEmail = draftMessage.Receiver.Email;
                    }

                    model.Subject = draftMessage.Subject;
                    model.MessageDetail = draftMessage.MessageDetail;
                    model.MessageCategoryId = draftMessage.CategoryId;
                }
            }
            // EĞER ID YOKSA AMA EMAIL VARSA (Yeni mesaj butonu veya kişi listesinden tıklama)
            else if (!string.IsNullOrEmpty(email))
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
                AttachedFilePath = attachedFilePath,
                IsDeleted=false,
                IsDraft=false
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


        [HttpPost]
        public async Task<IActionResult> DeleteMessages(List<int> selectedIds)
        {
            // Eğer hiçbir şey seçilmeden post edildiyse direkt geri yolla
            if (selectedIds == null || !selectedIds.Any())
            {
                return RedirectToAction("Inbox");
            }

            // Seçilen tüm ID'leri dön
            foreach (var id in selectedIds)
            {
                var message = await _messageService.GetByIdAsync(id);

                if (message != null)
                {
                    // Mesajın çöp kutusuna gitmesi için gerekli property'i güncelle
                    message.IsDeleted = true; // Kendi property ismine göre düzelt

                    await _messageService.UpdateAsync(message);
                }
            }

            // İşlem başarıyla bitince sayfayı yenilemiş gibi Inbox'a geri dön
            return RedirectToAction("Inbox");
        }


        public async Task<IActionResult> GarbageBox(int page = 1, int pageSize = 12)
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("SignIn", "Login");
            }




            var messages = await _messageService.GetAllGarbageBoxAsync(user.Id);

            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            return View(values);


        }


        public async Task<IActionResult> GetMessagesByCategoryId(int id,int page=1,int pageSize=12)
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);


            var messages = await _messageService.GetMessagesByCategoryId(id,user.Id);
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            return View(values);

        }



        [HttpPost]
        public async Task<IActionResult> SaveDraftMessage(SendMessageViewModel dto)
        {
            // Aktif kullanıcıyı buluyoruz
            var sender = await _userManager.FindByNameAsync(User.Identity.Name);

            // Alıcı maili yazılmışsa alıcıyı bulalım, yazılmamışsa boş (null) kalsın
            int? receiverId = null;
            if (!string.IsNullOrEmpty(dto.ReceiverEmail))
            {
                var receiver = await _userManager.FindByEmailAsync(dto.ReceiverEmail);
                if (receiver != null)
                {
                    // Identity'den gelen ID string ise int'e çeviriyoruz (CS0029 Hata Çözümü)
                    receiverId = Convert.ToInt32(receiver.Id);
                }
            }

            string? attachedFilePath = null;

            // Dosya varsa onu da kaydedelim (Senin yazdığın kodun aynısı)
            if (dto.AttachedFile != null && dto.AttachedFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "attachments");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.AttachedFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.AttachedFile.CopyToAsync(fileStream);
                }
                attachedFilePath = "/attachments/" + uniqueFileName;
            }

            // Mesajı Taslak Olarak Oluşturuyoruz
            var message = new CreateMessageDto()
            {
                Subject = dto.Subject ?? "Konusuz Taslak", // Konu boşsa default isim ata
                MessageDetail = dto.MessageDetail ?? "",

                // Eğer Message entity'sinde ReceiverId nullable (int?) DEĞİLSE ve sistem hata verirse, 
                // buraya geçici olarak sender'ın kendi ID'sini atayabiliriz. Ama şimdilik böyle kalsın.
                ReceiverId = receiverId ?? 0,

                SenderId = Convert.ToInt32(sender.Id), // CS0029 Hata Çözümü
                SendDate = DateTime.Now,
                CategoryId = dto.MessageCategoryId,
                AttachedFilePath = attachedFilePath,
                IsDeleted = false,
                IsDraft = true // İŞTE CAN ALICI NOKTA: Bunu true yapıyoruz ki taslak olsun!
            };

            // Veritabanına Kaydet
            await _messageService.CreateAsync(message);

            // Javascript'e "İşlem Başarılı" mesajı dön
            return Json(new { success = true });
        }



        public async Task<IActionResult> DraftBox(int page = 1, int pageSize = 12)
        {
            var messages=await _messageService.GetAllDrafAsync();
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            return View(values);


        }





    }
}
