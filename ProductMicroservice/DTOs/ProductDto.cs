// ProductDto.cs
using System.ComponentModel.DataAnnotations; 
namespace ProductService.DTOs;
public class ProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Brand { get; set; }
    public string Image { get; set; }
    public int CategoryId { get; set; } // sadece ID
    public List<CreateProductItemDto> Items { get; set; }
}

public class CreateProductItemDto
{
    public string Sku { get; set; }
    public int QuantityInStock { get; set; }
    public decimal Price { get; set; }
    public string ProductImage { get; set; }
}
