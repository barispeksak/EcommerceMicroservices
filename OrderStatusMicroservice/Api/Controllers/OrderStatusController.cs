using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using OrderStatusMicroservice.Data.Dtos;
using OrderStatusMicroservice.Models;
using OrderStatusMicroservice.Services.Interfaces;
using OrderStatusMicroservice.Services.Logging;

namespace OrderStatusMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderStatusController : ControllerBase
{
    private readonly IOrderStatusService _service;
    private readonly OrderStatusActionLogger _logger;

    public OrderStatusController(IOrderStatusService service, OrderStatusActionLogger logger)
    {
        _service = service;
        _logger = logger;
    }

    private static BsonDocument WrapObject(object? o) =>
        o is null ? new BsonDocument { { "msg", "null" } } : BsonDocument.Parse(JsonSerializer.Serialize(o));

    private string? GetCorrelationId() =>
        Request.Headers["X-Correlation-Id"].FirstOrDefault();

    private string? GetPerformedByEmail() =>
        Request.Headers["X-User-Email"].FirstOrDefault();

    // GET: api/orderstatus
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        await _logger.LogAsync(new OrderStatusActionLog
        {
            CorrelationId = GetCorrelationId(),
            Action = "GetAll",
            Status = "Success",
            Message = "Tüm OrderStatus kayıtları getirildi.",
            Timestamp = DateTime.UtcNow,
            PerformedByEmail = GetPerformedByEmail(),
            Description = WrapObject(new { Count = result.Count() })
        });

        return Ok(result);
    }

    // GET: api/orderstatus/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var existing = await _service.GetByIdAsync(id);

        if (existing == null)
        {
            string msg = $"OrderStatus bulunamadı: {id}";
            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "GetById",
                Status = "Fail",
                Message = msg,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(new { OrderStatusId = id })
            });

            return NotFound(new { message = msg });
        }

        await _logger.LogAsync(new OrderStatusActionLog
        {
            CorrelationId = GetCorrelationId(),
            Action = "GetById",
            Status = "Success",
            Message = "OrderStatus getirildi.",
            Timestamp = DateTime.UtcNow,
            PerformedByEmail = GetPerformedByEmail(),
            Description = WrapObject(existing)
        });

        return Ok(existing);
    }

    // POST: api/orderstatus
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderStatusDto dto)
    {
        if (!ModelState.IsValid)
        {
            string msg = "Geçersiz veri gönderildi.";
            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "Create",
                Status = "Fail",
                Message = msg,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(dto)
            });
            return BadRequest(new { message = msg });
        }

        try
        {
            // Gerekirse burada ilişkili başka bir ID kontrolü ekleyebilirsin
            var created = await _service.CreateAsync(dto);

            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "Create",
                Status = "Success",
                Message = "OrderStatus oluşturuldu.",
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(new { Request = dto, CreatedId = created.Id })
            });

            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "Create",
                Status = "Fail",
                Message = ex.Message,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(dto)
            });
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "Create",
                Status = "Fail",
                Message = "OrderStatus oluşturulamadı.",
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(new { Request = dto, Exception = ex.Message })
            });
            return StatusCode(500, new { message = "OrderStatus oluşturulamadı." });
        }
    }

    // PUT: api/orderstatus/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        if (id != dto.Id)
        {
            string msg = "ID uyuşmuyor.";
            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "Update",
                Status = "Fail",
                Message = msg,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(new { Id = id, Request = dto })
            });
            return BadRequest(new { message = msg });
        }

        // ÖNCE ID KONTROLÜ!
        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
        {
            string msg = $"OrderStatus bulunamadı: {id}";
            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "Update",
                Status = "Fail",
                Message = msg,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(new { Id = id })
            });
            return NotFound(new { message = msg });
        }

        try
        {
            await _service.UpdateAsync(dto);

            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "Update",
                Status = "Success",
                Message = "OrderStatus güncellendi.",
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(dto)
            });

            return NoContent();
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "Update",
                Status = "Fail",
                Message = "OrderStatus güncellenemedi.",
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(new { Id = id, Request = dto, Exception = ex.Message })
            });
            return StatusCode(500, new { message = "OrderStatus güncellenemedi." });
        }
    }

    // DELETE: api/orderstatus/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
        {
            string msg = $"OrderStatus bulunamadı: {id}";
            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "Delete",
                Status = "Fail",
                Message = msg,
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(new { Id = id })
            });
            return NotFound(new { message = msg });
        }

        try
        {
            await _service.DeleteAsync(id);

            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "Delete",
                Status = "Success",
                Message = "OrderStatus silindi.",
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(new { Id = id })
            });

            return NoContent();
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new OrderStatusActionLog
            {
                CorrelationId = GetCorrelationId(),
                Action = "Delete",
                Status = "Fail",
                Message = "OrderStatus silinemedi.",
                Timestamp = DateTime.UtcNow,
                PerformedByEmail = GetPerformedByEmail(),
                Description = WrapObject(new { Id = id, Exception = ex.Message })
            });
            return StatusCode(500, new { message = "OrderStatus silinemedi." });
        }
    }
}
