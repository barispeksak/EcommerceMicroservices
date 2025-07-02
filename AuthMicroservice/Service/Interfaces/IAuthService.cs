using AuthMicroservice.Service.DTOs;

namespace AuthMicroservice.Service.Interfaces
{
    public interface IAuthService
    {
        Task<(string? accessToken, string? refreshToken)> RegisterAsync(RegisterDto dto);
        Task<(string? accessToken, string? refreshToken)> LoginAsync(LoginDto dto);
        Task<(string? accessToken, string? refreshToken)> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string refreshToken);
        bool IsTokenValid(string token);
        Task<List<UserDto>> GetUsersAsync();
    }
}
