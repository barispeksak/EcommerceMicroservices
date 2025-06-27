namespace Variation_OptionMicroservice.Service.DTOs
{
    public class VariationOptionDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = null!;
        public string? AdditionalInfo { get; set; }
        public int VariationId { get; set; }
        public string? VariationName { get; set; }
    }
}