using Core_IyzicoPaymentSystem.Entities;
using Core_IyzicoPaymentSystem.Methods;
using Core_IyzicoPaymentSystem.Models;
using Core_IyzicoPaymentSystem.Repositories.OrderRepositories;
using Iyzico.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NotikaEmail_Identity.Entities;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace NotikaEmail_Identity.Controllers
{
    [Authorize(Roles = "Admin, User")]
    public class PaymentController(IOptions<IyzicoSettings> options,
                                   IOrderRepository _orderRepository,
                                   UserManager<AppUser> _userManager,
                                   ILogger<PaymentController> _logger // Düzeltme: Generic tip PaymentController yapıldı.
                                   ) : Controller
    {
        private readonly IyzicoSettings _settings = options.Value;

        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Payment([FromForm] CardModel cardModel)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                _logger.LogWarning("⚠️ ÖDEME BAŞLATILAMADI: Kullanıcı oturumu bulunamadı. IP: {Ip}", ipAddress);
                return RedirectToAction("SignIn", "Login");
            }

            _logger.LogInformation("💳 ÖDEME SÜRECİ BAŞLADI: Kullanıcı: {Email} | IP: {Ip}", user.Email, ipAddress);

            PageResponse pageResponse = new();

            // Sipariş oluşturma
            Order order = new()
            {
                OrderNo = Guid.NewGuid().ToString(),
                Price = 1250m,
                PaidPrice = 1250m,
                CreatedDate = DateTime.Now,
                Status = false,
                OrderLines = new List<OrderLine>
                {
                    new OrderLine { Name = "Premium Üyelik", Price = 500m, Quantity = 1 },
                    new OrderLine { Name = "Vergi", Price = 750m, Quantity = 1 }
                }
            };

            // Taksit Hesaplama Mantığı
            var binNumber = cardModel.CardNumber!.Replace(" ", "").Substring(0, 6); // Genelde ilk 6 hane yeterlidir (Bin Number)

            // Loglarda kart numarasının tamamını asla saklamıyoruz!
            _logger.LogDebug("Taksit kontrol ediliyor. Bin: {Bin}****** | Taksit İsteği: {Installment}", binNumber, cardModel.Installment);

            var installmentResponse = await GetInstallmentsAsync(binNumber, order.Price);

            if (cardModel.Installment > 1)
            {
                var selectedInstallment = installmentResponse.InstallmentDetails
                    .SelectMany(d => d.InstallmentPrices)
                    .FirstOrDefault(ip => ip.InstallmentNumber == cardModel.Installment);

                if (selectedInstallment != null)
                {
                    order.PaidPrice = selectedInstallment.TotalPrice;
                    _logger.LogInformation("🔄 TAKSİT UYGULANDI: OrderNo: {OrderNo} | Taksit: {Installment} | Yeni Tutar: {PaidPrice}", order.OrderNo, cardModel.Installment, order.PaidPrice);
                }
            }

            // Payment Request Hazırlığı
            PaymentRequest paymentRequest = new()
            {
                Locale = "tr",
                ConversationId = order.OrderNo,
                Price = order.Price,
                PaidPrice = order.PaidPrice,
                Currency = "TRY",
                Installment = cardModel.Installment,
                PaymentChannel = "WEB",
                BasketId = order.OrderNo,
                PaymentGroup = "PRODUCT",
                CallbackUrl = "https://localhost:7052/Payment/Callback", // Canlıda burası domain olmalı
                PaymentCard = new PaymentCard
                {
                    CardHolderName = user.Name + " " + user.Surname,
                    CardNumber = cardModel.CardNumber!.Replace(" ", ""),
                    ExpireYear = "30", // Burayı dinamik almalısınız
                    ExpireMonth = "12", // Burayı dinamik almalısınız
                    Cvc = "123" // Burayı dinamik almalısınız
                },
                Buyer = new Buyer
                {
                    Id = user.Id.ToString(), // Dinamik User ID
                    Name = user.Name,
                    Surname = user.Surname,
                    IdentityNumber = "11111111111", // Gerekirse kullanıcıdan alınmalı
                    Email = user.Email,
                    GsmNumber = user.PhoneNumber ?? "+905555555555",
                    RegistrationDate = (user.CreatedDate ?? DateTime.Now).ToString("yyyy-MM-dd HH:mm:ss"),
                    LastLoginDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    RegistrationAddress = "Nidakule Göztepe...", // Kullanıcı adresinden gelmeli
                    City = user.City ?? "Istanbul",
                    Country = "Turkey",
                    ZipCode = "34732",
                    Ip = ipAddress
                },
                ShippingAddress = new ShippingAddress
                {
                    Address = "Teslimat Adresi...",
                    ZipCode = "34732",
                    ContactName = user.Name + " " + user.Surname,
                    City = "Istanbul",
                    Country = "Turkey"
                },
                BillingAddress = new BillingAddress
                {
                    Address = "Fatura Adresi...",
                    ZipCode = "34732",
                    ContactName = user.Name + " " + user.Surname,
                    City = "Istanbul",
                    Country = "Turkey"
                },
                BasketItems = new List<BasketItem>()
            };

            foreach (var orderLine in order.OrderLines)
            {
                paymentRequest.BasketItems.Add(new BasketItem
                {
                    Id = orderLine.OrderLineId.ToString(),
                    Price = orderLine.Price,
                    Name = orderLine.Name ?? "Ürün",
                    Category1 = "General",
                    ItemType = "PHYSICAL"
                });
            }

            // Iyzico İsteği Gönderme
            try
            {
                using var client = new HttpClient();
                JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(paymentRequest, jsonOptions);

                var authToken = IyzicoAuthHelper.GenerateAuthToke(_settings.ApiKey!, _settings.SecretKey!, "/payment/3dsecure/initialize", json);
                client.DefaultRequestHeaders.Add("Authorization", authToken);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("🚀 IYZICO İSTEĞİ: 3D Secure Initialize gönderiliyor. OrderNo: {OrderNo}", order.OrderNo);

                var response = await client.PostAsync(_settings.BaseUrl + "/payment/3dsecure/initialize", content);
                var responseString = await response.Content.ReadAsStringAsync();
                var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseString);

                if (paymentResponse == null)
                {
                    _logger.LogCritical("❌ IYZICO HATASI: Yanıt null döndü veya deserialize edilemedi. OrderNo: {OrderNo}", order.OrderNo);
                    pageResponse.Success = false;
                    pageResponse.Message = "Ödeme servisine ulaşılamadı.";
                    return Json(pageResponse);
                }

                if (paymentResponse.Status == "success")
                {
                    _logger.LogInformation("✅ 3D BAŞLATILDI: Iyzico başarılı yanıt verdi. HTML içeriği alınıyor. OrderNo: {OrderNo}", order.OrderNo);

                    order.Status = false; // Henüz ödeme bitmedi, sadece başladı
                    var dbResult = await _orderRepository.CreateAsync(order);

                    if (!dbResult)
                    {
                        _logger.LogError("🔥 DB HATASI: Sipariş Iyzico'ya gitti ama DB'ye yazılamadı! OrderNo: {OrderNo}", order.OrderNo);
                        pageResponse.Success = false;
                        pageResponse.Message = "Sipariş kayıt hatası.";
                        return Json(pageResponse);
                    }

                    pageResponse.Success = true;
                    if (!string.IsNullOrEmpty(paymentResponse.ThreeDSHtmlContent))
                    {
                        byte[] data = Convert.FromBase64String(paymentResponse.ThreeDSHtmlContent);
                        pageResponse.HtmlContent = Encoding.UTF8.GetString(data);
                    }
                }
                else
                {
                    // Iyzico Business Error (Yetersiz bakiye, geçersiz kart vb.)
                    _logger.LogWarning("⛔ ÖDEME REDDEDİLDİ: OrderNo: {OrderNo} | Hata Kodu: {Code} | Mesaj: {Msg}", order.OrderNo, paymentResponse.ErrorCode, paymentResponse.ErrorMessage);

                    pageResponse.Success = false;
                    pageResponse.Message = paymentResponse.ErrorMessage;
                    pageResponse.ErrorCode = paymentResponse.ErrorCode;

                    // Başarısız siparişi de loglamak iyidir
                    order.Status = false;
                    order.Message = $"{paymentResponse.ErrorCode} - {paymentResponse.ErrorMessage}";
                    await _orderRepository.CreateAsync(order);
                }

                return Json(pageResponse);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "🔥 SİSTEM HATASI: Ödeme isteği sırasında exception oluştu. OrderNo: {OrderNo}", order.OrderNo);
                return Json(new { Success = false, Message = "Sistem hatası oluştu." });
            }
        }

        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> Callback()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            try
            {
                var form = await Request.ReadFormAsync();
                var status = form["status"].ToString();
                var conversationId = form["conversationId"].ToString(); // OrderNo
                var paymentId = form["paymentId"].ToString();
                var errorMessage = form["errorMessage"].ToString();

                _logger.LogInformation("📞 CALLBACK GELDİ: OrderNo: {OrderNo} | Durum: {Status} | IP: {Ip}", conversationId, status, ipAddress);

                if (status == "false" || status == "failure")
                {
                    _logger.LogWarning("❌ CALLBACK BAŞARISIZ: OrderNo: {OrderNo} | Hata: {Error}", conversationId, errorMessage);

                    // Siparişi bulup güncellemek iyi olabilir
                    var failedOrder = await _orderRepository.GetByOrderNoAsync(conversationId);
                    if (failedOrder != null)
                    {
                        failedOrder.Message = "Callback Hatası: " + errorMessage;
                        await _orderRepository.UpdateAsync(failedOrder);
                    }

                    return RedirectToAction("Error", "Payment", new { message = errorMessage ?? "Ödeme işlemi başarısız." });
                }

                var order = await _orderRepository.GetByOrderNoAsync(conversationId);
                if (order == null)
                {
                    _logger.LogCritical("😱 KRİTİK VERİ HATASI: Callback geldi ama Sipariş DB'de yok! OrderNo: {OrderNo} | PaymentId: {PaymentId}", conversationId, paymentId);
                    return RedirectToAction("Error", "Payment", new { message = "Sipariş bulunamadı." });
                }

                // 3D Tamamlama İsteği
                PaymentCompletionRequest completionRequest = new()
                {
                    Locale = "tr",
                    ConversationId = conversationId,
                    PaymentId = paymentId,
                    PaidPrice = order.PaidPrice.ToString("0.00", CultureInfo.InvariantCulture),
                    BasketId = conversationId,
                    Currency = "TRY"
                };

                using var client = new HttpClient();
                JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(completionRequest, jsonOptions);
                var authToken = IyzicoAuthHelper.GenerateAuthToke(_settings.ApiKey!, _settings.SecretKey!, "/payment/v2/3dsecure/auth", json);

                client.DefaultRequestHeaders.Add("Authorization", authToken);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("🔄 3D DOĞRULAMA (AUTH) GÖNDERİLİYOR: OrderNo: {OrderNo} | PaymentId: {PaymentId}", conversationId, paymentId);

                var response = await client.PostAsync(_settings.BaseUrl + "/payment/v2/3dsecure/auth", content);
                var responseString = await response.Content.ReadAsStringAsync();
                var completionResponse = JsonSerializer.Deserialize<PaymentCompletionResponse>(responseString);

                if (completionResponse != null && completionResponse.Status == "success")
                {
                    _logger.LogInformation("✅ ÖDEME TAMAMLANDI: OrderNo: {OrderNo} başarıyla tahsil edildi.", conversationId);

                    order.Status = true;
                    order.Message = "Ödeme Başarılı";
                    // order.PaymentId = paymentId; // PaymentId'yi de kaydetmek çok önemlidir!

                    bool updateResult = await _orderRepository.UpdateAsync(order);
                    if (!updateResult)
                    {
                        _logger.LogError("🔥 DB UPDATE HATASI: Para çekildi ama sipariş durumu güncellenemedi! OrderNo: {OrderNo}", conversationId);
                        // Burada gerekirse bir "Acil Durum" maili attırılabilir.
                    }

                    return RedirectToAction("Success", "Payment", new { message = $"Siparişiniz onaylandı. No: {order.OrderNo}" });
                }
                else
                {
                    var errorMsg = completionResponse?.ErrorMessage ?? "Bilinmeyen hata";
                    var errorCode = completionResponse?.ErrorCode ?? "Unknown";

                    _logger.LogWarning("⛔ 3D DOĞRULAMA REDDEDİLDİ: OrderNo: {OrderNo} | Hata: {Msg} ({Code})", conversationId, errorMsg, errorCode);

                    order.Status = false;
                    order.Message = errorMsg;
                    await _orderRepository.UpdateAsync(order);

                    return RedirectToAction("Error", "Payment", new { message = errorMsg });
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "🔥 CALLBACK EXCEPTION: Callback işlenirken uygulama patladı!");
                return RedirectToAction("Error", "Payment", new { message = "İşlem sırasında teknik bir hata oluştu." });
            }
        }

        public IActionResult Error(string message)
        {
            return View(new ResultModel { Message = message });
        }

        public IActionResult Success(string message)
        {
            return View(new ResultModel { Message = message });
        }

        // Installment metodu aynı kalabilir, sadece log ekledik
        public async Task<IActionResult> GetInstallments(string binNumber, decimal price)
        {
            var response = await GetInstallmentsAsync(binNumber, price);
            return Json(response);
        }

        private async Task<InstallmentResponse> GetInstallmentsAsync(string binNumber, decimal price)
        {
            _logger.LogInformation("🔍 TAKSİT SORGUSU: Bin: {Bin} | Tutar: {Price}", binNumber, price);

            InstallmentRequest request = new() { Locale = "tr", Price = price, BinNumber = binNumber };

            using var client = new HttpClient();
            JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(request, jsonOptions);

            var authToken = IyzicoAuthHelper.GenerateAuthToke(_settings.ApiKey!, _settings.SecretKey!, "/payment/iyzipos/installment", json);
            client.DefaultRequestHeaders.Add("Authorization", authToken);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(_settings.BaseUrl + "/payment/iyzipos/installment", content);
                var responseString = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                var status = root.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : "failure";

                if (status == "success")
                {
                    _logger.LogInformation("✅ TAKSİT BİLGİSİ HAM VERİ ALINDI: Bin: {Bin}", binNumber);

                    var result = new InstallmentResponse
                    {
                        Status = "success",
                        Success = true,
                        InstallmentDetails = new List<InstallmentDetailModel>()
                    };

                    if (root.TryGetProperty("installmentDetails", out var detailsArray))
                    {
                        foreach (var detail in detailsArray.EnumerateArray())
                        {
                            // DÜZELTME: Hata veren BankCode ve BankName satırları kaldırıldı.
                            var detailModel = new InstallmentDetailModel
                            {
                                InstallmentPrices = new List<InstallmentPriceModel>()
                            };

                            if (detail.TryGetProperty("installmentPrices", out var pricesArray))
                            {
                                foreach (var item in pricesArray.EnumerateArray())
                                {
                                    detailModel.InstallmentPrices.Add(new InstallmentPriceModel
                                    {
                                        InstallmentPrice = item.GetProperty("installmentPrice").GetDecimal(),
                                        TotalPrice = item.GetProperty("totalPrice").GetDecimal(),
                                        InstallmentNumber = item.GetProperty("installmentNumber").GetInt32()
                                    });
                                }
                            }
                            result.InstallmentDetails.Add(detailModel);
                        }
                    }

                    return result;
                }
                else
                {
                    var errMsg = root.TryGetProperty("errorMessage", out var err) ? err.GetString() : "Bilinmeyen hata";
                    _logger.LogWarning("⚠️ TAKSİT SORGUSU BAŞARISIZ: Bin: {Bin} | Hata: {Error}", binNumber, errMsg);

                    return new InstallmentResponse { Status = "failure", Success = false, ErrorMessage = errMsg };
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "🔥 TAKSİT HATASI: Bin: {Bin}", binNumber);
                return new InstallmentResponse { Status = "failure", Success = false, ErrorMessage = "Sistem hatası." };
            }
        }





        public IActionResult PlanSummary()
        {
            return View();
        }
    }
}