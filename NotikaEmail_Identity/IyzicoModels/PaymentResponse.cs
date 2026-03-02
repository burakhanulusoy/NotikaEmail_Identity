using System.Text.Json.Serialization;

namespace Core_IyzicoPaymentSystem.Models
{
    public class PaymentResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("threeDSHtmlContent")]
        public string? ThreeDSHtmlContent { get; set; }
        [JsonPropertyName("errorCode")]

        public string? ErrorCode { get; set; }
        [JsonPropertyName("errorMessage")]

        public string? ErrorMessage { get; set; }







    }
}
