namespace Iyzico.Models
{
    public class PaymentRequest
    {
        public string Locale { get; set; } = null!;
        public string ConversationId { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal PaidPrice { get; set; }
        public string Currency { get; set; } = null!;
        public int Installment { get; set; }
        public string PaymentChannel { get; set; } = null!;
        public string BasketId { get; set; } = null!;
        public string PaymentGroup { get; set; } = null!;
        public string CallbackUrl { get; set; } = null!;

        public PaymentCard PaymentCard { get; set; } = null!;
        public Buyer Buyer { get; set; } = null!;
        public ShippingAddress ShippingAddress { get; set; } = null!;
        public BillingAddress BillingAddress { get; set; } = null!;
        public List<BasketItem> BasketItems { get; set; } = null!;
    }

    public class PaymentCard
    {
        public string CardHolderName { get; set; } = null!;
        public string CardNumber { get; set; } = null!;
        public string ExpireYear { get; set; } = null!;
        public string ExpireMonth { get; set; } = null!;
        public string Cvc { get; set; } = null!;
    }

    public class Buyer
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string IdentityNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string GsmNumber { get; set; } = null!;
        public string RegistrationDate { get; set; } = null!;
        public string LastLoginDate { get; set; } = null!;
        public string RegistrationAddress { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string ZipCode { get; set; } = null!;
        public string Ip { get; set; } = null!;
    }

    public class ShippingAddress
    {
        public string Address { get; set; } = null!;  // JSON'daki "address" alanı için
        public string ZipCode { get; set; } = null!;
        public string ContactName { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Country { get; set; } = null!;
    }

    public class BillingAddress
    {
        public string Address { get; set; } = null!;  // JSON'daki "address" alanı için
        public string ZipCode { get; set; } = null!;
        public string ContactName { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Country { get; set; } = null!;
    }

    public class BasketItem
    {
        public string Id { get; set; } = null!;
        public decimal Price { get; set; }
        public string Name { get; set; } = null!;
        public string Category1 { get; set; } = null!;
        public string Category2 { get; set; } = null!;
        public string ItemType { get; set; } = null!;
    }
}