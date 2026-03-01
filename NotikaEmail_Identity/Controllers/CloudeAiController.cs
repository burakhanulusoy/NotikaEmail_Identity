using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaEmail_Identity.Entities;
using NotikaEmail_Identity.Services.MessageServices;
using System.Text;
using System.Text.Json;

namespace NotikaEmail_Identity.Controllers
    {

    [Authorize]
        public class CloudeAiController(IConfiguration _configuration
                                       , IHttpClientFactory _httpClientFactory
                                       ,UserManager<AppUser> _userManager
                                       ,IMessageService  _messageService ) : Controller
        {
            public async Task<IActionResult> Index()
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                ViewBag.name = user.Name + " " + user.Surname;
                ViewBag.email = user.Email;
                ViewBag.imageUrl = user.ImageUrl;
                ViewBag.createdDate = user.CreatedDate;

                ViewBag.lastDateSend = (await _messageService.GetMessageUserSendDateAsync(user.Id)).SendDate.ToShortDateString();
            


                return View();
            }
        [HttpPost]
        public async Task<IActionResult> AskClaude([FromBody] JsonElement data)
        {
            try
            {
                string prompt = data.GetProperty("prompt").GetString();
                string apiKey = _configuration["MyCloudeApiKey:Key"];

                if (string.IsNullOrEmpty(prompt)) return BadRequest("Soru boş olamaz.");
                if (string.IsNullOrEmpty(apiKey)) return StatusCode(500, "API Anahtarı bulunamadı! Appsettings.json kontrol edilmeli.");

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api.anthropic.com");
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                // 🌟 YENİ, DETAYLI VE İNGİLİZCE SYSTEM PROMPT
                string systemInstruction = @"You are the official customer support AI assistant for an advanced email application called 'Notika'. 
Your STRICT DUTY is to assist users ONLY with issues related to the Notika application (e.g., cannot send emails, password reset, inbox synchronization, connection issues, account settings). 
Provide step-by-step, polite, and highly professional guidance. 
If a user asks a question completely unrelated to Notika or email clients (e.g., cars, politics, software programming, recipes), you MUST NOT answer the question. Instead, reply strictly with: 'I am the Notika Customer Support assistant. I am only trained to answer questions related to the Notika email application. How can I help you with your app today?'. 
CRITICAL RULE: You must automatically detect the language the user is speaking, and reply in that EXACT SAME language. If the user types in Turkish, reply perfectly in Turkish. If they type in English, reply in English.";

                var requestBody = new
                {
                    model = "claude-sonnet-4-20250514", // Modelini aynen bıraktım
                    max_tokens = 1024,
                    system = systemInstruction,
                    messages = new[] { new { role = "user", content = prompt } }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("v1/messages", jsonContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return BadRequest(new { answer = $"Claude API Hatası: {responseString}" });

                using var doc = JsonDocument.Parse(responseString);
                var text = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString();

                return Json(new { answer = text });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { answer = $"Sunucu Hatası: {ex.Message}" });
            }
        }
    }
    }
