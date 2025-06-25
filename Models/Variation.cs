using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ProductService.Models
{
    public class Variation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string VarTypeName { get; set; }  // Örn: Renk, Beden, Numara

        // Bu varyasyon hangi kategoriye ait?
        [ForeignKey("ProductCategory")]
        public int CategoryId { get; set; }
        public ProductCategory Category { get; set; }

        // Navigation: Seçenekler
        public ICollection<VariationOption> Options { get; set; }
    }
}