using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ProductService.Models
{

    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Brand { get; set; }

        public string Image { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public ProductCategory Category { get; set; }

        public ICollection<ProductItem> Items { get; set; } 
    
    }
    
    

}