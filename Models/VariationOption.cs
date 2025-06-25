using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductService.Models
{
    public class VariationOption
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Value { get; set; }  // Örn: Mavi, S, 42

        [ForeignKey("Variation")]
        public int VariationId { get; set; }

        public Variation Variation { get; set; }
    }
}
