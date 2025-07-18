using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShopOrderMicroservice.Data.Entities;

namespace ShopOrderMicroservice.Models
{
    [Table("order_summary")]
    public class OrderSummary
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("product_item_id")]
        public int ProductItemId { get; set; }

        [Column("qty")]
        public int Quantity { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        // 🔗 Navigation property'ler
        public required ShopOrder ShopOrder { get; set; }
    }
}
