<div align="center">

# 🚀 Notika
### Yapay Zeka Destekli, Abonelik Bazlı  Mail Gönderme Platformu

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
  <i>Iyzico ile entegre ödeme geçidi, Seq ile yapılandırılmış kullanıcı takibi, Google/Facebook OAuth mimarisi<br>ve ML.NET tabanlı güvenlik kalkanı ile donatılmış .Net Core MVC projesi.</i>
</p>

</div>

---

**Notika**, modern yazılım dünyasının en kritik üç bileşenini (FinTech, Observability, Security) tek bir potada eriten vizyoner bir platformdur. Kullanıcıların **Iyzico** güvencesiyle premium paket satın alabildiği, tüm sistemsel hareketlerin **Seq** sunucusunda adım adım izlendiği ve giriş işlemlerinin **OAuth 2.0** (Google/Facebook) ile saniyeler içinde tamamlandığı bütüncül bir yönetim ekosistemidir.

Bu proje, sadece teknik bir çözüm değil; **abonelik ekonomisine (Subscription Economy)** uygun, ölçeklenebilir ve canlıya alınmaya hazır bir platform modelidir.

---

## 🌟 Projenin 7 Ana Güç Sütunu

Notika, sıradan bir mesajlaşma uygulamasından, ticari bir ürüne şu yedi temel özellikle dönüşür:

### 💳 1. Iyzico ile Uçtan Uca Ödeme (FinTech)
Proje, gerçek dünya e-ticaret standartlarına uygun bir ödeme altyapısına sahiptir.
* **Paket Mimarisi:** Sistemde "Basic" ,"Platinum" olmak üzere dinamik paketler bulunur.
* **Güvenli Ödeme Ağı:** Kullanıcı kart bilgilerini girdiğinde, veri veritabanına kaydedilmeden doğrudan **Iyzico API**'ye şifreli olarak iletilir.
* **3D Secure & Taksit:** Iyzico'nun sunduğu güvenli ödeme formları ve taksit seçenekleri backend tarafında yönetilir.
* **Anlık Rol Yükseltme:** Ödeme "Success" döndüğü milisaniye içerisinde, arka planda çalışan servisler kullanıcının  yetkisini `StandardUser`'dan `PremiumUser`'a yükseltir.

### 📊 2. Seq ile Merkezi Loglama ve Kullanıcı Yolculuğu (Observability)
Klasik metin dosyası loglamasının ötesinde, **Structured Logging (Yapılandırılmış Loglama)** mimarisi kullanılmıştır.
* **Canlı İzleme:** Sistemdeki her hata, uyarı veya bilgi mesajı anlık olarak Seq sunucusuna (Dashboard) düşer.
* **User Journey (Kullanıcı Yolculuğu):** Bir hata oluştuğunda, sadece hatayı değil; o kullanıcının sisteme girdiği andan hatayı aldığı ana kadar hangi sayfalara tıkladığı, ödeme adımında ne kadar beklediği gibi tüm akış `CorrelationId` ile takip edilebilir.
* **Sorgulanabilir Loglar:** Adminler, "Son 1 saatte ödeme hatası alan kullanıcılar kim?" gibi soruları S Seq üzerinden bulabiir.

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
| **Ödeme Altyapısı** | **Iyzico API Entegrasyonu** |
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

## 📊 Ekran Görüntüleri

<details>
<summary><strong>🔐 Login Ekranı</strong></summary>
<br>
<img width="1900" height="909" alt="Ekran görüntüsü 2026-03-04 103541" src="https://github.com/user-attachments/assets/66a43d70-5785-4e7f-94d9-00ad03e2456b" />
<img width="1890" height="911" alt="Ekran görüntüsü 2026-03-04 103444" src="https://github.com/user-attachments/assets/3bbecdbc-eff9-4c2b-855a-5c491a3211ba" />
<img width="1895" height="953" alt="Ekran görüntüsü 2026-03-04 155221" src="https://github.com/user-attachments/assets/67864a3d-dba8-41ec-8a56-800e02e0dcfa" />
<img width="1903" height="945" alt="Ekran görüntüsü 2026-03-04 155234" src="https://github.com/user-attachments/assets/128ee7ce-d70e-4ac5-9cc2-79712bc4cc04" />
<img width="1888" height="938" alt="Ekran görüntüsü 2026-03-04 155334" src="https://github.com/user-attachments/assets/2ce923c7-dc39-448f-8f99-55d08c8c9d12" />
</details>

