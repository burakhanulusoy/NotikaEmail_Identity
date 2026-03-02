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

        // KISITLAMAYI KALDIRDIK: Artık sadece Error/Warning değil, ödeme süreciyle ilgili tüm adımları (Info dahil) getirecek.
        // Ayrıca loglarında yer alan "Taksit" kelimesini de ekledik ki taksit hatalarını/başarılarını da görebilesin.
        var filter = "@Message like '%Sistem%' or @Message like '%Iyzico%' or @Message like '%Ödeme%' or @Message like '%Sipariş%' or @Message like '%Taksit%'";

        // render: true ekleyerek mesajın tam metin gelmesini garantiye alıyoruz
        // count: 50 değerini istersen artırabilirsin (örneğin 100 yapabilirsin), tüm adımları net görmek için.
        var result = await connection.Events.ListAsync(filter: filter, count: 100, render: true);

        return result.Select(x => new AdminLogViewModel
        {
            // Zamanı yerel saate çevirelim ki tabloda doğru görünsün
            Timestamp = DateTimeOffset.Parse(x.Timestamp).ToLocalTime(),
            Level = x.Level,
            Message = x.RenderedMessage,
            LogColor = x.Level switch
            {
                "Information" => "text-info",
                "Warning" => "text-warning",
                "Error" => "text-danger",
                "Fatal" => "text-danger fw-bold",
                _ => "text-dark"
            }
        })
        // En güncel log en üstte çıksın diye listeyi tarihe göre tersine sıralıyoruz
        .OrderByDescending(x => x.Timestamp)
        .ToList();
    }
}