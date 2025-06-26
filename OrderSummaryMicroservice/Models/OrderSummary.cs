using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderSummaryMicroservice.Models
{
    public class OrderSummary
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ShopOrder")]
        public int OrderId { get; set; }
        // We will not have a direct navigation property to ShopOrder here
        // as it resides in a different microservice. The relationship will be handled via Id.

        public int ProductItemId { get; set; }
        // We will not have a direct navigation property to ProductItem here
        // as it resides in a different microservice. The relationship will be handled via Id.

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}