<details>
<summary><strong>📝 Kayıt Ekranı</strong></summary>
<br>
<img width="1903" height="936" alt="Ekran görüntüsü 2026-03-04 155540" src="https://github.com/user-attachments/assets/d3762502-2e45-4755-b095-bc737b6e30df" />
<img width="1901" height="944" alt="Ekran görüntüsü 2026-03-04 155551" src="https://github.com/user-attachments/assets/e66fad88-b832-4421-bfd3-536e93d8bec4" />
</details>

<details>
<summary><strong>🔑 Şifremi Unuttum ve Maile Link Gönderme</strong></summary>
<br>
<img width="1907" height="817" alt="Ekran görüntüsü 2026-03-04 155742" src="https://github.com/user-attachments/assets/60088000-76d7-429a-831c-b9a2883259e8" />
<img width="1909" height="706" alt="Ekran görüntüsü 2026-03-04 155746" src="https://github.com/user-attachments/assets/44e07579-a9d6-47fa-a411-91bf0a259bd3" />
<img width="638" height="585" alt="Ekran görüntüsü 2026-03-04 165417" src="https://github.com/user-attachments/assets/a0dee005-703c-40bc-9004-a895a62ab569" />
<img width="1867" height="391" alt="Ekran görüntüsü 2026-03-04 103805" src="https://github.com/user-attachments/assets/ea59aca6-d5a9-41eb-8adb-9193a66a2b42" />
<img width="1255" height="907" alt="Ekran görüntüsü 2026-03-04 103839" src="https://github.com/user-attachments/assets/e812b714-71f7-4b24-a8d1-1a8aa2ee0f27" />

</details>

<details>
<summary><strong>📥 Gelen Kutusu Ana Sayfa</strong></summary>
<br>
<img width="1889" height="946" alt="Ekran görüntüsü 2026-03-04 155819" src="https://github.com/user-attachments/assets/12f70ba6-ead2-486c-ac6e-cc556e992d99" />
<img width="1884" height="935" alt="Ekran görüntüsü 2026-03-04 155830" src="https://github.com/user-attachments/assets/c0307c46-e5e2-4180-98e2-cbf24907bf52" />
</details>

<details>
<summary><strong>📄 Mesaj İçeriği</strong></summary>
<br>
<img width="1900" height="946" alt="Ekran görüntüsü 2026-03-04 155846" src="https://github.com/user-attachments/assets/f446063a-5430-43cf-967b-d69a44b8d85b" />
</details>

<details>
<summary><strong>⬇️ Mesajı PDF Formatında İndirme</strong></summary>
<br>
<img width="1897" height="903" alt="Ekran görüntüsü 2026-03-04 155855" src="https://github.com/user-attachments/assets/55063256-d1f6-4269-b317-27d3dc281962" />
<img width="1834" height="923" alt="Ekran görüntüsü 2026-03-04 155915" src="https://github.com/user-attachments/assets/ac9d8341-020c-4411-8f02-bea2e8566c81" />
</details>

<details>
<summary><strong>📎 Mesaj ile Gönderilen Resim ya da PDF İndirme</strong></summary>
<br>
<img width="1877" height="933" alt="Ekran görüntüsü 2026-03-04 155932" src="https://github.com/user-attachments/assets/156829c8-79cc-408e-8c66-aad2f3d91964" />
</details>

