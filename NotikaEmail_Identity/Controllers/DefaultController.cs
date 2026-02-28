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

namespace NotikaEmail_Identity.Controllers
{
    [Authorize]
    public class DefaultController(IMessageService _messageService,
                                   UserManager<AppUser> _userManager,
                                   ICategoryService _categoryService,
                                   IWebHostEnvironment _hostEnvironment) : Controller
    {

        public async Task<IActionResult> Inbox(int page = 1,int pageSize=2)
        {




            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var messages=await _messageService.GetAllFiterWithSenderAsync(x=>x.ReceiverId==user.Id);

            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(),page, pageSize);

            return View(values);
        }


        
        public async Task<IActionResult> SendBox(int page = 1 ,int pageSize = 2)
        {

            var user = await _userManager.FindByNameAsync(User.Identity.Name);
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




        
        public async Task<IActionResult> SendMessage()
        {
           await  GetCategories();
            return View();


        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageViewModel dto)
        {
             await  GetCategories();

            var sender = await _userManager.FindByNameAsync(User.Identity.Name);


            

            var receiver = await _userManager.FindByEmailAsync(dto.ReceiverEmail);

            if(receiver is null)
            {
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
            return RedirectToAction("SendBox");



        }

        public IActionResult DownloadAttachment(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return NotFound("Dosya yolu bulunamadı.");

            // Dosya yolundan sadece ismini alıyoruz (Örn: ae1038...-tahlil.pdf)
            var fileName = System.IO.Path.GetFileName(filePath);

            // wwwroot/attachments/dosya-adi dizinini oluşturuyoruz (IWebHostEnvironment _env kullanıyoruz)
            var path = System.IO.Path.Combine(_hostEnvironment.WebRootPath, "attachments", fileName);

            // Eğer klasörde gerçekten dosya yoksa hata fırlat
            if (!System.IO.File.Exists(path))
                return NotFound("Dosya sunucuda bulunamadı.");

            // Dosyayı byte olarak okuyup tarayıcıya "octet-stream" formatında yolluyoruz
            // octet-stream tarayıcıya şunu söyler: "Bu bir indirme dosyasıdır, sakın sekmede açmaya çalışma!"
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/octet-stream", fileName);
        }


    }
}
