using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderStatusMicroservice.Models
{
    public class OrderStatus
    {
        [Key]
        public int Id { get; set; }

        public Guid OrderId { get; set; }

        [ForeignKey("ShopOrder")]
        public int ShopOrderId { get; set; }

        public string Status { get; set; } = null!;
        public string City { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