<details>
<summary><strong>📤 Mesaj Gönderme</strong></summary>
<br>
<img width="1859" height="846" alt="Ekran görüntüsü 2026-03-04 155953" src="https://github.com/user-attachments/assets/257034c2-965d-465d-9d21-153026e9613d" />
</details>

<details>
<summary><strong>📂 Mesaj Gönderirken Dosya Seçme</strong></summary>
<br>
<img width="1875" height="930" alt="Ekran görüntüsü 2026-03-04 160008" src="https://github.com/user-attachments/assets/92133d20-6995-4a08-a7a5-7ba82b419c13" />
</details>

<details>
<summary><strong>💾 Mesajı İstersek Taslak Olarak Kaydetme</strong></summary>
<br>
<img width="1888" height="928" alt="Ekran görüntüsü 2026-03-04 160024" src="https://github.com/user-attachments/assets/bc0574f4-fa66-4da5-b1ab-8a703f4a33b1" />
<img width="1888" height="930" alt="Ekran görüntüsü 2026-03-04 160030" src="https://github.com/user-attachments/assets/9dfc2d40-d8bc-4188-a244-9d16440c3e30" />
</details>

<details>
<summary><strong>📬 Gelen Kutusu</strong></summary>
<br>
<img width="1875" height="927" alt="Ekran görüntüsü 2026-03-04 160043" src="https://github.com/user-attachments/assets/7154293f-72d1-4091-b9c7-813235862d2f" />
</details>

<details>
<summary><strong>🤖 ML.NET Spam Algılananlar (Spam Kutusu)</strong></summary>
<br>
<img width="1869" height="933" alt="Ekran görüntüsü 2026-03-04 160054" src="https://github.com/user-attachments/assets/b6d8c08e-617b-468b-a093-8180437f69f4" />
<img width="1882" height="944" alt="Ekran görüntüsü 2026-03-04 160100" src="https://github.com/user-attachments/assets/36e18382-ca47-47b0-a26a-0d38d0aecd5a" />
<img width="1894" height="936" alt="Ekran görüntüsü 2026-03-04 160107" src="https://github.com/user-attachments/assets/f9dedabb-de50-4726-9c4d-6edd5e6c36c5" />
</details>

<details>
<summary><strong>🗂️ Kategorilere Göre Gelen Mesajlar</strong></summary>
<br>
<img width="1903" height="931" alt="Ekran görüntüsü 2026-03-04 160119" src="https://github.com/user-attachments/assets/9945e694-30cd-4dcf-84ec-c12ec432cd67" />
</details>

<details>
<summary><strong>👤 Profilim & Güncelleme</strong></summary>
<br>
<img width="1890" height="770" alt="Ekran görüntüsü 2026-03-04 160132" src="https://github.com/user-attachments/assets/92a9c359-9a85-4094-8776-7c410f296638" />
<img width="1883" height="878" alt="Ekran görüntüsü 2026-03-04 160141" src="https://github.com/user-attachments/assets/2dc4f46f-4804-4b23-b707-63f810bf91dd" />
<img width="1891" height="927" alt="Ekran görüntüsü 2026-03-04 160145" src="https://github.com/user-attachments/assets/61a7ca11-0152-4ceb-9e97-9430a8cb7b1e" />
<img width="1884" height="826" alt="Ekran görüntüsü 2026-03-04 160234" src="https://github.com/user-attachments/assets/5d17de42-7176-46af-ba9c-3177dc05a5fd" />
</details>

<details>
<summary><strong>🔄 Şifre Değiştirme</strong></summary>
<br>
<img width="1894" height="906" alt="Ekran görüntüsü 2026-03-04 160253" src="https://github.com/user-attachments/assets/ad62597d-8cca-4a74-834c-93a2c3109548" />
</details>

