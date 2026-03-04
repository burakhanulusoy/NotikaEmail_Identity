using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Services.MessageServices;
using System.Text;
using System.Text.Json;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize(Roles = "Admin, User")]
    public class CloudeAiController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMessageService _messageService;
        // DÜZELTME: Logger sınıf ismi Controller ismiyle aynı olmalı
        private readonly ILogger<CloudeAiController> _logger;

        public CloudeAiController(IConfiguration configuration,
                                  IHttpClientFactory httpClientFactory,
                                  UserManager<AppUser> userManager,
                                  IMessageService messageService,
                                  ILogger<CloudeAiController> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _userManager = userManager;
            _messageService = messageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userName = User.Identity?.Name;
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                // LOG: Kullanıcı bulunamadı (Warning)
                _logger.LogWarning("CloudeAi/Index: Kullanıcı bulunamadı. Talep eden: {UserName}", userName);
                return NotFound("Kullanıcı bulunamadı.");
            }

            ViewBag.name = user.Name + " " + user.Surname;
            ViewBag.email = user.Email;
            ViewBag.imageUrl = user.ImageUrl;
            ViewBag.createdDate = user.CreatedDate;

            var lastMsg = await _messageService.GetMessageUserSendDateAsync(user.Id);
            ViewBag.lastDateSend = lastMsg != null ? lastMsg.SendDate.ToShortDateString() : "Henüz yok";

            // LOG: Sayfa başarıyla yüklendi (Info)
            _logger.LogInformation("CloudeAi Sayfası görüntülendi: {Email}", user.Email);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AskClaude([FromBody] JsonElement data)
        {
            // LOG: İstek geldi
            _logger.LogInformation("AskClaude endpointine istek geldi. Kullanıcı: {User}", User.Identity?.Name);

            try
            {
                // JSON property kontrolü
                if (!data.TryGetProperty("prompt", out var promptElement))
                {
                    _logger.LogWarning("AskClaude: JSON içinde 'prompt' alanı eksik.");
                    return BadRequest("Prompt verisi eksik.");
                }

                string prompt = promptElement.GetString();
                string apiKey = _configuration["MyCloudeApiKey:Key"];

                if (string.IsNullOrEmpty(prompt))
                {
                    _logger.LogWarning("AskClaude: Kullanıcı boş soru gönderdi.");
                    return BadRequest("Soru boş olamaz.");
                }

                if (string.IsNullOrEmpty(apiKey))
                {
                    // LOG: Kritik Hata - API Key yok (Error)
                    _logger.LogError("AskClaude: API Anahtarı appsettings.json dosyasında bulunamadı!");
                    return StatusCode(500, "API Anahtarı bulunamadı! Appsettings.json kontrol edilmeli.");
                }

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api.anthropic.com");

                // Header eklerken hata almamak için kontrol
                if (!client.DefaultRequestHeaders.Contains("x-api-key"))
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);

                if (!client.DefaultRequestHeaders.Contains("anthropic-version"))
                    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                // System Prompt
                string systemInstruction = @"
You are 'Notika AI', an intelligent assistant embedded within the Notika Email Application.
You have TWO primary roles. You must intelligently detect which role is needed based on the user's input.

ROLE 1: CUSTOMER SUPPORT
- If the user asks for help regarding the app (e.g., 'cannot send email', 'reset password', 'sync issues'), act as a Technical Support Agent.
- Provide step-by-step, polite solutions related to Notika.
- If the question is unrelated to Notika or Emailing (e.g., 'recipe for pasta', 'who won the match'), politely refuse: 'I am Notika AI. I can only assist with app support or drafting emails.'

ROLE 2: PROFESSIONAL EMAIL WRITER / EDITOR
- If the user asks you to write, rewrite, reply to, or polish an email (e.g., 'Write a formal apology for late reply', 'Make this text more professional', 'Draft a cold email for sales'), act as a World-Class Copywriter.
- Create professional, clear, and well-structured email content.
- Format the output clearly so the user can copy-paste it easily.

CRITICAL RULES:
1. Detect the language automatically. If the user writes in Turkish, reply in Turkish. If English, reply in English.
2. Be concise and helpful.
";

                // Claude API Request Body
                var requestBody = new
                {
                    
                    model = "claude-sonnet-4-20250514",
                    max_tokens = 1024,
                    system = systemInstruction,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                // API Çağrısı Başlıyor
                _logger.LogInformation("Claude API'ye istek gönderiliyor...");

                var response = await client.PostAsync("v1/messages", jsonContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // LOG: API'den hata döndü (Error)
                    _logger.LogError("Claude API Hatası! Status: {StatusCode}, Response: {Response}", response.StatusCode, responseString);
                    return BadRequest(new { answer = $"Claude API Hatası: {response.StatusCode} - {responseString}" });
                }

                using var doc = JsonDocument.Parse(responseString);

                if (doc.RootElement.TryGetProperty("content", out var contentElement) && contentElement.GetArrayLength() > 0)
                {
                    var text = contentElement[0].GetProperty("text").GetString();

                    // LOG: Başarılı (Info)
                    _logger.LogInformation("Claude API'den başarılı yanıt alındı ve kullanıcıya dönüldü.");

                    return Json(new { answer = text });
                }

                // LOG: API boş içerik döndü (Warning)
                _logger.LogWarning("Claude API yanıtı başarılı (200) fakat içerik boş veya format farklı. Ham yanıt: {Response}", responseString);
                return StatusCode(500, new { answer = "API'den boş yanıt döndü." });
            }
            catch (Exception ex)
            {
                // LOG: Beklenmeyen bir exception (Error) - Exception detayını da basar
                _logger.LogError(ex, "AskClaude metodunda beklenmeyen sunucu hatası.");
                return StatusCode(500, new { answer = $"Sunucu Hatası: {ex.Message}" });
            }
        }
    }
}