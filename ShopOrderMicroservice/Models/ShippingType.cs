using System.ComponentModel.DataAnnotations;

namespace ShopOrderMicroservice.Models
{
    public class ShippingType
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
    }
}
