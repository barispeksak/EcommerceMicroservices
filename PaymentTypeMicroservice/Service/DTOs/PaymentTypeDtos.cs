// Data/Dtos/PaymentTypeDtos.cs
namespace PaymentTypeMicroservice.Data.Dtos
{
    public class CreatePaymentTypeDto
    {
        public string Type { get; set; } = string.Empty;
    }

    public class UpdatePaymentTypeDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;

    }

    public class PaymentTypeDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
