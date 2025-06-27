namespace VariationMicroservice.Service.DTOs
{
    public class VariationOptionDto
    {
        public int Id { get; set; }
        public string OptionName { get; set; } = null!;
        public string? OptionValue { get; set; }
    }
}