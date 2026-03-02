using System.ComponentModel.DataAnnotations.Schema;

namespace Core_IyzicoPaymentSystem.Entities
{
    public class OrderLine
    {

        public int OrderLineId { get; set; }
        public string? Name { get; set; }

        [Column(TypeName ="decimal(18,2)")]
        public decimal Price { get; set; }

        public int  Quantity { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;





    }
}
