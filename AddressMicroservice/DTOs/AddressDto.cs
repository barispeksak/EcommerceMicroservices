using System.ComponentModel.DataAnnotations;

namespace AddressMicroservice.DTOs
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAddressDto
    {
        [Required(ErrorMessage = "Address line is required")]
        [StringLength(500, ErrorMessage = "Address line cannot exceed 500 characters")]
        public string AddressLine { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters")]
        [Phone(ErrorMessage = "Invalid phone format")]
        public string Phone { get; set; } = string.Empty;
    }

    public class UpdateAddressDto
    {
        [Required(ErrorMessage = "Address line is required")]
        [StringLength(500, ErrorMessage = "Address line cannot exceed 500 characters")]
        public string AddressLine { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters")]
        [Phone(ErrorMessage = "Invalid phone format")]
        public string Phone { get; set; } = string.Empty;
    }
}