<details>
<summary><strong>❌ Hesap Kapatma & Maile Kod Gönderme</strong></summary>
<br>
<img width="1895" height="909" alt="Ekran görüntüsü 2026-03-04 160308" src="https://github.com/user-attachments/assets/f10c6cf9-857f-484f-afa4-27c22c42b01b" />
<img width="378" height="70" alt="Ekran görüntüsü 2026-03-04 170511" src="https://github.com/user-attachments/assets/b9887d75-27b4-49ae-81ee-a6df20c8c811" />
</details>

<details>
<summary><strong>📦 Paketler</strong></summary>
<br>
<img width="1881" height="823" alt="Ekran görüntüsü 2026-03-04 160322" src="https://github.com/user-attachments/assets/b4dee502-9d8d-4c16-bd86-bbc4e060b028" />
<img width="1882" height="952" alt="Ekran görüntüsü 2026-03-04 160327" src="https://github.com/user-attachments/assets/55d4a27d-23d9-4b5a-8a2e-be0ae4b5e364" />
</details>

<details>
<summary><strong>💳 İyzico Ödeme Sistemi (Kredi ve Banka Kartına Göre Ücret Artışı/Faiz)</strong></summary>
<br>
<img width="1878" height="911" alt="Ekran görüntüsü 2026-03-04 160408" src="https://github.com/user-attachments/assets/2891ab1f-e21c-4f64-bb8f-2a7a6cdd1470" />
<img width="1407" height="852" alt="Ekran görüntüsü 2026-03-04 160432" src="https://github.com/user-attachments/assets/4f7e989a-a4f8-4558-b56c-6f414da3cd15" />
<img width="1230" height="889" alt="Ekran görüntüsü 2026-03-04 160452" src="https://github.com/user-attachments/assets/f484d184-de15-4735-98fd-000fd6990d8a" />
<img width="1677" height="928" alt="Ekran görüntüsü 2026-03-04 160516" src="https://github.com/user-attachments/assets/73a41e56-821e-40ad-a375-02820d7753cf" />
<img width="1131" height="584" alt="Ekran görüntüsü 2026-03-04 160546" src="https://github.com/user-attachments/assets/488fdb53-e723-4331-89e0-d6599eec972e" />
<img width="1540" height="753" alt="Ekran görüntüsü 2026-03-04 160552" src="https://github.com/user-attachments/assets/93842137-77f1-4788-b540-c4d68269a562" />
<img width="1163" height="872" alt="Ekran görüntüsü 2026-03-04 160625" src="https://github.com/user-attachments/assets/22408e09-a4f4-4f0d-8ddd-e78943bac790" />
<img width="1015" height="654" alt="Ekran görüntüsü 2026-03-04 160631" src="https://github.com/user-attachments/assets/d7329ae1-214d-4dce-a3fe-6733698a67e2" />
<img width="833" height="660" alt="Ekran görüntüsü 2026-03-03 192921" src="https://github.com/user-attachments/assets/7aa07243-bb05-4a6f-aa56-2c7a583a6803" />
</details>

<details>
<summary><strong>✅ İyzico Ödeme Sonuçları</strong></summary>
<br>
<img width="1629" height="890" alt="Ekran görüntüsü 2026-03-04 170106" src="https://github.com/user-attachments/assets/7ee55d1a-7e07-4e8e-b2bc-92b4e0ce599a" />
</details>

<details>
<summary><strong>🧠 Claude AI ile Mail Örnek ve Destek Alma</strong></summary>
<br>
<img width="1899" height="942" alt="Ekran görüntüsü 2026-03-04 160720" src="https://github.com/user-attachments/assets/a00aef5d-517e-41c5-8c41-c062f907b444" />
<img width="1894" height="774" alt="Ekran görüntüsü 2026-03-04 160824" src="https://github.com/user-attachments/assets/82c13906-7694-4598-8d6e-aad64da682c8" />
<img width="1668" height="826" alt="Ekran görüntüsü 2026-03-04 160925" src="https://github.com/user-attachments/assets/6b0fb475-9519-4e85-bc49-2ad39bab88f2" />
<img width="1898" height="863" alt="Ekran görüntüsü 2026-03-04 161027" src="https://github.com/user-attachments/assets/eb9645b5-4116-4b86-9224-7941afcbb1ce" />
<img width="1902" height="935" alt="Ekran görüntüsü 2026-03-04 161034" src="https://github.com/user-attachments/assets/39ae5f2a-9449-44a4-bd39-3ca1a2c7eda7" />
</details>

