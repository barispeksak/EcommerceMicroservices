using AuthMicroservice.Service.DTOs;
using AuthMicroservice.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthMicroservice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            if (result.accessToken == null || result.refreshToken == null)
                return BadRequest("Bu email zaten kayıtlı.");

            return Ok(new
            {
                AccessToken = result.accessToken,
                RefreshToken = result.refreshToken
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            if (result.accessToken == null || result.refreshToken == null)
                return Unauthorized("Email veya şifre hatalı.");

            return Ok(new
            {
                AccessToken = result.accessToken,
                RefreshToken = result.refreshToken
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);
            if (result.accessToken == null)
                return Unauthorized("Refresh token geçersiz veya süresi dolmuş.");

            return Ok(new
            {
                AccessToken = result.accessToken,
                RefreshToken = result.refreshToken
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            var success = await _authService.LogoutAsync(refreshToken);
            if (!success)
                return BadRequest("Geçersiz refresh token.");

            return Ok("Çıkış yapıldı.");
        }

        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] string token)
        {
            var isValid = _authService.IsTokenValid(token);
            return Ok(new { IsValid = isValid });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _authService.GetUsersAsync();
            return Ok(users);
        }

                [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var result = await _authService.DeleteUserAsync(userId);
            if (!result)
                return NotFound("Kullanıcı bulunamadı.");

            return Ok("Kullanıcı silindi.");
        }
    }
}
