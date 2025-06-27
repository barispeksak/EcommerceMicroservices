namespace UserMicroservice.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string? Fname { get; set; }
        public string? Lname { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime Dob { get; set; }
    }
}
