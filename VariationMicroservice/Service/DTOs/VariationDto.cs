using Swashbuckle.AspNetCore.Annotations;

namespace VariationMicroservice.Service.DTOs
{
    public class VariationDto
    {
        public int Id { get; set; }
        public string VarTypeName { get; set; } = null!;
        public int CategoryId { get; set; }

    }
}