<details>
<summary><strong>✉️ Kişiye Özel Direkt Mesaj Atma ve Listesi</strong></summary>
<br>
<img width="1888" height="939" alt="Ekran görüntüsü 2026-03-04 161055" src="https://github.com/user-attachments/assets/10e4143c-cb32-4f7f-8eef-c16b98bfdab9" />
<img width="1901" height="911" alt="Ekran görüntüsü 2026-03-04 161108" src="https://github.com/user-attachments/assets/10f9a009-9d8f-42a6-8ac4-434cc9ea0114" />
<img width="1882" height="880" alt="Ekran görüntüsü 2026-03-04 161113" src="https://github.com/user-attachments/assets/97002168-ccf9-45dd-94ee-3f4ee6707f6f" />
</details>

<details>
<summary><strong>👑 Admin Kullanıcılar ve Rol Atama</strong></summary>
<br>
<img width="1892" height="926" alt="Ekran görüntüsü 2026-03-04 161206" src="https://github.com/user-attachments/assets/c493bd9d-212f-44fc-bfe3-4453cdab3ce9" />
<img width="1900" height="662" alt="Ekran görüntüsü 2026-03-04 161215" src="https://github.com/user-attachments/assets/46e3a861-c7c9-419c-94de-8e89ee2a1f0d" />
<img width="1892" height="527" alt="Ekran görüntüsü 2026-03-04 161222" src="https://github.com/user-attachments/assets/0ab33fd3-03a2-4ceb-befc-a4a455b95a6d" />
</details>

<details>
<summary><strong>📊 Admin Seq ile Özel Log Ekranı</strong></summary>
<br>
<img width="1910" height="886" alt="Ekran görüntüsü 2026-03-04 161234" src="https://github.com/user-attachments/assets/790810b4-267b-4ce0-9b7e-6071c4015753" />
<img width="1900" height="915" alt="Ekran görüntüsü 2026-03-04 161257" src="https://github.com/user-attachments/assets/c4849fe3-0316-481f-aa11-533413adb867" />
<img width="1905" height="910" alt="Ekran görüntüsü 2026-03-04 102734" src="https://github.com/user-attachments/assets/6915cc2b-89c0-4fbb-9f9c-fccefcef023c" />
<img width="1888" height="905" alt="Ekran görüntüsü 2026-03-04 102742" src="https://github.com/user-attachments/assets/a02ea000-9b5f-4263-9e34-644ce6edf3b7" />
<img width="1905" height="897" alt="Ekran görüntüsü 2026-03-04 102826" src="https://github.com/user-attachments/assets/544c6054-5fde-4821-b708-15233feff16b" />
</details>

<details>
<summary><strong>🚫 404 ve 403 Sayfaları</strong></summary>
<br>
<img width="1329" height="645" alt="Ekran görüntüsü 2026-03-04 162504" src="https://github.com/user-attachments/assets/b1121189-9ff7-40f7-82ec-fc5cf4f66960" />
<img width="1218" height="606" alt="Ekran görüntüsü 2026-03-04 162518" src="https://github.com/user-attachments/assets/616c8a92-1433-4f11-afe9-db64fbed6c70" />
</details>

<details>
<summary><strong>🏗️ Kod Yapısı</strong></summary>
<br>
<img width="376" height="911" alt="image" src="https://github.com/user-attachments/assets/7a1331ba-8dd2-4881-9fff-3e5290643a3d" />
</details>

<details>
<summary><strong>📈 Seq Dashboard</strong></summary>
<br>
<img width="1618" height="935" alt="image" src="https://github.com/user-attachments/assets/053f9750-1d4f-40d7-8f99-1d5b7b366ea9" />
</details>

