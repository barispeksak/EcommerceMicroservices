using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VariationMicroservice.Data.Entities
{
    public class VariationOption
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string OptionName { get; set; } = null!; // Örn: Kırmızı, L, 42
        
        public string? OptionValue { get; set; } // Örn: #FF0000 (renk kodu)
        
        [ForeignKey("Variation")]
        public int VariationId { get; set; }
        public Variation Variation { get; set; } = null!;
    }
}