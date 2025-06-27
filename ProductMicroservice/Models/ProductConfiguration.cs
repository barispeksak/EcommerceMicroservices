using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ProductService.Models
{
    public class ProductConfiguration
    {

        [Key]
        public int Id { get; set; }

        [ForeignKey("ProductItem")]
        public int ProductItemId { get; set; }
        public ProductItem ProductItem { get; set; }

        [ForeignKey("VariationOption")]
        public int VariationOptionId { get; set; }
        public VariationOption VariationOption { get; set; }
   }
}