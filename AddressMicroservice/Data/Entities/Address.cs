using System.ComponentModel.DataAnnotations;

namespace AddressMicroservice.Data.Entities
{
    public class Address
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string AddressLine { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]  
        public string Phone { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}