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

        public async Task<IActionResult> Inbox(int page = 1, int pageSize = 12)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                _logger.LogWarning("Oturum Hatası: Gelen kutusuna erişmeye çalışan '{UserName}' isimli kullanıcı bulunamadı. Oturum kapatılıyor.", User.Identity.Name);
                await signInManager.SignOutAsync();
                return RedirectToAction("SignIn", "Login");
            }

            var messages = await _messageService.GetAllFiterWithReceiverAsync(x => x.ReceiverId == user.Id);
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("Gelen Kutusu Listelendi: Kullanıcı: {UserEmail} (ID: {UserId}), Toplam Mesaj: {TotalCount}, Sayfa: {Page}", user.Email, user.Id, messages.Count, page);

            return View(values);
        }

        public async Task<IActionResult> SendBox(int page = 1, int pageSize = 12)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                _logger.LogWarning("Oturum Hatası: Giden kutusuna erişmeye çalışan kullanıcı bulunamadı.");
                await signInManager.SignOutAsync();
                return RedirectToAction("SignIn", "Login");
            }

            var messages = await _messageService.GetAllFiterWithSenderAsync(x => x.SenderId == user.Id);
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("Giden Kutusu Listelendi: Kullanıcı: {UserEmail}, Sayfa: {Page}, Mesaj Sayısı: {Count}", user.Email, page, messages.Count);

            return View(values);
        }

        public async Task<IActionResult> ViewEmail(int id)
        {
            var messageToUpdate = await _messageService.GetByIdAsync(id);

            if (messageToUpdate == null)
            {
                _logger.LogError("Hata: Görüntülenmek istenen {MessageId} ID'li mesaj bulunamadı.", id);
                return NotFound();
            }

            if (!messageToUpdate.IsRead)
            {
                messageToUpdate.IsRead = true;
                await _messageService.UpdateAsync(messageToUpdate);
                _logger.LogInformation("Mesaj Durumu: {MessageId} ID'li mesaj ilk kez okundu olarak işaretlendi.", id);
            }

            var message = await _messageService.GetByIdWithReceiverAsync(id);
            _logger.LogInformation("Mesaj Görüntülendi: ID: {MessageId}, Konu: '{Subject}', Gönderen ID: {SenderId}", id, message.Subject, message.SenderId);

            return View(message);
        }

        public async Task<IActionResult> ReceiverViewEmail(int id)
        {
            var message = await _messageService.GetByIdWithReceiverAsync(id);
            _logger.LogInformation("Alıcı Mesaj Detayı: ID: {MessageId} görüntülendi.", id);
            return View(message);
        }

        private async Task GetCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            _logger.LogDebug("Kategori listesi yüklendi. Toplam kategori: {Count}", categories.Count);

            ViewBag.categories = (from category in categories
                                  select new SelectListItem
                                  {
                                      Text = category.Name,
                                      Value = category.Id.ToString()
                                  }).ToList();
        }

        public async Task<IActionResult> SendMessage(string? email, int? messageId)
        {
            await GetCategories();
            var model = new SendMessageViewModel();

            if (messageId.HasValue)
            {
                var draftMessage = await _messageService.GetByIdMessageForDraftAsync(messageId.Value);
                if (draftMessage != null)
                {
                    if (draftMessage.Receiver != null) model.ReceiverEmail = draftMessage.Receiver.Email;
                    model.Subject = draftMessage.Subject;
                    model.MessageDetail = draftMessage.MessageDetail;
                    model.MessageCategoryId = draftMessage.CategoryId;
                    _logger.LogInformation("Taslak Yüklendi: {MessageId} ID'li taslak düzenleme modunda açıldı.", messageId);
                }
            }
            else if (!string.IsNullOrEmpty(email))
            {
                model.ReceiverEmail = email;
                _logger.LogInformation("Yeni Mesaj: '{Email}' adresine gönderilmek üzere form açıldı.", email);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageViewModel dto)
        {
            await GetCategories();
            var sender = await _userManager.FindByNameAsync(User.Identity.Name);
            var receiver = await _userManager.FindByEmailAsync(dto.ReceiverEmail);

            if (receiver is null)
            {
                _logger.LogWarning("Geçersiz Gönderim: {SenderEmail} kullanıcısı, sistemde olmayan {ReceiverEmail} adresine yazmaya çalıştı.", sender.Email, dto.ReceiverEmail);
                ModelState.AddModelError("", "Bu maile sahip bir kullanıcı bulunamadı");
                return View(dto);
            }

            string? attachedFilePath = null;
            if (dto.AttachedFile != null && dto.AttachedFile.Length > 0)
            {
                try
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
                    _logger.LogInformation("Dosya Yüklendi: '{FileName}', Yol: {Path}", uniqueFileName, attachedFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Dosya Yükleme Hatası: {SenderEmail}", sender.Email);
                }
            }

            // 🔥 YAPAY ZEKA KONTROLÜ 🔥
            bool isSpamDetected = false;
            if (!string.IsNullOrEmpty(dto.MessageDetail))
            {
                var input = new SpamDetector.ModelInput { Message = dto.MessageDetail };
                var result = SpamDetector.Predict(input);
                if (result.PredictedLabel.ToString() == "1")
                {
                    isSpamDetected = true;
                    _logger.LogWarning("ML.NET Filtresi: {SenderEmail} tarafından gönderilen mesaj SPAM olarak işaretlendi.", sender.Email);
                }
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
                IsDeleted = false,
                IsDraft = false,
                IsSpam = isSpamDetected
            };

            await _messageService.CreateAsync(message);
            _logger.LogInformation("Mesaj Gönderildi: Gönderen: {Sender}, Alıcı: {Receiver}, Konu: '{Subject}', Ek: {HasFile}", sender.Email, receiver.Email, dto.Subject, attachedFilePath != null ? "Evet" : "Hayır");

            return RedirectToAction("SendBox");
        }

        public IActionResult DownloadAttachment(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogWarning("Geçersiz İndirme: Dosya yolu boş veya geçersiz.");
                return NotFound("Dosya yolu bulunamadı.");
            }

            var fileName = Path.GetFileName(filePath);
            var path = Path.Combine(_hostEnvironment.WebRootPath, "attachments", fileName);

            if (!System.IO.File.Exists(path))
            {
                _logger.LogCritical("Kritik Hata: Veritabanında kayıtlı dosya fiziksel sunucuda bulunamadı! Yol: {Path}", path);
                return NotFound("Dosya sunucuda bulunamadı.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            _logger.LogInformation("Dosya Başarıyla İndirildi: '{FileName}', İndiren: {UserName}", fileName, User.Identity.Name);

            return File(fileBytes, "application/octet-stream", fileName);
        }

        public async Task<IActionResult> PeopleISentMessagesTo()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.name = user.Name + " " + user.Surname;
            ViewBag.job = user.Job;
            ViewBag.image = user.ImageUrl;

            var messagesTo = await _messageService.PeopleISentMessagesTo(user.Id);
            _logger.LogInformation("Kişi Listesi Görüntülendi: {UserEmail} kullanıcısının iletişim kurduğu {Count} kişi listelendi.", user.Email, messagesTo.Count);

            return View(messagesTo);
        }

        public async Task<IActionResult> Logout()
        {
            var userEmail = User.Identity.Name;
            await signInManager.SignOutAsync();
            _logger.LogInformation("Güvenli Çıkış: {UserEmail} sistemden çıkış yaptı.", userEmail);
            return RedirectToAction("SignIn", "Login");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessages(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                _logger.LogInformation("Mesaj Silme: Herhangi bir mesaj seçilmeden silme işlemi istendi.");
                return RedirectToAction("Inbox");
            }

            foreach (var id in selectedIds)
            {
                var message = await _messageService.GetByIdAsync(id);
                if (message != null)
                {
                    message.IsDeleted = true;
                    await _messageService.UpdateAsync(message);
                }
            }

            _logger.LogInformation("Toplu Silme: {Count} adet mesaj çöp kutusuna taşındı. ID Listesi: {Ids}", selectedIds.Count, string.Join(",", selectedIds));
            return RedirectToAction("Inbox");
        }

        public async Task<IActionResult> GarbageBox(int page = 1, int pageSize = 12)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                _logger.LogWarning("Çöp Kutusu Erişimi: Kullanıcı bulunamadı.");
                await signInManager.SignOutAsync();
                return RedirectToAction("SignIn", "Login");
            }

            var messages = await _messageService.GetAllGarbageBoxAsync(user.Id);
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("Çöp Kutusu Görüntülendi: {UserEmail}, Sayfa: {Page}, Silinen Mesaj Sayısı: {Count}", user.Email, page, messages.Count);

            return View(values);
        }

        public async Task<IActionResult> GetMessagesByCategoryId(int id, int page = 1, int pageSize = 12)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var messages = await _messageService.GetMessagesByCategoryId(id, user.Id);
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("Kategori Filtreleme: Kullanıcı: {UserEmail}, Kategori ID: {CatId}, Sayfa: {Page}, Bulunan Mesaj: {Count}", user.Email, id, page, messages.Count);

            return View(values);
        }

        [HttpPost]
        public async Task<IActionResult> SaveDraftMessage(SendMessageViewModel dto)
        {
            var sender = await _userManager.FindByNameAsync(User.Identity.Name);
            int? receiverId = null;

            if (!string.IsNullOrEmpty(dto.ReceiverEmail))
            {
                var receiver = await _userManager.FindByEmailAsync(dto.ReceiverEmail);
                if (receiver != null) receiverId = Convert.ToInt32(receiver.Id);
            }

            string? attachedFilePath = null;
            // (Dosya yükleme mantığını burada aynı şekilde korudun)

            var message = new CreateMessageDto()
            {
                Subject = dto.Subject ?? "Konusuz Taslak",
                MessageDetail = dto.MessageDetail ?? "",
                ReceiverId = receiverId ?? 0,
                SenderId = Convert.ToInt32(sender.Id),
                SendDate = DateTime.Now,
                CategoryId = dto.MessageCategoryId,
                IsDeleted = false,
                IsDraft = true
            };

            await _messageService.CreateAsync(message);
            _logger.LogInformation("Taslak Kaydedildi: Kullanıcı: {UserEmail}, Konu: '{Subject}'", sender.Email, message.Subject);

            return Json(new { success = true });
        }

        public async Task<IActionResult> DraftBox(int page = 1, int pageSize = 12)
        {
            var messages = await _messageService.GetAllDrafAsync();
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("Taslaklar Listelendi: Toplam {Count} taslak bulundu. Sayfa: {Page}", messages.Count, page);

            return View(values);
        }

        public async Task<IActionResult> SpamBox(int page = 1, int pageSize = 12)
        {
            var messages = await _messageService.GetAllSpamAsync();
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("Spam Kutusu Görüntülendi: Toplam {Count} spam bulundu. Sayfa: {Page}", messages.Count, page);

            return View(values);
        }
    }
}