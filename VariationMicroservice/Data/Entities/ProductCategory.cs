using System.ComponentModel.DataAnnotations;

namespace VariationMicroservice.Data.Entities
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = null!;
        
        public string? Description { get; set; }
        
        // Navigation property
        public ICollection<Variation> Variations { get; set; } = new List<Variation>();
    }
}