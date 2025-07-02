// Data/Entities/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthMicroservice.Data.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }
        [Required]
        public required string Email { get; set; }

        [Required]
        public required string PasswordHash { get; set; }  // Şifre hashlenmiş şekilde saklanacak

        public string Role { get; set; } = "User"; 
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
