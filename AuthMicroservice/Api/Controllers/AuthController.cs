using AuthMicroservice.Service.DTOs;
using AuthMicroservice.Service.Interfaces;
using AuthMicroservice.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Serilog.Context;

using Microsoft.AspNetCore.Mvc;

namespace AuthMicroservice.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AuthLogService _logService;

        public AuthController(IAuthService authService, AuthLogService logService)
        {
            _authService = authService;
            _logService = logService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();

            if (result.accessToken == null || result.refreshToken == null)
            {
                await _logService.LogAsync(new AuthLog
                {
                    CorrelationId = correlationId,
                    Action = "Register",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = "Bu email zaten kayıtlı.",
                    Description = new BsonDocument
                    {
                        { "FirstName", dto.FirstName ?? "" },
                        { "LastName", dto.LastName ?? "" },
                        { "Email", dto.Email }
                    }
                });
                return BadRequest("Bu email zaten kayıtlı.");
            }

            await _logService.LogAsync(new AuthLog
            {
                CorrelationId = correlationId,
                Action = "Register",
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = "Kayıt başarılı.",
                Description = new BsonDocument
                {
                    { "FirstName", dto.FirstName ?? "" },
                    { "LastName", dto.LastName ?? "" },
                    { "Email", dto.Email }
                }
            });

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
            var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();

            if (result.accessToken == null || result.refreshToken == null)
            {
                await _logService.LogAsync(new AuthLog
                {
                    CorrelationId = correlationId,
                    Action = "Login",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = "Email veya şifre hatalı.",
                    Description = new BsonDocument
                    {
                        { "Email", dto.Email }
                    }
                });
                return Unauthorized("Email veya şifre hatalı.");
            }

            await _logService.LogAsync(new AuthLog
            {
                CorrelationId = correlationId,
                Action = "Login",
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = "Login başarılı.",
                Description = new BsonDocument
                {
                    { "Email", dto.Email }
                }
            });

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
            var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();

            if (result.accessToken == null)
            {
                await _logService.LogAsync(new AuthLog
                {
                    CorrelationId = correlationId,
                    Action = "RefreshToken",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = "Refresh token geçersiz veya süresi dolmuş.",
                    Description = new BsonDocument { }
                });
                return Unauthorized("Refresh token geçersiz veya süresi dolmuş.");
            }

            await _logService.LogAsync(new AuthLog
            {
                CorrelationId = correlationId,
                Action = "RefreshToken",
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = "Refresh token başarıyla yenilendi.",
                Description = new BsonDocument { }
            });

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
            var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();

            if (!success)
            {
                await _logService.LogAsync(new AuthLog
                {
                    CorrelationId = correlationId,
                    Action = "Logout",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = "Geçersiz refresh token.",
                    Description = new BsonDocument { }
                });
                return BadRequest("Geçersiz refresh token.");
            }

            await _logService.LogAsync(new AuthLog
            {
                CorrelationId = correlationId,
                Action = "Logout",
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = "Çıkış yapıldı.",
                Description = new BsonDocument { }
            });

            return Ok("Çıkış yapıldı.");
        }

        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] string token)
        {
            var isValid = _authService.IsTokenValid(token);
            var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();

            _logService.LogAsync(new AuthLog
            {
                CorrelationId = correlationId,
                Action = "ValidateToken",
                Timestamp = DateTime.UtcNow,
                Status = isValid ? "Success" : "Fail",
                Message = isValid ? "Token geçerli." : "Token geçersiz.",
                Description = new BsonDocument { }
            });

            return Ok(new { IsValid = isValid });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _authService.GetUsersAsync();
            var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();

            await _logService.LogAsync(new AuthLog
            {
                CorrelationId = correlationId,
                Action = "GetUsers",
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = "Kullanıcı listesi çekildi.",
                Description = new BsonDocument { { "Count", users.Count() } }
            });

            return Ok(users);
        }

        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var result = await _authService.DeleteUserAsync(userId);
            var correlationId = Request.Headers["X-Correlation-Id"].FirstOrDefault();

            if (!result)
            {
                await _logService.LogAsync(new AuthLog
                {
                    CorrelationId = correlationId,
                    Action = "DeleteUser",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = "Kullanıcı bulunamadı.",
                    Description = new BsonDocument { { "UserId", userId } }
                });
                return NotFound("Kullanıcı bulunamadı.");
            }

            await _logService.LogAsync(new AuthLog
            {
                CorrelationId = correlationId,
                Action = "DeleteUser",
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = "Kullanıcı silindi.",
                Description = new BsonDocument { { "UserId", userId } }
            });

            return Ok("Kullanıcı silindi.");
        }
    }
}
