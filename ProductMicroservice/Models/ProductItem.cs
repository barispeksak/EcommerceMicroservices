using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ProductService.Models
{
    public class ProductItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SKU { get; set; }

        [Required]
        public int QuantityInStock { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string ProductImage { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product Product { get; set; }
    }
}