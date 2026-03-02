using System.Text.Json.Serialization;

namespace Core_IyzicoPaymentSystem.Models
{
    public class InstallmentResponse
    {

        public bool Success { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("installmentDetails")]
        public List<InstallmentDetailModel> InstallmentDetails { get; set; } = new();
    }

    public class InstallmentDetailModel
    {
        [JsonPropertyName("installmentPrices")]
        public List<InstallmentPriceModel> InstallmentPrices { get; set; } = new();
    }

    public class InstallmentPriceModel
    {
        [JsonPropertyName("installmentPrice")]
        public decimal InstallmentPrice { get; set; }

        [JsonPropertyName("totalPrice")]
        public decimal TotalPrice { get; set; }

        [JsonPropertyName("installmentNumber")]
        public int InstallmentNumber { get; set; }
    }








}
