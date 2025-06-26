using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class ProductCategoryDto
{
    [Required]                  // => Swagger’da “zorunlu” olur
    public string CategoryName { get; set; } = null!;

    // Opsiyonel: Alt-kategori hiyerarşisi kuracaksan
    public int? ParentCategoryId { get; set; }
}
