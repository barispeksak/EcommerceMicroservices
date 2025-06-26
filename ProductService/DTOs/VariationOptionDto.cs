using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class VariationOptionDto
{
    [Required] public string Value { get; set; } = null!;
    [Required] public int VariationId { get; set; }
}
