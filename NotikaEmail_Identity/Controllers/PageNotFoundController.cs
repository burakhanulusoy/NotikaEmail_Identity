using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace NotikaEmail_Identity.Controllers
{
    public class PageNotFoundController(ILogger<PageNotFoundController> _logger) : Controller
    {
        public IActionResult Index()
        {
            // 404 hatası alan orijinal yolu (path) yakalıyoruz
            var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            string originalPath = statusCodeReExecuteFeature?.OriginalPath ?? "Bilinmiyor";

            // SEQ LOGU: Birisi olmayan bir yere gittiğinde sarı uyarı fırlatır
            _logger.LogWarning("Sistem Uyarısı: Bir kullanıcı mevcut olmayan bir sayfaya erişmeye çalıştı. Hatalı URL: {OriginalPath}", originalPath);
            return View();
        }
    }
}
