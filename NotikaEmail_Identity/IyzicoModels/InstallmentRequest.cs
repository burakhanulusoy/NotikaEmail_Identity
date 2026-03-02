namespace Core_IyzicoPaymentSystem.Models
{
    public class InstallmentRequest
    {
        public string? Locale { get; set; }
        public decimal Price { get; set; }
        public string? BinNumber { get; set; }
    }
}
