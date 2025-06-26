using System.ComponentModel.DataAnnotations;

namespace ShopOrderMicroservice.Models
{
    public class OrderStatus
    {
        [Key]
        public int Id { get; set; }
        public string Status { get; set; }
        public string City { get; set; }
    }
}