<details>
<summary><strong>🌐 Facebook ve Google ile Giriş</strong></summary>
<br>
<img width="1884" height="827" alt="Ekran görüntüsü 2026-03-04 104339" src="https://github.com/user-attachments/assets/9516a024-9be6-4d46-a27e-451f159cb3a3" />
<img width="1884" height="905" alt="Ekran görüntüsü 2026-03-04 104402" src="https://github.com/user-attachments/assets/6f5ca389-c406-4ba0-875d-7ff26b0a0e9c" />
<img width="1781" height="866" alt="Ekran görüntüsü 2026-03-04 104412" src="https://github.com/user-attachments/assets/053f7551-c32b-434e-b12e-b950705c67d6" />
<img width="1606" height="909" alt="Ekran görüntüsü 2026-03-04 100613" src="https://github.com/user-attachments/assets/8911bebc-d0ca-4c7b-89e1-05306d8ce535" />
<img width="1883" height="918" alt="Ekran görüntüsü 2026-03-04 101439" src="https://github.com/user-attachments/assets/3a8da40c-6624-4748-9358-10aa86b72a40" />
<img width="1883" height="914" alt="Ekran görüntüsü 2026-03-04 101615" src="https://github.com/user-attachments/assets/5e420d57-5a80-48c0-88b5-525f57b8e944" />
<img width="1603" height="914" alt="Ekran görüntüsü 2026-03-04 101641" src="https://github.com/user-attachments/assets/fcf3df5f-07ab-4885-95f4-ffeb7498df88" />
<img width="1653" height="916" alt="Ekran görüntüsü 2026-03-04 101651" src="https://github.com/user-attachments/assets/1a0438bf-0edd-481a-8454-314128940402" />
<img width="551" height="182" alt="Ekran görüntüsü 2026-03-04 101730" src="https://github.com/user-attachments/assets/f26816eb-cc13-43c3-b0d5-95a39a7df164" />
</details>




## ⚙️ Kurulum

Projeyi yerel ortamınızda sorunsuz çalıştırmak için aşağıdaki adımları izleyin:

1.  **Projeyi Klonlayın**
    ```bash
    git clone [https://github.com/BURAYA-REPO-ADRESINIZI-YAZIN.git](https://github.com/BURAYA-REPO-ADRESINIZI-YAZIN.git)
    cd proje-adi
    ```

2.  **Gerekli Ayarlamalar (appsettings.json)**
    Projenin düzgün çalışması için `appsettings.json` dosyasındaki şu alanları kendi bilgilerinizle güncelleyin:
    * **Connection Strings:** Veritabanı bağlantı cümleniz.
    * **Mail Ayarları:** SMTP bilgileri (Şifremi unuttum vb. işlemler için).
    * **Iyzico:** API ve Secret Key bilgileri.
    * **Auth:** Google ve Facebook Login için Client ID ve Secret bilgileri.
    * **Seq:** Loglama için (`http://localhost:5341` varsayılan).

3.  **Veritabanı Migrations**
    Terminal veya Package Manager Console üzerinden veritabanını oluşturun:
    ```bash
    dotnet ef database update
    ```

4.  **Projeyi Başlatın**
    ```bash
    dotnet run
    ```

<div align="center">

## 🎥 Proje Videosu ve İletişim

Projenin çalışır halini görmek, kurulum videosunu izlemek veya bana soru sormak için LinkedIn profilime göz atabilirsiniz.

<a href="https://www.linkedin.com/in/burakhanulusoy/" target="_blank">
  <img src="https://img.shields.io/badge/LinkedIn-Burakhan%20Ulusoy-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" alt="LinkedIn Hesabım" />
</a>

<br>
<br>

📫 **Bana Ulaşın:** [linkedin.com/in/burakhanulusoy](https://www.linkedin.com/in/burakhanulusoy/)

</div>












