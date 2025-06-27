namespace VariationMicroservice.Service.DTOs
{
    public class UpdateVariationDto
    {
        public int Id { get; set; }
        public string VarTypeName { get; set; } = null!;
        public int CategoryId { get; set; }
    }
}