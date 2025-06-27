using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ProductService.Models
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CategoryName { get; set; }

        // 🔁 Self-Referencing: Üst kategori (nullable olabilir)
        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public ProductCategory ParentCategory { get; set; }

       //Bağlı kategoriler
        public ICollection<ProductCategory> SubCategories { get; set; }

        public ICollection<Product> Products { get; set; }

    }
}