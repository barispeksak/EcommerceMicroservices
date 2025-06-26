using System.ComponentModel.DataAnnotations;

namespace ShopOrderMicroservice.Models
{
    public class PaymentType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Method { get; set; } 
    }
}
