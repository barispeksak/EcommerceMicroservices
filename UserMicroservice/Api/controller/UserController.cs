using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Dtos;
using UserMicroservice.Service.Interfaces;
using UserMicroservice.Models;
using UserMicroservice.Service.Services;
using System.Text.Json;
using MongoDB.Bson;

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

    // ------------ Yardımcılar ------------

    private static BsonDocument WrapString(string msg) =>
        new() { { "msg", msg } };

    private static BsonDocument WrapObject(object? obj) =>
        obj is null
            ? new BsonDocument { { "msg", "null" } }
            : BsonDocument.Parse(JsonSerializer.Serialize(obj));

    // JWT + Gateway header’dan e-posta çek
    private string GetPerformedByEmail()
    {
        var email = HttpContext.Request.Headers["X-User-Email"].FirstOrDefault();
        if (string.IsNullOrEmpty(email))
        {
            email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
        }
        return email ?? "anonymous";
    }

    // ------------ CRUD ------------

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _service.GetAllUsersAsync();

        await _logService.LogAsync(new UserActionLog
        {
            Action = "GetAll",
            Level = "Info",
            Message = "Tüm kullanıcılar listelendi.",
            CorrelationId = HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            Description = WrapString($"Toplam kullanıcı sayısı: {users.Count()}"),
            PerformedByEmail = GetPerformedByEmail()
        });

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var user = await _service.GetUserByIdAsync(id);

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
                ? WrapString($"Id: {id} ile kullanıcı bulunamadı.")
                : WrapObject(user),
            PerformedByEmail = GetPerformedByEmail()
        });

        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            await _logService.LogAsync(new UserActionLog
            {
                Action = "Create",
                Level = "Error",
                Message = "Geçersiz model ile kullanıcı oluşturulmak istendi.",
                CorrelationId = HttpContext.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                Description = new BsonDocument
                {
                    { "modelState", JsonSerializer.Serialize(ModelState) },
                    { "dto", JsonSerializer.Serialize(dto) }
                },
                PerformedByEmail = GetPerformedByEmail()
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
                Description = WrapObject(result),
                PerformedByEmail = GetPerformedByEmail()
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
                Description = new BsonDocument
                {
                    { "exception", ex.Message },
                    { "stackTrace", ex.StackTrace ?? string.Empty }
                },
                PerformedByEmail = GetPerformedByEmail()
            });
            return StatusCode(500, "Kullanıcı oluşturulurken bir hata oluştu.");
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update(UserDto dto)
    {
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
                Description = new BsonDocument
                {
                    { "modelState", JsonSerializer.Serialize(ModelState) },
                    { "dto", JsonSerializer.Serialize(dto) }
                },
                PerformedByEmail = GetPerformedByEmail()
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
                    ? WrapObject(dto)
                    : WrapString("Kullanıcı bulunamadı veya güncellenemedi."),
                PerformedByEmail = GetPerformedByEmail()
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
                Description = new BsonDocument
                {
                    { "exception", ex.Message },
                    { "stackTrace", ex.StackTrace ?? string.Empty }
                },
                PerformedByEmail = GetPerformedByEmail()
            });
            return StatusCode(500, "Kullanıcı güncellenirken bir hata oluştu.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var user = await _service.GetUserByIdAsync(id);
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
                    ? WrapObject(user != null ? (object)user : new { Id = id })
                    : WrapString($"Kullanıcı {id} bulunamadı veya silinemedi."),
                PerformedByEmail = GetPerformedByEmail()
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
                Description = new BsonDocument
                {
                    { "exception", ex.Message },
                    { "stackTrace", ex.StackTrace ?? string.Empty }
                },
                PerformedByEmail = GetPerformedByEmail()
            });
            return StatusCode(500, "Kullanıcı silinirken bir hata oluştu.");
        }
    }
}
