namespace PaymentTypeMicroservice.Entities
{
    public class PaymentType
    {
        public int Id { get; set; }
        public string Method { get; set; } = null!;
    }
}
