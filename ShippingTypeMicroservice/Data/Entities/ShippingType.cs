namespace ShippingTypeMicroservice.Entities
{
    public class ShippingType
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
