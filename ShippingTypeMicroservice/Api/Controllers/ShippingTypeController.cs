using Microsoft.AspNetCore.Mvc;
using ShippingTypeMicroservice.Data.Dtos;
using ShippingTypeMicroservice.Services.Interfaces;
using ShippingTypeMicroservice.Service.Logging;
using ShippingTypeMicroservice.Models;
using MongoDB.Bson;
using System.Text.Json;

namespace ShippingTypeMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShippingTypeController : ControllerBase
{
    private readonly IShippingTypeService _service;
    private readonly ShippingActionLogger _logger;

    public ShippingTypeController(IShippingTypeService service, ShippingActionLogger logger)
    {
        _service = service;
        _logger = logger;
    }

    // --- Helper'lar ---
    private static BsonDocument WrapString(string msg) =>
        new() { { "msg", msg } };

    private static BsonDocument WrapObject(object? obj) =>
        obj is null
            ? new BsonDocument { { "msg", "null" } }
            : BsonDocument.Parse(JsonSerializer.Serialize(obj));

    private string GetCorrelationId() =>
        HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? HttpContext.TraceIdentifier;

    private string GetPerformedByEmail()
    {
        var email = HttpContext.Request.Headers["X-User-Email"].FirstOrDefault();
        return email ?? "anonymous";
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cid = GetCorrelationId();
        var performedBy = GetPerformedByEmail();
        try
        {
            var result = await _service.GetAllAsync();

            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "GetAll",
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = "Tüm gönderim tipleri listelendi.",
                Description = WrapString($"Toplam gönderim tipi: {result.Count()}")
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "GetAll",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "Gönderim tipleri listelenemedi.",
                Description = WrapString($"Hata: {ex.Message}")
            });
            return StatusCode(500, "Sunucu hatası.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var cid = GetCorrelationId();
        var performedBy = GetPerformedByEmail();

        try
        {
            var result = await _service.GetByIdAsync(id);

            if (result == null)
            {
                await _logger.LogAsync(new ShippingActionLog
                {
                    CorrelationId = cid,
                    PerformedByEmail = performedBy,
                    Action = "GetById",
                    Timestamp = DateTime.UtcNow,
                    Status = "Fail",
                    Message = "Gönderim tipi bulunamadı.",
                    Description = WrapString($"Id: {id} ile gönderim tipi bulunamadı.")
                });

                return NotFound("Gönderim tipi bulunamadı.");
            }

            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "GetById",
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = "Gönderim tipi getirildi.",
                Description = WrapObject(result)
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "GetById",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "Gönderim tipi getirilemedi.",
                Description = WrapString($"Hata: {ex.Message}")
            });
            return StatusCode(500, "Sunucu hatası.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShippingTypeDto dto)
    {
        var cid = GetCorrelationId();
        var performedBy = GetPerformedByEmail();

        if (!ModelState.IsValid)
        {
            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "Create",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "Model doğrulaması geçersiz.",
                Description = WrapObject(dto)
            });
            return BadRequest(ModelState);
        }

        try
        {
            var created = await _service.CreateAsync(dto);

            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "Create",
                Timestamp = DateTime.UtcNow,
                Status = "Success",
                Message = "Yeni gönderim tipi oluşturuldu.",
                Description = WrapObject(dto)
            });

            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "Create",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "Gönderim tipi oluşturulamadı.",
                Description = WrapString($"Hata: {ex.Message}")
            });
            return StatusCode(500, "Sunucu hatası.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShippingTypeDto dto)
    {
        var cid = GetCorrelationId();
        var performedBy = GetPerformedByEmail();

        if (id != dto.Id)
        {
            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "Update",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "ID uyuşmuyor.",
                Description = WrapString($"URL id: {id}, Body id: {dto.Id}")
            });
            return BadRequest("ID uyuşmuyor.");
        }

        try
        {
            var success = await _service.UpdateAsync(dto);

            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "Update",
                Timestamp = DateTime.UtcNow,
                Status = success ? "Success" : "Fail",
                Message = success ? "Gönderim tipi güncellendi." : "Gönderim tipi bulunamadı.",
                Description = WrapObject(dto)
            });

            return success ? NoContent() : NotFound("Gönderim tipi bulunamadı.");
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "Update",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "Gönderim tipi güncellenemedi.",
                Description = WrapString($"Hata: {ex.Message}")
            });
            return StatusCode(500, "Sunucu hatası.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cid = GetCorrelationId();
        var performedBy = GetPerformedByEmail();

        try
        {
            var success = await _service.DeleteAsync(id);

            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "Delete",
                Timestamp = DateTime.UtcNow,
                Status = success ? "Success" : "Fail",
                Message = success ? "Gönderim tipi silindi." : "Gönderim tipi bulunamadı.",
                Description = WrapString($"Id: {id}")
            });

            return success ? NoContent() : NotFound("Gönderim tipi bulunamadı.");
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new ShippingActionLog
            {
                CorrelationId = cid,
                PerformedByEmail = performedBy,
                Action = "Delete",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "Gönderim tipi silinemedi.",
                Description = WrapString($"Hata: {ex.Message}")
            });
            return StatusCode(500, "Sunucu hatası.");
        }
    }
}
