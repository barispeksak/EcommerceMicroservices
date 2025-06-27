namespace AddressMicroservice.Service.DTOs
{
    public class UpdateAddressDto
    {
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}