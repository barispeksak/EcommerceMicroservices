using System.ComponentModel.DataAnnotations;

namespace VariationMicroservice.Data.Entities;

public class Variation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string VarTypeName { get; set; } = null!;

    // Sadece CategoryId tut, navigation yok!
    public int CategoryId { get; set; }
}
