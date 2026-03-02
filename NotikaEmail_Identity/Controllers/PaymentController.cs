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
                                   ILogger<DefaultController> _logger // Not: Yapıyı bozmamak için DefaultController bıraktım, ama ideali ILogger<PaymentController> olmasıdır.
                                   ) : Controller
    {


        private readonly IyzicoSettings _settings = options.Value;

        public async Task<IActionResult> Index()
        {

            return View();
        }

        [HttpPost] // Güvenlik ve routing için ekledik
        public async Task<IActionResult> Payment([FromForm] CardModel cardModel)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user is null)
            {
                _logger.LogWarning("Ödeme işlemi başlatılamadı: Kullanıcı bulunamadı.");
                return RedirectToAction("SignIn", "Login");
            }

            // Response için kullanılacak model
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
                new OrderLine
                {
                    Name = "Preminum Üyelik",
                    Price = 500m,
                    Quantity = 1
                },
                new OrderLine
                {
                    Name = "Vergi",
                    Price = 750m,
                    Quantity = 1
                }
            }
            };

            var binNumber = cardModel.CardNumber!.Replace(" ", "").Substring(0, 8);
            var installmentResponse = await GetInstallmentsAsync(binNumber, order.Price);

            if (cardModel.Installment > 1)
            {
                var selectedInstallment = installmentResponse.InstallmentDetails
                    .SelectMany(d => d.InstallmentPrices)
                    .FirstOrDefault(ip => ip.InstallmentNumber == cardModel.Installment);

                if (selectedInstallment != null)
                {
                    order.PaidPrice = selectedInstallment.TotalPrice;
                    _logger.LogInformation("Taksit seçimi uygulandı. Kullanıcı: {Email}, OrderNo: {OrderNo}, Taksit: {Installment}, Yeni Tutar: {PaidPrice}", user.Email, order.OrderNo, cardModel.Installment, order.PaidPrice);
                }
            }

            // Ödeme isteği oluşturma
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
                CallbackUrl = "https://localhost:7052/Payment/Callback",
                PaymentCard = new PaymentCard
                {
                    CardHolderName = user.Name + " " + user.Surname,
                    CardNumber = cardModel.CardNumber!.Replace(" ", ""),
                    ExpireYear = "30",
                    ExpireMonth = "12",
                    Cvc = "123"
                },
                Buyer = new Buyer
                {
                    Id = "BY789",
                    Name = user.Name,
                    Surname = user.Surname,
                    IdentityNumber = "11111111111",
                    Email = user.Email, // Email'i de User modelinden çekmek daha sağlıklı
                    GsmNumber = "+905551234567",
                    RegistrationDate = "2013-04-21 15:12:09",
                    LastLoginDate = "2015-10-05 12:43:35",
                    RegistrationAddress = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                    City = "İstanbul",
                    Country = "Turkey",
                    ZipCode = "34732",
                    Ip = "176.88.36.38"
                },
                ShippingAddress = new ShippingAddress
                {
                    Address = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                    ZipCode = "34732",
                    ContactName = "Jane Doe",
                    City = "İstanbul",
                    Country = "Turkey"
                },
                BillingAddress = new BillingAddress
                {
                    Address = "Nidakule Göztepe, Merdivenköy Mah. Bora Sok. No:1",
                    ZipCode = "34732",
                    ContactName = "Jane Doe",
                    City = "İstanbul",
                    Country = "Turkey"
                },
                BasketItems = new List<BasketItem>()
            };

            // Basket item'ların eklenmesi
            foreach (var orderLine in order.OrderLines)
            {
                BasketItem basketItem = new()
                {
                    Id = orderLine.OrderLineId.ToString(),
                    Price = orderLine.Price,
                    Name = orderLine.Name ?? "Ürün",
                    Category1 = "Collectibles",
                    Category2 = "Accessories",
                    ItemType = "PHYSICAL"
                };
                paymentRequest.BasketItems.Add(basketItem);
            }

            // Client oluşturma
            using var client = new HttpClient();

            // İstek gövdesinin JSON formatına dönüştürülmesi
            JsonSerializerOptions jsonOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Data'nın JSON formatına dönüştürülmesi
            var json = JsonSerializer.Serialize(paymentRequest, jsonOptions);

            // Authorization token oluşturma
            var authToken = IyzicoAuthHelper.GenerateAuthToke(
                _settings.ApiKey!,
                _settings.SecretKey!,
                "/payment/3dsecure/initialize",
                json
            );

            // Token'ın header'a eklenmesi
            client.DefaultRequestHeaders.Add("Authorization", authToken);

            // İstek içeriğinin oluşturulması
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var postUrl = _settings.BaseUrl + "/payment/3dsecure/initialize";

            _logger.LogInformation("Iyzico 3D Secure Initialize isteği gönderiliyor. Kullanıcı: {Email}, OrderNo: {OrderNo}", user.Email, order.OrderNo);

            var response = await client.PostAsync(postUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Response'un deserialize edilmesi
            var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseString);

            // Response kontrolü
            if (paymentResponse == null)
            {
                _logger.LogError("Iyzico 3D Secure Initialize isteği null yanıt döndü. Kullanıcı: {Email}, OrderNo: {OrderNo}", user.Email, order.OrderNo);
                pageResponse.Success = false;
                pageResponse.Message = "Ödeme işlemi sırasında bir hata oluştu.";
                return Json(pageResponse);
            }

            // Response durumuna göre işlem yapılması
            if (paymentResponse.Status == "success")
            {
                _logger.LogInformation("Iyzico 3D Secure Initialize başarılı. Kullanıcı: {Email}, OrderNo: {OrderNo}", user.Email, order.OrderNo);

                // Order'ın kayıt edilmesi
                order.Status = true;
                // İleride Callback'te kullanıcıyı bulabilmek için siparişe User ID veya Email eklemen iyi olabilir. (Eğer Order modelinde Email/UserId property'si varsa ekle)
                // order.UserEmail = user.Email; 

                var result = await _orderRepository.CreateAsync(order);
                if (!result)
                {
                    _logger.LogError("Sipariş veritabanına kaydedilemedi! Kullanıcı: {Email}, OrderNo: {OrderNo}", user.Email, order.OrderNo);
                    pageResponse.Success = false;
                    pageResponse.Message = "Sipariş oluşturulamadı.";
                    return Json(pageResponse);
                }

                _logger.LogInformation("Sipariş veritabanına başarıyla kaydedildi. Kullanıcı: {Email}, OrderNo: {OrderNo}", user.Email, order.OrderNo);
                pageResponse.Success = true;

                string htmlContent = string.Empty;

                // HtmlContent base64 encoded olarak geliyor. Decode etmek gerekiyor.
                if (!string.IsNullOrEmpty(paymentResponse.ThreeDSHtmlContent))
                {
                    byte[] data = Convert.FromBase64String(paymentResponse.ThreeDSHtmlContent);
                    htmlContent = Encoding.UTF8.GetString(data);
                }

                pageResponse.HtmlContent = htmlContent;
            }
            else
            {
                _logger.LogWarning("Iyzico 3D Secure Initialize başarısız. Kullanıcı: {Email}, Hata: {ErrorMessage} Kodu: {ErrorCode} OrderNo: {OrderNo}", user.Email, paymentResponse.ErrorMessage, paymentResponse.ErrorCode, order.OrderNo);

                pageResponse.Success = false;
                pageResponse.Message = paymentResponse.ErrorMessage;
                pageResponse.ErrorCode = paymentResponse.ErrorCode;

                // Order'ın kayıt edilmesi
                order.Status = false;
                order.Message = $"{paymentResponse.ErrorCode} - {paymentResponse.ErrorMessage}";
                var result = await _orderRepository.CreateAsync(order);
                if (!result)
                {
                    _logger.LogError("Başarısız sipariş denemesi veritabanına kaydedilemedi! Kullanıcı: {Email}, OrderNo: {OrderNo}", user.Email, order.OrderNo);
                    pageResponse.Success = false;
                    pageResponse.Message = "Sipariş oluşturulamadı.";
                }
            }

            return Json(pageResponse);
        }


        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> Callback()
        {
            // Iyzico'dan gelen formun yakalanması
            var form = await Request.ReadFormAsync();

            var status = form["status"].ToString(); // success veya failure
            var conversationId = form["conversationId"].ToString(); // Başlatma sorgusunda gönderilen conversationId değeridir. Yani buna OrderNo gönderiliyor.
            var paymentId = form["paymentId"].ToString(); // Iyzico'nun ödemeye verdiği id değeri. status failure ise gelmez bu. 3D bitirme sorgusunda kullanılır.
            var errorMessage = form["errorMessage"].ToString();

            _logger.LogInformation("Iyzico Callback tetiklendi. ConversationId: {ConversationId}, Status: {Status}", conversationId, status);

            if (status == "false" || status == "failure")
            {
                _logger.LogWarning("Iyzico Callback başarısız döndü. ConversationId: {ConversationId}, Hata: {ErrorMessage}", conversationId, errorMessage);
                return RedirectToAction("Error", "Payment", new { message = errorMessage ?? "Ödeme işlemi sırasında bir hata oluştu." });
            }

            // Sipariş bulma
            var order = await _orderRepository.GetByOrderNoAsync(conversationId);
            if (order == null)
            {
                _logger.LogError("Iyzico Callback: Sipariş veritabanında bulunamadı! ConversationId: {ConversationId}", conversationId);
                return RedirectToAction("Error", "Payment", new { message = "Sipariş bulunamadı." });
            }

            // Kullanıcı mailini loglamak için User'ı Callback içinde bulmaya çalışıyoruz.
            // Not: [AllowAnonymous] olduğu için HttpContext.User boş olabilir. 
            // Eğer Order tablosunda UserEmail veya UserId tutuyorsan, oradan çekmek daha garanti olur.
            // Örnek: var userEmail = order.UserEmail ?? "Bilinmiyor";
            var userEmail = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Anonim/Callback";

            // 3D doğrulama bitirme isteği oluşturma
            PaymentCompletionRequest paymentCompletionRequest = new()
            {
                Locale = "tr",
                ConversationId = conversationId,
                PaymentId = paymentId,
                PaidPrice = order.PaidPrice.ToString("0.00", CultureInfo.InvariantCulture),
                BasketId = conversationId,
                Currency = "TRY"
            };

            // Client oluşturma
            using var client = new HttpClient();

            // İstek gövdesinin JSON formatına dönüştürülmesi
            JsonSerializerOptions jsonOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Data'nın JSON formatına dönüştürülmesi
            var json = JsonSerializer.Serialize(paymentCompletionRequest, jsonOptions);

            // Authorization token oluşturma
            var authToken = IyzicoAuthHelper.GenerateAuthToke(
                _settings.ApiKey!,
                _settings.SecretKey!,
                "/payment/v2/3dsecure/auth",
                json
            );

            // Token'ın header'a eklenmesi
            client.DefaultRequestHeaders.Add("Authorization", authToken);

            // İstek içeriğinin oluşturulması
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var postUrl = _settings.BaseUrl + "/payment/v2/3dsecure/auth";

            _logger.LogInformation("Iyzico 3D Tamamlama (Auth) isteği gönderiliyor. Kullanıcı: {Email}, ConversationId: {ConversationId}, PaymentId: {PaymentId}", userEmail, conversationId, paymentId);

            var response = await client.PostAsync(postUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            // Response'un deserialize edilmesi
            var paymentCompletionResponse = JsonSerializer.Deserialize<PaymentCompletionResponse>(responseString);

            // Response kontrolü
            if (paymentCompletionResponse == null)
            {
                _logger.LogError("Iyzico 3D Tamamlama yanıtı null döndü. Kullanıcı: {Email}, ConversationId: {ConversationId}", userEmail, conversationId);
                return RedirectToAction("Error", "Payment", new { message = "Ödeme işlemi sırasında bir hata oluştu." });
            }

            bool updateOrder = false;

            // Response durumuna göre işlem yapılması
            if (paymentCompletionResponse.Status == "success")
            {
                _logger.LogInformation("Ödeme başarıyla tamamlandı. Kullanıcı: {Email}, ConversationId: {ConversationId}, PaymentId: {PaymentId}", userEmail, conversationId, paymentId);

                order.Status = true;
                order.Message = "Ödeme başarılı.";

                updateOrder = await _orderRepository.UpdateAsync(order);
                if (updateOrder)
                {
                    return RedirectToAction("Success", "Payment", new { message = $"{order.OrderNo} numaralı siparişiniz oluşturulmuştur." });
                }
                else
                {
                    _logger.LogError("Ödeme başarılı oldu ancak sipariş güncellenirken veritabanı hatası oluştu! Kullanıcı: {Email}, OrderNo: {OrderNo}", userEmail, order.OrderNo);
                }
            }

            _logger.LogWarning("Ödeme tamamlama işlemi Iyzico tarafında reddedildi. Kullanıcı: {Email}, Hata Kodu: {ErrorCode}, Hata Mesajı: {ErrorMessage}, ConversationId: {ConversationId}", userEmail, paymentCompletionResponse.ErrorCode, paymentCompletionResponse.ErrorMessage, conversationId);

            order.Status = false;
            order.Message = $"Ödeme sırasında bir sorun oluştu. Hata kodu: {paymentCompletionResponse.ErrorCode}, Hata mesajı: {paymentCompletionResponse.ErrorMessage}";

            updateOrder = await _orderRepository.UpdateAsync(order);
            if (updateOrder)
                return RedirectToAction("Error", "Payment", new { message = order.Message });

            _logger.LogError("Başarısız sipariş güncellemesi veritabanına kaydedilemedi! Kullanıcı: {Email}, OrderNo: {OrderNo}", userEmail, order.OrderNo);
            return RedirectToAction("Error", "Payment", new { message = "Sipariş işlemi sırasında bir hata oluştu." });
        }



        public IActionResult Error(string message)
        {
            ResultModel model = new() { Message = message };
            return View(model);
        }

        public IActionResult Success(string message)
        {
            ResultModel model = new() { Message = message };
            return View(model);
        }

        public async Task<IActionResult> GetInstallments(string binNumber, decimal price)
        {
            var installmentResponse = await GetInstallmentsAsync(binNumber, price);
            return Json(installmentResponse);
        }

        private async Task<InstallmentResponse> GetInstallmentsAsync(string binNumber, decimal price)
        {
            // 3D doğrulama bitirme isteği oluşturma
            InstallmentRequest installmentRequest = new()
            {
                Locale = "tr",
                Price = price,
                BinNumber = binNumber
            };

            // Client oluşturma
            using var client = new HttpClient();

            // İstek gövdesinin JSON formatına dönüştürülmesi
            JsonSerializerOptions jsonOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Data'nın JSON formatına dönüştürülmesi
            var json = JsonSerializer.Serialize(installmentRequest, jsonOptions);

            // Authorization token oluşturma
            var authToken = IyzicoAuthHelper.GenerateAuthToke(
                _settings.ApiKey!,
                _settings.SecretKey!,
                "/payment/iyzipos/installment",
                json
            );

            // Token'ın header'a eklenmesi
            client.DefaultRequestHeaders.Add("Authorization", authToken);

            // İstek içeriğinin oluşturulması
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var postUrl = _settings.BaseUrl + "/payment/iyzipos/installment";

            _logger.LogInformation("Iyzico Taksit seçenekleri sorgulanıyor. BinNumber: {BinNumber}", binNumber);

            var response = await client.PostAsync(postUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            var result = new InstallmentResponse
            {
                Status = root.GetProperty("status").GetString(),
            };

            if (result.Status == "success")
            {
                _logger.LogInformation("Taksit seçenekleri başarıyla getirildi. BinNumber: {BinNumber}", binNumber);
                result.Success = true;

                var detailsArray = root.GetProperty("installmentDetails");

                foreach (var detail in detailsArray.EnumerateArray())
                {
                    var detailModel = new InstallmentDetailModel();

                    var pricesArray = detail.GetProperty("installmentPrices");
                    foreach (var item in pricesArray.EnumerateArray())
                    {
                        detailModel.InstallmentPrices.Add(new InstallmentPriceModel
                        {
                            InstallmentPrice = item.GetProperty("installmentPrice").GetDecimal(),
                            TotalPrice = item.GetProperty("totalPrice").GetDecimal(),
                            InstallmentNumber = item.GetProperty("installmentNumber").GetInt32()
                        });
                    }

                    result.InstallmentDetails.Add(detailModel);
                }
                return result;
            }

            result.Success = false;
            result.ErrorCode = root.GetProperty("errorCode").GetString();
            result.ErrorMessage = root.GetProperty("errorMessage").GetString();

            _logger.LogWarning("Taksit seçenekleri getirilemedi. Hata Kodu: {ErrorCode}, Hata: {ErrorMessage}, BinNumber: {BinNumber}", result.ErrorCode, result.ErrorMessage, binNumber);

            return result;
        }




        public IActionResult PlanSummary()
        {
            return View();
        }







    }
}