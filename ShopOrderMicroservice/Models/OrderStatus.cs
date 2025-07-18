using System.ComponentModel.DataAnnotations;

namespace ShopOrderMicroservice.Models
{
    public class OrderStatus
    {
        [Key]
        public int Id { get; set; }
        public required string Status { get; set; }
        public required string City { get; set; }
    }
}
