using System.ComponentModel.DataAnnotations.Schema;

namespace Core_IyzicoPaymentSystem.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? OrderNo { get; set; }

        [Column(TypeName ="decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidPrice { get; set; }

        public bool Status { get; set; }
        public string? Message { get; set; }
        public List<OrderLine> OrderLines { get; set; } = [];











    }


}
