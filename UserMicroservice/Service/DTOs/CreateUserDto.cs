// Dtos/CreateUserDto.cs
namespace UserMicroservice.Dtos
{
    public record CreateUserDto(
        string Fname,
        string Lname,
        string Email,
        string Phone,
        DateTime Dob
    );
}
