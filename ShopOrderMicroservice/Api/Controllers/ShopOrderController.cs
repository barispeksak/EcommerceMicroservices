using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using ShopOrderMicroservice.Data.Dtos;
using ShopOrderMicroservice.Services.Interfaces;
using ShopOrderMicroservice.Services.Logging;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopOrderController : ControllerBase
{
    private readonly IShopOrderService _service;
    private readonly ShopOrderActionLogger _logger;

    public ShopOrderController(IShopOrderService service, ShopOrderActionLogger logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShopOrderDto dto)
    {
        var result = await _service.CreateAsync(dto);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        if (result == null)
        {
            await _logger.LogAsync(new ShopOrderActionLog
            {
                CorrelationId = cid,
                Action = "Create",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "Sipariş oluşturulamadı.",
                Description = new BsonDocument { { "UserId", dto.UserId } }
            });

            return BadRequest("Sipariş oluşturulamadı.");
        }

        await _logger.LogAsync(new ShopOrderActionLog
        {
            CorrelationId = cid,
            Action = "Create",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "Sipariş oluşturuldu.",
            Description = new BsonDocument { { "OrderId", result.Id }, { "UserId", dto.UserId } }
        });

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new ShopOrderActionLog
        {
            CorrelationId = cid,
            Action = "GetAll",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "Tüm siparişler getirildi.",
            Description = new BsonDocument { { "Count", result.Count() } }
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        if (result == null)
        {
            await _logger.LogAsync(new ShopOrderActionLog
            {
                CorrelationId = cid,
                Action = "GetById",
                Timestamp = DateTime.UtcNow,
                Status = "Fail",
                Message = "Sipariş bulunamadı.",
                Description = new BsonDocument { { "OrderId", id } }
            });

            return NotFound();
        }

        await _logger.LogAsync(new ShopOrderActionLog
        {
            CorrelationId = cid,
            Action = "GetById",
            Timestamp = DateTime.UtcNow,
            Status = "Success",
            Message = "Sipariş getirildi.",
            Description = new BsonDocument { { "OrderId", id } }
        });

        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShopOrderDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new ShopOrderActionLog
        {
            CorrelationId = cid,
            Action = "Update",
            Timestamp = DateTime.UtcNow,
            Status = success ? "Success" : "Fail",
            Message = success ? "Sipariş güncellendi." : "Güncelleme başarısız.",
            Description = new BsonDocument { { "OrderId", id } }
        });

        return success ? NoContent() : BadRequest("Güncelleme başarısız.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        await _logger.LogAsync(new ShopOrderActionLog
        {
            CorrelationId = cid,
            Action = "Delete",
            Timestamp = DateTime.UtcNow,
            Status = success ? "Success" : "Fail",
            Message = success ? "Sipariş silindi." : "Sipariş bulunamadı.",
            Description = new BsonDocument { { "OrderId", id } }
        });

        return success ? NoContent() : NotFound();
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var result = await _service.GetByUserIdAsync(userId);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        var status = result.Any() ? "Success" : "Fail";
        var message = result.Any() ? "Kullanıcıya ait siparişler getirildi." : "Kullanıcıya ait sipariş bulunamadı.";

        await _logger.LogAsync(new ShopOrderActionLog
        {
            CorrelationId = cid,
            Action = "GetByUserId",
            Timestamp = DateTime.UtcNow,
            Status = status,
            Message = message,
            Description = new BsonDocument { { "UserId", userId }, { "Count", result.Count() } }
        });

        return result.Any() ? Ok(result) : NotFound($"Kullanıcı {userId} için sipariş bulunamadı.");
    }

    [HttpGet("daterange")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var result = await _service.GetByDateRangeAsync(start, end);
        var cid = Request.Headers["X-Correlation-Id"].FirstOrDefault();

        var status = result.Any() ? "Success" : "Fail";
        var message = result.Any() ? "Tarih aralığındaki siparişler getirildi." : "Tarih aralığında sipariş bulunamadı.";

        await _logger.LogAsync(new ShopOrderActionLog
        {
            CorrelationId = cid,
            Action = "GetByDateRange",
            Timestamp = DateTime.UtcNow,
            Status = status,
            Message = message,
            Description = new BsonDocument
            {
                { "Start", start },
                { "End", end },
                { "Count", result.Count() }
            }
        });

        return result.Any() ? Ok(result) : NotFound("Belirtilen tarih aralığında sipariş bulunamadı.");
    }
}
