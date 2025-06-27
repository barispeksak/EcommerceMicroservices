using System;
using System.ComponentModel.DataAnnotations;

namespace UserMicroservice.Data.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string? Fname { get; set; }

        public string? Lname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        public DateTime Dob { get; set; }
    }
}
