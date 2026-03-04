<div align="center">

# 🚀 Notika
### Yapay Zeka Destekli, Abonelik Bazlı SaaS İletişim Platformu

![ASP.NET Core 9.0](https://img.shields.io/badge/ASP.NET_Core_9.0-5C2D91?style=for-the-badge&logo=dotnet)
![Payment](https://img.shields.io/badge/FinTech-Iyzico_Payment-0065FF?style=for-the-badge&logo=contactlesspayment)
![Logging](https://img.shields.io/badge/Observability-SEQ_Logging-00C853?style=for-the-badge&logo=datalore)
![Auth](https://img.shields.io/badge/Auth-Google_%26_Facebook-4285F4?style=for-the-badge&logo=google)
![AI](https://img.shields.io/badge/AI-Anthropic_%26_ML.NET-D96634?style=for-the-badge&logo=anthropic)

<br>

<p align="center">
  <strong>Güvenli Ödeme, Derinlemesine Analiz ve Akıllı İletişim.</strong>
</p>
<p align="center">
  <i>Iyzico ile entegre ödeme geçidi, Seq ile yapılandırılmış kullanıcı takibi, Google/Facebook OAuth mimarisi<br>ve ML.NET tabanlı güvenlik kalkanı ile donatılmış uçtan uca ticari SaaS projesi.</i>
</p>

</div>

---

**Notika**, modern yazılım dünyasının en kritik üç bileşenini (FinTech, Observability, Security) tek bir potada eriten vizyoner bir platformdur. Kullanıcıların **Iyzico** güvencesiyle premium paket satın alabildiği, tüm sistemsel hareketlerin **Seq** sunucusunda adım adım izlendiği ve giriş işlemlerinin **OAuth 2.0** (Google/Facebook) ile saniyeler içinde tamamlandığı bütüncül bir yönetim ekosistemidir.

Bu proje, sadece teknik bir çözüm değil; **abonelik ekonomisine (Subscription Economy)** uygun, ölçeklenebilir ve canlıya alınmaya hazır bir iş modelidir.

---

## 🌟 Projenin 3 Ana Güç Sütunu

Notika, sıradan bir mesajlaşma uygulamasından, ticari bir ürüne şu üç temel özellikle dönüşür:

### 💳 1. Iyzico ile Uçtan Uca Ödeme (FinTech)
Proje, gerçek dünya e-ticaret standartlarına uygun bir ödeme altyapısına sahiptir.
* **Paket Mimarisi:** Sistemde "Basic", "Gold" ve "Platinum" olmak üzere dinamik paketler bulunur.
* **Güvenli Ödeme Ağı:** Kullanıcı kart bilgilerini girdiğinde, veri veritabanına kaydedilmeden doğrudan **Iyzico API**'ye şifreli olarak iletilir.
* **3D Secure & Taksit:** Iyzico'nun sunduğu güvenli ödeme formları ve taksit seçenekleri backend tarafında yönetilir.
* **Anlık Rol Yükseltme:** Ödeme "Success" döndüğü milisaniye içerisinde, arka planda çalışan servisler kullanıcının **Role** yetkisini `StandardUser`'dan `PremiumUser`'a yükseltir.

### 📊 2. Seq ile Merkezi Loglama ve Kullanıcı Yolculuğu (Observability)
Klasik metin dosyası loglamasının ötesinde, **Structured Logging (Yapılandırılmış Loglama)** mimarisi kullanılmıştır.
* **Canlı İzleme:** Sistemdeki her hata, uyarı veya bilgi mesajı anlık olarak Seq sunucusuna (Dashboard) düşer.
* **User Journey (Kullanıcı Yolculuğu):** Bir hata oluştuğunda, sadece hatayı değil; o kullanıcının sisteme girdiği andan hatayı aldığı ana kadar hangi sayfalara tıkladığı, ödeme adımında ne kadar beklediği gibi tüm akış `CorrelationId` ile takip edilebilir.
* **Sorgulanabilir Loglar:** Adminler, "Son 1 saatte ödeme hatası alan kullanıcılar kim?" gibi soruları SQL benzeri sorgularla Seq üzerinden cevaplayabilir.

### 🔐 3. Google & Facebook ile OAuth 2.0 Entegrasyonu
Kullanıcı deneyimini (UX) maksimize eden modern kimlik doğrulama süreçleri.
* **Tek Tıkla Giriş:** Kullanıcılar kayıt formlarıyla uğraşmadan, Google veya Facebook hesaplarıyla sisteme dahil olabilir.
* **Claims Mapping:** Sosyal medyadan gelen veriler (Email, İsim, Profil Resmi), ASP.NET Core Identity yapısındaki `Claims` mekanizmasına otomatik olarak eşlenir.
* **Güvenli Token Değişimi:** OAuth protokolü sayesinde kullanıcı şifreleri asla Notika sunucularında tutulmaz, sadece yetkilendirme token'ları (Access Token) işlenir.

### 🧠 4. ML.NET ile Otonom Spam Kalkanı
Yapay zeka, bu projede sadece bir özellik değil; sistemin güvenlik duvarıdır.
* **Binary Classification:** Sistem, Microsoft'un ML.NET kütüphanesini kullanarak mesajları arka planda "Spam" veya "Ham" (Temiz) olarak sınıflandırır.
* **Gerçek Zamanlı Analiz (Real-Time Inference):** Mesaj gönderildiği milisaniye içinde model tarafından puanlanır. %85 ve üzeri spam skoru alan içerikler, alıcının gelen kutusunu kirletmeden doğrudan karantinaya alınır.
* **Eğitilebilir Model:** Sistem, yeni spam türlerine ve kelime örüntülerine karşı sürekli güncellenebilir bir `.zip` model mimarisine sahiptir.

### 🤖 5. Anthropic Claude ile Üretken Yapay Zeka (GenAI)
Sistem, statik bir destek sayfasından öte, bağlamı anlayan akıllı bir asistana sahiptir.
* **Akıllı Teknik Destek:** Kullanıcılar site içi sorunlarını (Ödeme, Giriş vb.) SSS sayfalarında aramak yerine, doğal dilde yapay zeka asistanına sorarak anında çözüm bulabilirler.
* **İçerik Mühendisliği:** Kullanıcılar, "Müşteriye nazik bir ret maili yaz" veya "Proje teslimi için teşekkür mesajı oluştur" gibi komutlarla saniyeler içinde profesyonel ve kurumsal dilde e-posta taslakları üretebilir.
* **Context Awareness:** Asistan, kullanıcının önceki konuşmalarını veya o anki sayfa bağlamını analiz ederek en doğru yanıtı verir.

### 📂 6. Gelişmiş Dosya ve Raporlama Yönetimi
Sadece metin tabanlı değil, belge ve multimedya destekli zengin bir iletişim altyapısı.
* **Dinamik PDF Dönüştürücü:** Kullanıcılar, önemli mesajlaşma geçmişlerini veya sistem raporlarını, HTML'den PDF'e (Rotativa/Wkhtmltopdf motoru ile) kayıpsız bir şekilde dönüştürüp indirebilir.
* **Güvenli Ek (Attachment) Sistemi:** Mail gönderimlerinde PDF, JPEG veya PNG dosyaları, sunucu tarafında güvenlik taramasından geçirilerek şifreli dosya yollarında saklanır.
* **Görsel İletişim:** Kullanıcılar, sorunlarını veya taleplerini anlatırken metinlere görsel kanıtlar ekleyerek süreci hızlandırabilir.

### 🛡️ 7. Üst Düzey Hesap Güvenliği ve Identity
Kullanıcı verilerini koruyan "Sıfır Güven" (Zero Trust) yaklaşımı ve ASP.NET Core Identity entegrasyonu.
* **Zaman Ayarlı (Time-Sensitive) Doğrulama:** Şifremi unuttum veya hesap silme gibi kritik işlemlerde, e-postaya gönderilen doğrulama kodları/linkleri sadece kısıtlı bir süre (Örn: 3 dakika) geçerlidir.
* **Token Tabanlı İşlem:** Linkler ve kodlar, veritabanında açık halde tutulmaz; Identity kütüphanesinin güvenli token (GeneratePasswordResetToken) algoritmalarıyla şifrelenir.
* **Kademeli Onay Mekanizması:** Kullanıcı sisteme giriş yapmış olsa bile, hesabını silmek gibi geri dönülemez bir işlem yapmak istediğinde sistem tekrar kimlik doğrulaması (Re-Authentication) talep eder.

---


## 🏗️ Mimari ve Tasarım Prensipleri

Proje, spagetti kod yapısından uzak, **SOLID** prensiplerine sıkı sıkıya bağlı ve **Clean Architecture** (Temiz Mimari) felsefesiyle geliştirilmiştir. Amaç; sürdürülebilir, test edilebilir ve gevşek bağımlılığa (Loose Coupling) sahip bir kurumsal yapı kurmaktır.

| Katman / Konsept | Uygulama Detayı ve Teknolojiler |
| :--- | :--- |
| **Veri Erişim (DAL)** | **Entity Framework Core 9.0 (Code First)** |
| **Tasarım Deseni** | **Generic Repository & Unit of Work Pattern** |
| **İş Mantığı (BL)** | **Service Layer (Business Managers)** |
| **Nesne Eşleme** | **AutoMapper (Entity <-> DTO Separation)** |
| **Validasyon** | **FluentValidation (Server-Side Rules)** |
| **Asenkron Yapı** | **Tamamen `async/await` tabanlı Non-Blocking I/O** |
| **Bağımlılık Yönetimi** | **Dependency Injection (Built-in IoC Container)** |
| **Ödeme Altyapısı** | **Iyzico API Entegrasyonu (SaaS Modeli)** |
| **Gözlemlenebilirlik** | **Serilog & Seq (Structured Logging)** |
| **Yapay Zeka** | **ML.NET (On-Premise) & Anthropic Claude (Cloud API)** |

---

### 🏛️ Mimari Derinlik ve Karar Mekanizmaları

Projede alınan her teknik karar, belirli bir problemi çözmek üzerine kurgulanmıştır:

#### 1. Generic Repository ve Unit of Work Deseni
Veritabanı işlemlerini tekerrürden kurtarmak (DRY - Don't Repeat Yourself) için **Generic Repository** kullanılmıştır.
* **Neden?** Her entity (User, Message, Payment) için ayrı ayrı CRUD metotları yazmak yerine, tek bir jenerik yapı üzerinden tüm veritabanı işlemleri yönetilir.
* **Unit of Work:** Yapılan işlemler (örneğin; Ödeme alma -> Rol güncelleme -> Log atma) zincirleme bir bütündür. `SaveChanges()` tek bir noktadan çağrılarak **Transaction Integrity (Veri Tutarlılığı)** garanti altına alınır. İşlem sırasında bir hata olursa, tüm adımlar geri alınır (Rollback).

#### 2. Service Layer ve Thin Controller
Controller sınıfları sadece gelen isteği (Request) karşılar ve cevabı (Response) döner.
* **İş Mantığı İzolasyonu:** "Kullanıcı premium mu?", "Mesaj spam mi?", "Kredi kartı limiti yetiyor mu?" gibi tüm mantıksal sorgulamalar **Service Katmanı**'nda (Manager sınıfları) yapılır. Bu sayede kodun okunabilirliği artar ve tekrar kullanımı kolaylaşır.

#### 3. DTO (Data Transfer Object) ve AutoMapper
Veritabanı nesneleri (Entity) asla doğrudan API veya View tarafına gönderilmez.
* **Güvenlik:** Kullanıcının şifre hash'i, salt değeri veya internal ID'leri dış dünyaya kapalıdır.
* **Performans:** `AutoMapper` kütüphanesi ile Entity'ler, sadece ihtiyaç duyulan veriyi taşıyan hafif DTO nesnelerine otomatik olarak dönüştürülür.

#### 4. Aspect-Oriented Concepts (Validation & Logging)
Kodun içerisine `if-else` bloklarıyla validasyon yazmak yerine **FluentValidation** kullanılmıştır.
* **Merkezi Validasyon:** "Email boş olamaz", "Şifre en az 6 karakter olmalı" gibi kurallar, Entity sınıflarından ayrı bir validasyon katmanında yönetilir.
* **Seq Loglama:** Hata yönetimi `try-catch` bloklarına sıkıştırılmaz; global hata yakalayıcılar (Middlewares) ve Seq entegrasyonu ile tüm sistem akışı merkezi bir paneldan izlenir.

#### 5. ML.NET ve AI Servis Entegrasyonu
Yapay zeka modülleri, ana akışı bozmayacak şekilde servis olarak enjekte edilmiştir (Dependency Injection).
* **Soyutlama:** `ISpamDetectionService` veya `IContentGeneratorService` gibi arayüzler sayesinde, ileride farklı bir AI modeline geçilmek istenirse ana kodda değişiklik yapmaya gerek kalmaz.



















## 💻 Kritik Kod Kesitleri (Deep Dive)

Projenin kalbini oluşturan üç ana modülün backend kod yapıları:

### 1. 💳 Iyzico Ödeme Servisi (Service Layer)
Kullanıcının sepet tutarını ve kart bilgilerini Iyzico API'sine ileten ve dönen sonuca göre yetki yükselten servis.

```csharp
public async Task<PaymentResult> ProcessPaymentAsync(PaymentDto model, string userId)
{
    // 1. Iyzico İstek Konfigürasyonu
    CreatePaymentRequest request = new CreatePaymentRequest();
    request.Locale = Locale.TR.ToString();
    request.ConversationId = Guid.NewGuid().ToString();
    request.Price = model.Price.ToString();
    request.PaidPrice = model.Price.ToString(); // İndirim varsa burası değişir
    request.Currency = Currency.TRY.ToString();
    request.PaymentGroup = PaymentGroup.PRODUCT.ToString();

    // 2. Kart ve Alıcı Bilgileri (Mapper ile)
    request.PaymentCard = _mapper.Map<PaymentCard>(model.CardInfo);
    request.Buyer = await _userService.GetBuyerInfoAsync(userId);

    // 3. Iyzico API Çağrısı
    Payment payment = Payment.Create(request, _iyzicoOptions);

    // 4. Sonuç Yönetimi & Rol Yükseltme
    if (payment.Status == "success")
    {
        // Ödeme Başarılı: Kullanıcıyı Premium Yap
        await _roleManager.AddToRoleAsync(userId, "PremiumUser");
        
        // Loglama (Seq'e gider)
        Log.Information("Kullanıcı {UserId}, {Amount} TL tutarında paket satın aldı. TransactionId: {TransId}", 
            userId, model.Price, payment.PaymentId);
            
        return PaymentResult.Success(payment.PaymentId);
    }

    // Hata Durumu
    Log.Error("Ödeme Başarısız. Kullanıcı: {UserId}, Hata: {Error}", userId, payment.ErrorMessage);
    return PaymentResult.Fail(payment.ErrorMessage);
}
