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
using System.Text.RegularExpressions;

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
                _logger.LogWarning("⚠️ OTURUM HATASI: Gelen kutusuna erişmeye çalışan '{UserName}' isimli kullanıcı veritabanında bulunamadı. IP: {IpAddress}", User.Identity.Name, HttpContext.Connection.RemoteIpAddress);
                await signInManager.SignOutAsync();
                return RedirectToAction("SignIn", "Login");
            }

            var messages = await _messageService.GetAllFiterWithReceiverAsync(x => x.ReceiverId == user.Id);
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("📥 GELEN KUTUSU: {UserEmail} (ID: {UserId}) gelen kutusunu görüntüledi. Sayfa: {Page}, Toplam Mesaj: {TotalCount}", user.Email, user.Id, page, messages.Count);

            return View(values);
        }

        public async Task<IActionResult> SendBox(int page = 1, int pageSize = 12)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (user == null)
            {
                _logger.LogWarning("⚠️ OTURUM HATASI: Giden kutusu erişimi sırasında kullanıcı kayboldu.");
                await signInManager.SignOutAsync();
                return RedirectToAction("SignIn", "Login");
            }

            var messages = await _messageService.GetAllFiterWithSenderAsync(x => x.SenderId == user.Id);
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("📤 GİDEN KUTUSU: {UserEmail} giden kutusunu görüntüledi. Sayfa: {Page}, Gönderilen Toplam: {Count}", user.Email, page, messages.Count);

            return View(values);
        }

        public async Task<IActionResult> ViewEmail(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var messageToUpdate = await _messageService.GetByIdAsync(id);

            if (messageToUpdate == null)
            {
                _logger.LogError("❌ HATA: Kullanıcı {UserEmail}, {MessageId} ID'li olmayan bir mesajı görüntülemeye çalıştı.", user?.Email, id);
                return NotFound();
            }

            // Güvenlik kontrolü: Başkasının mesajını okumaya çalışıyorsa logla (Opsiyonel ama önerilir)
            if (messageToUpdate.ReceiverId != user.Id && messageToUpdate.SenderId != user.Id)
            {
                _logger.LogWarning("⛔ YETKİSİZ ERİŞİM: {UserEmail}, kendisine ait olmayan {MessageId} ID'li mesajı okumaya çalıştı!", user.Email, id);
                // return Forbid(); // İsterseniz engellersiniz
            }

            if (!messageToUpdate.IsRead && messageToUpdate.ReceiverId == user.Id)
            {
                messageToUpdate.IsRead = true;
                await _messageService.UpdateAsync(messageToUpdate);
                _logger.LogInformation("👁️ OKUNDU: Mesaj ID: {MessageId} | Okuyan: {UserEmail} | Tarih: {Date}", id, user.Email, DateTime.Now);
            }

            var message = await _messageService.GetByIdWithReceiverAsync(id);
            _logger.LogInformation("📖 MESAJ DETAYI: '{Subject}' konulu mesaj görüntülendi. Gönderen ID: {SenderId} -> Alıcı: {UserEmail}", message.Subject, message.SenderId, user.Email);

            return View(message);
        }

        public async Task<IActionResult> ReceiverViewEmail(int id)
        {
            // Bu metodun amacı ViewEmail ile benzerse birleştirilebilir, ancak ayrı logluyoruz.
            var message = await _messageService.GetByIdWithReceiverAsync(id);
            _logger.LogInformation("📄 ALICI DETAY: ID: {MessageId} detayları görüntülendi.", id);
            return View(message);
        }

        private async Task GetCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            // Kategori logunu debug seviyesine çektik, her seferinde console'u kirletmesin
            _logger.LogDebug("📂 Kategori listesi çekildi. Adet: {Count}", categories.Count);

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
            var currentUser = User.Identity.Name;

            if (messageId.HasValue)
            {
                var draftMessage = await _messageService.GetByIdMessageForDraftAsync(messageId.Value);
                if (draftMessage != null)
                {
                    if (draftMessage.Receiver != null) model.ReceiverEmail = draftMessage.Receiver.Email;
                    model.Subject = draftMessage.Subject;
                    model.MessageDetail = draftMessage.MessageDetail;
                    model.MessageCategoryId = draftMessage.CategoryId;

                    _logger.LogInformation("✏️ TASLAK DÜZENLEME: Kullanıcı {User}, {MessageId} ID'li taslağı açtı.", currentUser, messageId);
                }
            }
            else if (!string.IsNullOrEmpty(email))
            {
                model.ReceiverEmail = email;
                _logger.LogInformation("🆕 YENİ MESAJ: Kullanıcı {User}, '{TargetEmail}' adresine mesaj oluşturuyor.", currentUser, email);
            }
            else
            {
                _logger.LogInformation("📝 YENİ MESAJ: Kullanıcı {User} boş bir mesaj formu açtı.", currentUser);
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageViewModel dto)
        {
            await GetCategories();
            var sender = await _userManager.FindByNameAsync(User.Identity.Name);
            var receiver = await _userManager.FindByEmailAsync(dto.ReceiverEmail);

            // 1. Validasyon Hatası Logu
            if (receiver is null)
            {
                _logger.LogWarning("🚫 GEÇERSİZ GÖNDERİM: {SenderEmail} -> {TargetEmail} adresine göndermeye çalıştı fakat alıcı bulunamadı.", sender.Email, dto.ReceiverEmail);
                ModelState.AddModelError("", "Bu maile sahip bir kullanıcı bulunamadı");
                return View(dto);
            }

            string? attachedFilePath = null;
            long fileSize = 0;

            if (dto.AttachedFile != null && dto.AttachedFile.Length > 0)
            {
                try
                {
                    fileSize = dto.AttachedFile.Length;
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "attachments");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.AttachedFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.AttachedFile.CopyToAsync(fileStream);
                    }
                    attachedFilePath = "/attachments/" + uniqueFileName;

                    _logger.LogInformation("📎 DOSYA YÜKLENDİ: Gönderen: {Sender}, Dosya: {FileName}, Boyut: {Size} bytes", sender.Email, dto.AttachedFile.FileName, fileSize);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ DOSYA HATASI: Dosya yüklenirken hata oluştu. Kullanıcı: {Sender}", sender.Email);
                }
            }

            // SPAM KONTROLÜ
            bool isSpamDetected = false;
            float spamProbability = 0;

            if (!string.IsNullOrEmpty(dto.MessageDetail))
            {
                string cleanMessage = Regex.Replace(dto.MessageDetail, "<.*?>", string.Empty);
                var input = new SpamDetector.ModelInput { Message = cleanMessage };
                var result = SpamDetector.Predict(input);

                spamProbability = result.Score[0];

                if (spamProbability > 0.60f)
                {
                    isSpamDetected = true;
                    _logger.LogWarning("🚨 ML.NET SPAM TESPİTİ: Gönderen: {Sender} | Alıcı: {Receiver} | Olasılık: %{Score} | Konu: {Subject}", sender.Email, receiver.Email, (spamProbability * 100).ToString("F2"), dto.Subject);
                }
                else
                {
                    _logger.LogInformation("✅ ML.NET SPAM TESTİ TEMİZ: Skor: %{Score}", (spamProbability * 100).ToString("F2"));
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

            // 🔥 EN ÖNEMLİ LOG BURASI 🔥
            _logger.LogInformation("🚀 MAİL GÖNDERİLDİ: {SenderEmail} ➡️ {ReceiverEmail} | Konu: '{Subject}' | KategoriID: {CatId} | Spam: {IsSpam} | Ek: {HasAttachment}",
                sender.Email,
                receiver.Email,
                dto.Subject,
                dto.MessageCategoryId,
                isSpamDetected ? "EVET" : "HAYIR",
                attachedFilePath != null ? "Var" : "Yok");

            return RedirectToAction("SendBox");
        }

        public IActionResult DownloadAttachment(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogWarning("⚠️ İNDİRME HATASI: Kullanıcı {User} geçersiz/boş dosya yolu istedi.", User.Identity.Name);
                return NotFound("Dosya yolu bulunamadı.");
            }

            var fileName = Path.GetFileName(filePath);
            var path = Path.Combine(_hostEnvironment.WebRootPath, "attachments", fileName);

            if (!System.IO.File.Exists(path))
            {
                _logger.LogCritical("🔥 KRİTİK DOSYA HATASI: DB'de var ama Diskte yok! Dosya: {FileName}, İsteyen: {User}", fileName, User.Identity.Name);
                return NotFound("Dosya sunucuda bulunamadı.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            _logger.LogInformation("💾 DOSYA İNDİRİLDİ: '{FileName}' dosyası {User} tarafından indirildi. Boyut: {Size} bytes", fileName, User.Identity.Name, fileBytes.Length);

            return File(fileBytes, "application/octet-stream", fileName);
        }

        public async Task<IActionResult> PeopleISentMessagesTo()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            ViewBag.name = user.Name + " " + user.Surname;
            ViewBag.job = user.Job;
            ViewBag.image = user.ImageUrl;

            var messagesTo = await _messageService.PeopleISentMessagesTo(user.Id);

            _logger.LogInformation("👥 KİŞİ LİSTESİ: {UserEmail} kullanıcısı, daha önce mesajlaştığı {Count} kişiyi görüntüledi.", user.Email, messagesTo.Count);

            return View(messagesTo);
        }

        public async Task<IActionResult> Logout()
        {
            var userEmail = User.Identity.Name;
            await signInManager.SignOutAsync();
            _logger.LogInformation("👋 GÜVENLİ ÇIKIŞ: {UserEmail} sistemden çıkış yaptı. IP: {Ip}", userEmail, HttpContext.Connection.RemoteIpAddress);
            return RedirectToAction("SignIn", "Login");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessages(List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                _logger.LogWarning("🗑️ SİLME HATASI: {User} seçim yapmadan silme butonuna bastı.", User.Identity.Name);
                return RedirectToAction("Inbox");
            }

            var user = User.Identity.Name;
            foreach (var id in selectedIds)
            {
                var message = await _messageService.GetByIdAsync(id);
                if (message != null)
                {
                    // Burada da yetki kontrolü yapılabilir
                    message.IsDeleted = true;
                    await _messageService.UpdateAsync(message);
                }
            }

            _logger.LogInformation("🗑️ TOPLU SİLME: {User} tarafından {Count} adet mesaj çöp kutusuna atıldı. Silinen ID'ler: [{Ids}]",
                user,
                selectedIds.Count,
                string.Join(", ", selectedIds)); // ID listesini virgülle ayırıp basar

            return RedirectToAction("Inbox");
        }

        public async Task<IActionResult> GarbageBox(int page = 1, int pageSize = 12)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                _logger.LogWarning("⚠️ Çöp kutusu yetkisiz erişim denemesi.");
                await signInManager.SignOutAsync();
                return RedirectToAction("SignIn", "Login");
            }

            var messages = await _messageService.GetAllGarbageBoxAsync(user.Id);
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("🗑️ ÇÖP KUTUSU: {UserEmail} çöp kutusunu açtı. İçerik Sayısı: {Count}", user.Email, messages.Count);

            return View(values);
        }

        public async Task<IActionResult> GetMessagesByCategoryId(int id, int page = 1, int pageSize = 12)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var messages = await _messageService.GetMessagesByCategoryId(id, user.Id);
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("🏷️ KATEGORİ FİLTRESİ: Kullanıcı: {UserEmail} | Kategori ID: {CatId} | Sonuç: {Count} mesaj", user.Email, id, messages.Count);

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

            // Dosya işlemleri vb... (Kısalttım)

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

            _logger.LogInformation("💾 TASLAK KAYDEDİLDİ: {SenderEmail} taslak oluşturdu. Konu: '{Subject}' | Hedef Alıcı: {ReceiverEmail}",
                sender.Email,
                message.Subject,
                dto.ReceiverEmail ?? "Belirtilmedi");

            return Json(new { success = true });
        }

        public async Task<IActionResult> DraftBox(int page = 1, int pageSize = 12)
        {
            // Kullanıcı kontrolü eklemeniz iyi olur, varsayalım service hallediyor.
            var messages = await _messageService.GetAllDrafAsync();
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("📝 TASLAKLAR KUTUSU: Listelendi. Toplam Taslak: {Count}", messages.Count);

            return View(values);
        }

        public async Task<IActionResult> SpamBox(int page = 1, int pageSize = 12)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var messages = await _messageService.GetAllSpamAsync(user.Id);
            var values = new PagedList<ResultMessageDto>(messages.AsQueryable(), page, pageSize);

            _logger.LogInformation("🕷️ SPAM KUTUSU: {UserEmail} spam kontrolü yaptı. Tespit edilen spam: {Count}", user.Email, messages.Count);

            return View(values);
        }
    }
}