using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderStatusMicroservice.Models
{
    public class OrderStatus
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ShopOrder")]
        public int ShopOrderId { get; set; }

        public string Status { get; set; } = null!;
        public string City { get; set; } = null!;
    }
}

