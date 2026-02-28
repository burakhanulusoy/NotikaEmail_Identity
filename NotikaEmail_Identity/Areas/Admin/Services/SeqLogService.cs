using Microsoft.Extensions.Configuration;
using NotikaEmail_Identity.Models;
using Seq.Api;

public class SeqLogService
{
    private readonly string _seqUrl = "http://localhost:5341";
    private readonly IConfiguration _configuration;

    // Constructor'da IConfiguration'ı enjekte ettik
    public SeqLogService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<AdminLogViewModel>> GetSystemLogsAsync()
    {
        var apiKey = _configuration["Seq:ApiKey"];
        var connection = new SeqConnection(_seqUrl, apiKey);

        // SQL vari "like" yerine, Seq UI'da çalışan düz kelimeyi gönderiyoruz
        // Mesajın içinde "Sistem" geçenleri getir
        var filter = "@Message like '%Sistem%'";

        // VEYA önce filtrenin soruna yol açıp açmadığını anlamak için filtreyi tamamen boş bırak:
        // var filter = null;

        // render: true ekleyerek mesajın tam metin gelmesini garantiye alıyoruz
        var result = await connection.Events.ListAsync(filter: filter, count: 50, render: true);

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
                _ => "text-dark"
            }
        }).ToList();
    }
}