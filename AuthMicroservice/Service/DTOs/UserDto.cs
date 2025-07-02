namespace AuthMicroservice.Service.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public required string Email { get; set; }

        public string? RefreshToken { get; set; }
        public required string PasswordHash { get; set; }
    }
}