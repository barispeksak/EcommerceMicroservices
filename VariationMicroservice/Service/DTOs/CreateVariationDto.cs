namespace VariationMicroservice.Service.DTOs
{
    public class CreateVariationDto
    {
        public string VarTypeName { get; set; } = null!;
        public int CategoryId { get; set; }
    }
}