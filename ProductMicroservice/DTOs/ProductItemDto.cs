using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class ProductItemDto
{
    [Required] public string Sku { get; set; } = null!;
    [Required] public int QuantityInStock { get; set; }
    [Required] public decimal Price { get; set; }
    public string? ProductImage { get; set; }
}