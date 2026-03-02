using System.Text.Json.Serialization;

namespace Core_IyzicoPaymentSystem.Models
{
    public class PaymentCompletionResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }
    }
}
