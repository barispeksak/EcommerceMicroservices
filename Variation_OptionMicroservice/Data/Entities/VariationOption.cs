using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Variation_OptionMicroservice.Data.Entities
{
    public class VariationOption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Value { get; set; } = null!;  // Örn: Mavi, S, 42

        public string? AdditionalInfo { get; set; } // Örn: Renk kodu, stok kodu vs.

        [ForeignKey("Variation")]
        public int VariationId { get; set; }

        public virtual Variation? Variation { get; set; }
    }
}