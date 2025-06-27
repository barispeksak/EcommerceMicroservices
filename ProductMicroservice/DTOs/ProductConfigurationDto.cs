using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class ProductConfigurationDto
{
    [Required] public int ProductItemId { get; set; }
    [Required] public int VariationOptionId { get; set; }
}