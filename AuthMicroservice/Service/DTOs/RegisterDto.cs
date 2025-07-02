// Service/DTOs/RegisterDto.cs
namespace AuthMicroservice.Service.DTOs
{
    public class RegisterDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }  
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}