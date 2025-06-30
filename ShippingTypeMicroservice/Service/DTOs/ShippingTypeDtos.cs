// Data/Dtos/ShippingTypeDtos.cs
namespace ShippingTypeMicroservice.Data.Dtos
{
    public class CreateShippingTypeDto
    {
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class UpdateShippingTypeDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public class ShippingTypeDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
