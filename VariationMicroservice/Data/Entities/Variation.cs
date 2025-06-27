using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VariationMicroservice.Data.Entities
{
    public class Variation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string VarTypeName { get; set; } = null!;  // Örn: Renk, Beden, Numara

        // Bu varyasyon hangi kategoriye ait?
        [ForeignKey("ProductCategory")]
        public int CategoryId { get; set; }
        public ProductCategory Category { get; set; } = null!;

        // Navigation: Seçenekler
        public ICollection<VariationOption> Options { get; set; } = new List<VariationOption>();
    }
}