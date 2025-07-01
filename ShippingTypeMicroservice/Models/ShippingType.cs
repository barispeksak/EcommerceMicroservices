using System.ComponentModel.DataAnnotations;

namespace ShippingTypeMicroservice.Models
{
    public class ShippingType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;
    }
}
