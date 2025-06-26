using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class VariationDto
{
    [Required] public string VarTypeName { get; set; } = null!;
    [Required] public int CategoryId { get; set; }
}