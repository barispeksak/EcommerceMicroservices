namespace VariationMicroservice.Service.DTOs
{
    public class VariationDto
    {
        public int Id { get; set; }
        public string VarTypeName { get; set; } = null!;
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public List<VariationOptionDto> Options { get; set; } = new List<VariationOptionDto>();
    }
}