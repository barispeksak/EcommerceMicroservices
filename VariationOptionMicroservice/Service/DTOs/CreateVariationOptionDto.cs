using Swashbuckle.AspNetCore.Annotations;


namespace VariationOptionMicroservice.Service.DTOs
{
    public class CreateVariationOptionDto
    {
        public string Value { get; set; } = null!;
        public string? AdditionalInfo { get; set; }
        public int VariationId { get; set; }
    }
}