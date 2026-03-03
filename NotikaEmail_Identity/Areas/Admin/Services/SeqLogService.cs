using NotikaEmail_Identity.Models;
using Seq.Api;

public class SeqLogService
{
    private readonly string _seqUrl = "http://localhost:5341";
    private readonly IConfiguration _configuration;

    public SeqLogService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<AdminLogViewModel>> GetSystemLogsAsync()
    {
        var apiKey = _configuration["Seq:ApiKey"];
        var connection = new SeqConnection(_seqUrl, apiKey);

        // 🔥 KRİTİK DEĞİŞİKLİK BURADA 🔥
        // "SourceContext", logun hangi Class'tan atıldığını söyler.
        // 'NotikaEmail_Identity%' diyerek, senin projendeki (Controller, Service vb.)
        // tüm classlardan gelen logları alıyoruz. Microsoft veya System loglarını eliyoruz.

        var filter = "SourceContext like 'NotikaEmail_Identity%'";

        // İstersen Debug loglarını hariç tutup sadece önemli olanları (Info, Warning, Error) almak için şunu kullanabilirsin:
        // var filter = "SourceContext like 'NotikaEmail_Identity%' and @Level in ['Information', 'Warning', 'Error', 'Fatal']";

        var result = await connection.Events.ListAsync(filter: filter, count: 200, render: true);

        return result.Select(x => new AdminLogViewModel
        {
            // Zamanı yerel saate çevirme
            Timestamp = DateTimeOffset.Parse(x.Timestamp).ToLocalTime(),
            Level = x.Level,
            Message = x.RenderedMessage,
            // Logun hangi class'tan geldiğini de görmek istersen (Opsiyonel):
            // Source = x.Properties.FirstOrDefault(p => p.Name == "SourceContext")?.Value?.ToString(), 

            LogColor = x.Level switch
            {
                "Information" => "text-info",  // Mavi
                "Warning" => "text-warning",   // Sarı
                "Error" => "text-danger",      // Kırmızı
                "Fatal" => "text-danger fw-bold", // Kalın Kırmızı
                _ => "text-dark"
            }
        })
        .OrderByDescending(x => x.Timestamp) // En yeniler en üstte
        .ToList();
    }
}