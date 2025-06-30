using System.ComponentModel.DataAnnotations;

namespace PaymentTypeMicroservice.Models
{
    public class PaymentType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Method { get; set; } = string.Empty;
    }
}
