namespace ShippingTypeMicroservice.Entities
{
    public class ShippingType
    {
        public int Id { get; set; }
        public string Method { get; set; } = null!;
        public decimal Price { get; set; } // ➕ Senin eklediğin alan
    }
}