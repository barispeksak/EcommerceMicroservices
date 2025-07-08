using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Dtos;
using UserMicroservice.Service.Interfaces;
using UserMicroservice.Models;
using UserMicroservice.Service.Services;
using System.Text.Json;

namespace UserMicroservice.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _service;
    private readonly UserActionLogger _logService;

    public UserController(IUserService service, UserActionLogger logService)
    {
        _service = service;
        _logService = logService;
    }

    // JWT'den kim, id, email, name çek
    private (string id, string email, string name) GetPerformedBy()
    {
        // Öncelikle Gateway'in gönderdiği header'ları kontrol et
        var id = HttpContext.Request.Headers["X-User-Id"].FirstOrDefault();
        var email = HttpContext.Request.Headers["X-User-Email"].FirstOrDefault();
        var name = HttpContext.Request.Headers["X-User-Name"].FirstOrDefault();

        // Eğer header yoksa, fallback olarak JWT token içinden al
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(email))
        {
            id = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "anonymous";
            email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? "anonymous";
            name = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? "anonymous";
        }

        return (id ?? "anonymous", email ?? "anonymous", name ?? "anonymous");
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _service.GetAllUsersAsync();
        var (performedById, performedByEmail, performedByName) = GetPerformedBy();

        await _logService.LogAsync(new UserActionLog
        {
            Action = "GetAll",
            Level = "Info",
            Message = "Tüm kullanıcılar listelendi.",
            CorrelationId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Description = $"Toplam kullanıcı sayısı: {users.Count()}",
            PerformedById = performedById,
            PerformedByEmail = performedByEmail,
            PerformedByName = performedByName
        });

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _service.GetUserByIdAsync(id);
        var (performedById, performedByEmail, performedByName) = GetPerformedBy();

        await _logService.LogAsync(new UserActionLog
        {
            Action = "GetById",
            Level = user == null ? "Warn" : "Info",
            Message = user == null ? "Kullanıcı bulunamadı." : "Kullanıcı getirildi.",
            CorrelationId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            UserId = (user != null ? user.Id.ToString() : id.ToString()),
            UserEmail = user?.Email,
            UserName = user == null ? null : $"{user.Fname} {user.Lname}",
            UserPhone = user?.Phone,
            UserDob = user?.Dob,
            Description = user == null
                ? $"Id: {id} ile kullanıcı bulunamadı."
                : $"User: {JsonSerializer.Serialize(user)}",
            PerformedById = performedById,
            PerformedByEmail = performedByEmail,
            PerformedByName = performedByName
        });

        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        var (performedById, performedByEmail, performedByName) = GetPerformedBy();

        if (!ModelState.IsValid)
        {
            await _logService.LogAsync(new UserActionLog
            {
                Action = "Create",
                Level = "Error",
                Message = "Geçersiz model ile kullanıcı oluşturulmak istendi.",
                CorrelationId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                Description = $"ModelState: {JsonSerializer.Serialize(ModelState)} DTO: {JsonSerializer.Serialize(dto)}",
                PerformedById = performedById,
                PerformedByEmail = performedByEmail,
                PerformedByName = performedByName
            });
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _service.CreateUserAsync(dto);

            await _logService.LogAsync(new UserActionLog
            {
                Action = "Create",
                Level = "Info",
                Message = "Yeni kullanıcı başarıyla oluşturuldu.",
                CorrelationId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                UserId = result.Id.ToString(),
                UserEmail = result.Email,
                UserName = $"{result.Fname} {result.Lname}",
                UserPhone = result.Phone,
                UserDob = result.Dob,
                Description = $"User created: {JsonSerializer.Serialize(result)}",
                PerformedById = performedById,
                PerformedByEmail = performedByEmail,
                PerformedByName = performedByName
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            await _logService.LogAsync(new UserActionLog
            {
                Action = "Create",
                Level = "Error",
                Message = "Kullanıcı oluşturulurken hata oluştu.",
                CorrelationId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                UserEmail = dto.Email,
                Description = $"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}",
                PerformedById = performedById,
                PerformedByEmail = performedByEmail,
                PerformedByName = performedByName
            });
            return StatusCode(500, "Kullanıcı oluşturulurken bir hata oluştu.");
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update(UserDto dto)
    {
        var (performedById, performedByEmail, performedByName) = GetPerformedBy();

        if (!ModelState.IsValid)
        {
            await _logService.LogAsync(new UserActionLog
            {
                Action = "Update",
                Level = "Error",
                Message = "Geçersiz model ile kullanıcı güncellemesi denendi.",
                CorrelationId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                UserId = dto.Id.ToString(),
                UserEmail = dto.Email,
                UserName = $"{dto.Fname} {dto.Lname}",
                UserPhone = dto.Phone,
                UserDob = dto.Dob,
                Description = $"ModelState: {JsonSerializer.Serialize(ModelState)} DTO: {JsonSerializer.Serialize(dto)}",
                PerformedById = performedById,
                PerformedByEmail = performedByEmail,
                PerformedByName = performedByName
            });
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await _service.UpdateUserAsync(dto);

            await _logService.LogAsync(new UserActionLog
            {
                Action = "Update",
                Level = updated ? "Info" : "Warn",
                Message = updated ? "Kullanıcı başarıyla güncellendi." : "Kullanıcı güncellenemedi.",
                CorrelationId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                UserId = dto.Id.ToString(),
                UserEmail = dto.Email,
                UserName = $"{dto.Fname} {dto.Lname}",
                UserPhone = dto.Phone,
                UserDob = dto.Dob,
                Description = updated
                    ? $"User updated: {JsonSerializer.Serialize(dto)}"
                    : $"Kullanıcı bulunamadı veya güncellenemedi. DTO: {JsonSerializer.Serialize(dto)}",
                PerformedById = performedById,
                PerformedByEmail = performedByEmail,
                PerformedByName = performedByName
            });

            return updated ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            await _logService.LogAsync(new UserActionLog
            {
                Action = "Update",
                Level = "Error",
                Message = "Kullanıcı güncellenirken hata oluştu.",
                CorrelationId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                UserId = dto.Id.ToString(),
                UserEmail = dto.Email,
                UserName = $"{dto.Fname} {dto.Lname}",
                UserPhone = dto.Phone,
                UserDob = dto.Dob,
                Description = $"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}",
                PerformedById = performedById,
                PerformedByEmail = performedByEmail,
                PerformedByName = performedByName
            });
            return StatusCode(500, "Kullanıcı güncellenirken bir hata oluştu.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (performedById, performedByEmail, performedByName) = GetPerformedBy();

        try
        {
            var user = await _service.GetUserByIdAsync(id); // Silinen user bilgisi için!
            var result = await _service.DeleteUserAsync(id);

            await _logService.LogAsync(new UserActionLog
            {
                Action = "Delete",
                Level = result ? "Info" : "Warn",
                Message = result ? "Kullanıcı silindi." : "Kullanıcı silinemedi.",
                CorrelationId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                UserId = (user != null ? user.Id.ToString() : id.ToString()),
                UserEmail = user?.Email,
                UserName = user == null ? null : $"{user.Fname} {user.Lname}",
                UserPhone = user?.Phone,
                UserDob = user?.Dob,
                Description = result
                    ? $"Kullanıcı {id} silindi. User info: {JsonSerializer.Serialize(user)}"
                    : $"Kullanıcı {id} bulunamadı veya silinemedi.",
                PerformedById = performedById,
                PerformedByEmail = performedByEmail,
                PerformedByName = performedByName
            });

            return result ? NoContent() : NotFound();
        }
        catch (Exception ex)
        {
            await _logService.LogAsync(new UserActionLog
            {
                Action = "Delete",
                Level = "Error",
                Message = "Kullanıcı silinirken hata oluştu.",
                CorrelationId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                UserId = id.ToString(),
                Description = $"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}",
                PerformedById = performedById,
                PerformedByEmail = performedByEmail,
                PerformedByName = performedByName
            });
            return StatusCode(500, "Kullanıcı silinirken bir hata oluştu.");
        }
    }
}
