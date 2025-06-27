using System.ComponentModel.DataAnnotations;

namespace Variation_OptionMicroservice.Data.Entities
{
    public class Variation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string VarTypeName { get; set; } = null!;  // Örn: Renk, Beden, Numara

        public int CategoryId { get; set; }

        public virtual ICollection<VariationOption> Options { get; set; } = new List<VariationOption>();
    }
}