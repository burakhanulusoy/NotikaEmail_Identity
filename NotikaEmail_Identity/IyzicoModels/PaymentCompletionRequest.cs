namespace Core_IyzicoPaymentSystem.Models
{
    public class PaymentCompletionRequest
    {
        public string? Locale { get; set; }
        public string? ConversationId { get; set; }
        public string? PaymentId { get; set; }
        public string? PaidPrice { get; set; }
        public string? BasketId { get; set; }
        public string? Currency { get; set; }
    }
}
